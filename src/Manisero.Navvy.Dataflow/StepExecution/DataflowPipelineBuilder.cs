using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Events;

namespace Manisero.Navvy.Dataflow.StepExecution
{
    internal class DataflowPipelineBuilder<TItem>
    {
        private static readonly int DegreeOfParallelism = Environment.ProcessorCount - 1;

        public DataflowPipeline<TItem> Build(
            PipelineTaskStep<TItem> step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            return step.Blocks.Count != 0
                ? BuildNotEmpty(step, context, progress, cancellation)
                : BuildEmpty(step, context, progress, cancellation);
        }

        private DataflowPipeline<TItem> BuildNotEmpty(
            PipelineTaskStep<TItem> step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var events = context.EventsBag.TryGetEvents<PipelineExecutionEvents>();
            
            var firstBlock = ToTransformBlock(step, 0, context, events, cancellation);
            var previousBlock = firstBlock;

            for (var i = 1; i <= step.Blocks.Count - 1; i++)
            {
                var currentBlock = ToTransformBlock(step, i, context, events, cancellation);
                previousBlock.LinkTo(currentBlock, new DataflowLinkOptions { PropagateCompletion = true });

                previousBlock = currentBlock;
            }

            var progressBlock = CreateProgressBlock(step, context, events, progress, cancellation);
            previousBlock.LinkTo(progressBlock, new DataflowLinkOptions { PropagateCompletion = true });

            return new DataflowPipeline<TItem>
            {
                InputBlock = firstBlock,
                Completion = progressBlock.Completion
            };
        }

        private DataflowPipeline<TItem> BuildEmpty(
            PipelineTaskStep<TItem> step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var events = context.EventsBag.TryGetEvents<PipelineExecutionEvents>();

            var progressBlock = CreateProgressBlock(step, context, events, progress, cancellation);

            return new DataflowPipeline<TItem>
            {
                InputBlock = progressBlock,
                Completion = progressBlock.Completion
            };
        }

        private TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>> ToTransformBlock(
            PipelineTaskStep<TItem> step,
            int blockIndex,
            TaskStepExecutionContext context,
            PipelineExecutionEvents events,
            CancellationToken cancellation)
        {
            var block = step.Blocks[blockIndex];
            var maxDegreeOfParallelism = block.Parallel ? DegreeOfParallelism : 1;

            return new TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>>(
                x =>
                {
                    events?.OnBlockStarted(block, x.Number, x.Item, step, context.Task);
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
                    events?.OnBlockEnded(block, x.Number, x.Item, step, context.Task, sw.Elapsed);

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

        private ActionBlock<PipelineItem<TItem>> CreateProgressBlock(
            PipelineTaskStep<TItem> step,
            TaskStepExecutionContext context,
            PipelineExecutionEvents events,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            return new ActionBlock<PipelineItem<TItem>>(
                x =>
                {
                    x.ProcessingStopwatch.Stop();
                    events?.OnItemEnded(x.Number, x.Item, step, context.Task, x.ProcessingStopwatch.Elapsed);
                    PipelineProcessingUtils.ReportProgress(x.Number, step.ExpectedItemsCount, progress);
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
