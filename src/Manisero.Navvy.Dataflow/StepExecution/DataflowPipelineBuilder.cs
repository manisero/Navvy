using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.Dataflow.StepExecution
{
    internal class DataflowPipelineBuilder
    {
        public DataflowPipeline Build<TItem>(
            IEnumerator<TItem> inputEnumerator,
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            var inputBlock = BuildInputBlock<TItem>(cancellation);

            var postNextInputBlock = BuildPostNextInputBlock(inputEnumerator, inputBlock, step, context, cancellation);
            inputBlock.LinkTo(postNextInputBlock, new DataflowLinkOptions { PropagateCompletion = true });

            var previousBlock = postNextInputBlock;

            foreach (var block in step.Blocks)
            {
                var currentBlock = BuildProcessingBlock(block, step, context, cancellation);
                previousBlock.LinkTo(currentBlock, new DataflowLinkOptions { PropagateCompletion = true });

                previousBlock = currentBlock;
            }

            var progressBlock = BuildProgressBlock(step, context, cancellation);
            previousBlock.LinkTo(progressBlock, new DataflowLinkOptions { PropagateCompletion = true });

            return new DataflowPipeline
            {
                Execute = () =>
                {
                    PostNextInput(
                        inputEnumerator,
                        1,
                        inputBlock,
                        step,
                        context,
                        cancellation);

                    return progressBlock.Completion;
                }
            };
        }

        private TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>> BuildInputBlock<TItem>(
            CancellationToken cancellation)
        {
            return new TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>>(
                x => x,
                new ExecutionDataflowBlockOptions
                {
                    NameFormat = "Input",
                    BoundedCapacity = 1,
                    EnsureOrdered = true,
                    CancellationToken = cancellation
                });
        }

        private TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>> BuildPostNextInputBlock<TItem>(
            IEnumerator<TItem> inputEnumerator,
            ITargetBlock<PipelineItem<TItem>> inputBlock,
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            return new TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>>(
                x =>
                {
                    PostNextInput(
                        inputEnumerator,
                        x.Number + 1,
                        inputBlock,
                        step,
                        context,
                        cancellation);

                    return x;
                },
                new ExecutionDataflowBlockOptions
                {
                    NameFormat = "PostNextInput",
                    BoundedCapacity = 1,
                    EnsureOrdered = true,
                    CancellationToken = cancellation
                });
        }

        private TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>> BuildProcessingBlock<TItem>(
            PipelineBlock<TItem> block,
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            return new TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>>(
                x =>
                {
                    context.Events?.Raise(e => e.OnBlockStarted(block, x.Number, x.Item, step, context.StepContext.Task));
                    var sw = Stopwatch.StartNew();

                    try
                    {
                        block.Body(x.Item);
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, block.GetExceptionData());
                    }

                    sw.Stop();
                    context.TotalBlockDurations.AddOrUpdate(block.Name, sw.Elapsed, (_, duration) => duration + sw.Elapsed);
                    context.Events?.Raise(e => e.OnBlockEnded(block, x.Number, x.Item, step, context.StepContext.Task, sw.Elapsed));

                    return x;
                },
                new ExecutionDataflowBlockOptions
                {
                    NameFormat = block.Name,
                    MaxDegreeOfParallelism = block.MaxDegreeOfParallelism,
                    BoundedCapacity = block.MaxDegreeOfParallelism,
                    EnsureOrdered = true,
                    CancellationToken = cancellation
                });
        }

        private ActionBlock<PipelineItem<TItem>> BuildProgressBlock<TItem>(
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            return new ActionBlock<PipelineItem<TItem>>(
                x =>
                {
                    x.ProcessingStopwatch.Stop();
                    context.Events?.Raise(e => e.OnItemEnded(x.Number, x.Item, step, context.StepContext.Task, x.ProcessingStopwatch.Elapsed));
                    context.TaskEvents?.Raise(e => e.OnStepProgressed(x.Number, context.ExpectedItemsCount, x.ProcessingStopwatch.Elapsed, step, context.StepContext.Task));
                },
                new ExecutionDataflowBlockOptions
                {
                    NameFormat = "Progress",
                    BoundedCapacity = 1,
                    EnsureOrdered = true,
                    CancellationToken = cancellation
                });
        }

        private void PostNextInput<TItem>(
            IEnumerator<TItem> inputEnumerator,
            int itemNumber,
            ITargetBlock<PipelineItem<TItem>> inputBlock,
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            var itemStartTs = DateTimeOffset.UtcNow;
            var sw = Stopwatch.StartNew();

            if (inputEnumerator.MoveNext())
            {
                var pipelineItem = new PipelineItem<TItem>
                {
                    Number = itemNumber,
                    Item = inputEnumerator.Current,
                    ProcessingStopwatch = sw
                };

                var materializationDuration = sw.Elapsed;
                context.TotalInputMaterializationDuration += materializationDuration;
                context.Events?.Raise(x => x.OnItemMaterialized(pipelineItem.Number, pipelineItem.Item, itemStartTs, materializationDuration, step, context.StepContext.Task));

                inputBlock.Post(pipelineItem);

                cancellation.ThrowIfCancellationRequested();
            }
            else
            {
                sw.Stop();
                inputBlock.Complete();
            }
        }
    }
}
