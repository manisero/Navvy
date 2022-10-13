using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.Navvy.Core;
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
            var inputBlock = BuildInputBlock(cancellation);

            var materializeAndPostNextBlock = BuildMaterializeAndPostNextBlock(inputEnumerator, inputBlock, step, context, cancellation);
            inputBlock.LinkTo(materializeAndPostNextBlock, new DataflowLinkOptions { PropagateCompletion = true });

            ISourceBlock<PipelineItem<TItem>> previousBlock = materializeAndPostNextBlock;

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
                    PostNextInput(inputBlock, 1);
                    return progressBlock.Completion;
                }
            };
        }

        private void PostNextInput(
            ITargetBlock<int> inputBlock,
            int itemNumber)
        {
            var posted = inputBlock.Post(itemNumber);

            if (!posted)
            {
                throw new UnexpectedNavvyException(
                    $"Dataflow pipeline execution error. Failed to post item {itemNumber} to pipeline. As a workaround, you can switch to sequential pipeline execution (i.e. disable Dataflow execution).");
            }
        }

        private TransformBlock<int, int> BuildInputBlock(
            CancellationToken cancellation)
        {
            return new TransformBlock<int, int>(
                i => i,
                new ExecutionDataflowBlockOptions
                {
                    NameFormat = "Input",
                    BoundedCapacity = -1, // A race condition was observed when BoundedCapacity was set to 1. Next block's "inputBlock.Post()" was rejected at random (probably first block was still "holding" the exact message the next block was already processing).
                    EnsureOrdered = true,
                    CancellationToken = cancellation
                });
        }

        private TransformBlock<int, PipelineItem<TItem>> BuildMaterializeAndPostNextBlock<TItem>(
            IEnumerator<TItem> inputEnumerator,
            ITargetBlock<int> inputBlock,
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            return new TransformBlock<int, PipelineItem<TItem>>(
                i =>
                {
                    var itemStartTs = DateTimeOffset.UtcNow;
                    var sw = Stopwatch.StartNew();

                    bool hasNextItem;

                    try
                    {
                        hasNextItem = inputEnumerator.MoveNext();
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, step.GetInputExceptionData(i));
                    }

                    if (hasNextItem)
                    {
                        var pipelineItem = new PipelineItem<TItem>
                        {
                            Number = i,
                            Item = inputEnumerator.Current,
                            ProcessingStopwatch = sw
                        };

                        var materializationDuration = sw.Elapsed;
                        context.TotalInputMaterializationDuration += materializationDuration;
                        context.Events?.Raise(x => x.OnItemMaterialized(pipelineItem.Number, pipelineItem.Item, itemStartTs, materializationDuration, step, context.StepContext.Task));

                        PostNextInput(inputBlock, pipelineItem.Number + 1);
                        return pipelineItem;
                    }
                    else
                    {
                        sw.Stop();
                        inputBlock.Complete();
                        return null;
                    }
                },
                new ExecutionDataflowBlockOptions
                {
                    NameFormat = "MaterializeAndPostNextBlock",
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
                async x =>
                {
                    if (x == null)
                    {
                        return x;
                    }

                    context.Events?.Raise(e => e.OnBlockStarted(block, x.Number, x.Item, step, context.StepContext.Task));
                    var sw = Stopwatch.StartNew();

                    try
                    {
                        await block.Body(x.Item);
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, block.GetExceptionData(x.Number));
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
                    if (x == null)
                    {
                        return;
                    }

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
    }
}
