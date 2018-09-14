using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.Dataflow.StepExecution
{
    internal class DataflowPipelineBuilder
    {
        private static readonly int DegreeOfParallelism = Environment.ProcessorCount - 1;

        public DataflowPipeline<TItem> Build<TItem>(
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            return step.Blocks.Count != 0
                ? BuildNotEmpty(step, context, progress, cancellation)
                : BuildEmpty(step, context, progress, cancellation);
        }

        private DataflowPipeline<TItem> BuildNotEmpty<TItem>(
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var firstBlock = ToTransformBlock(step, 0, context, cancellation);
            var previousBlock = firstBlock;

            for (var i = 1; i <= step.Blocks.Count - 1; i++)
            {
                var currentBlock = ToTransformBlock(step, i, context, cancellation);
                previousBlock.LinkTo(currentBlock, new DataflowLinkOptions { PropagateCompletion = true });

                previousBlock = currentBlock;
            }

            var progressBlock = CreateProgressBlock(step, context, progress, cancellation);
            previousBlock.LinkTo(progressBlock, new DataflowLinkOptions { PropagateCompletion = true });

            return new DataflowPipeline<TItem>
            {
                InputBlock = firstBlock,
                Completion = progressBlock.Completion
            };
        }

        private DataflowPipeline<TItem> BuildEmpty<TItem>(
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var progressBlock = CreateProgressBlock(step, context, progress, cancellation);

            return new DataflowPipeline<TItem>
            {
                InputBlock = progressBlock,
                Completion = progressBlock.Completion
            };
        }

        private TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>> ToTransformBlock<TItem>(
            PipelineTaskStep<TItem> step,
            int blockIndex,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            var block = step.Blocks[blockIndex];
            var maxDegreeOfParallelism = block.Parallel ? DegreeOfParallelism : 1;

            return new TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>>(
                x =>
                {
                    context.Events?.OnBlockStarted(block, x.Number, x.Item, step, context.StepContext.Task);
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
                    context.Events?.OnBlockEnded(block, x.Number, x.Item, step, context.StepContext.Task, sw.Elapsed);

                    return x;
                },
                new ExecutionDataflowBlockOptions
                {
                    NameFormat = block.Name,
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    BoundedCapacity = maxDegreeOfParallelism,
                    EnsureOrdered = true,
                    CancellationToken = cancellation
                });
        }

        private ActionBlock<PipelineItem<TItem>> CreateProgressBlock<TItem>(
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            return new ActionBlock<PipelineItem<TItem>>(
                x =>
                {
                    x.ProcessingStopwatch.Stop();
                    context.Events?.OnItemEnded(x.Number, x.Item, step, context.StepContext.Task, x.ProcessingStopwatch.Elapsed);
                    PipelineProcessingUtils.ReportProgress(x.Number, step.Input.ExpectedItemsCount, progress);
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
