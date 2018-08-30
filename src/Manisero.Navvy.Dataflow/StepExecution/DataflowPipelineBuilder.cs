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
    internal class DataflowPipelineBuilder<TData>
    {
        private static readonly int DegreeOfParallelism = Environment.ProcessorCount - 1;

        public DataflowPipeline<TData> Build(
            PipelineTaskStep<TData> step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            return step.Blocks.Count != 0
                ? BuildNotEmpty(step, context, progress, cancellation)
                : BuildEmpty(step, context, progress, cancellation);
        }

        private DataflowPipeline<TData> BuildNotEmpty(
            PipelineTaskStep<TData> step,
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

            return new DataflowPipeline<TData>
            {
                InputBlock = firstBlock,
                Completion = progressBlock.Completion
            };
        }

        private DataflowPipeline<TData> BuildEmpty(
            PipelineTaskStep<TData> step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var events = context.EventsBag.TryGetEvents<PipelineExecutionEvents>();

            var progressBlock = CreateProgressBlock(step, context, events, progress, cancellation);

            return new DataflowPipeline<TData>
            {
                InputBlock = progressBlock,
                Completion = progressBlock.Completion
            };
        }

        private TransformBlock<DataBatch<TData>, DataBatch<TData>> ToTransformBlock(
            PipelineTaskStep<TData> step,
            int blockIndex,
            TaskStepExecutionContext context,
            PipelineExecutionEvents events,
            CancellationToken cancellation)
        {
            var block = step.Blocks[blockIndex];
            var maxDegreeOfParallelism = block.Parallel ? DegreeOfParallelism : 1;

            return new TransformBlock<DataBatch<TData>, DataBatch<TData>>(
                x =>
                {
                    events?.OnBlockStarted(block, x.Number, x.Data, step, context.Task);
                    var sw = Stopwatch.StartNew();

                    try
                    {
                        block.Body(x.Data);
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, block.GetExceptionData());
                    }

                    sw.Stop();
                    events?.OnBlockEnded(block, x.Number, x.Data, step, context.Task, sw.Elapsed);

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

        private ActionBlock<DataBatch<TData>> CreateProgressBlock(
            PipelineTaskStep<TData> step,
            TaskStepExecutionContext context,
            PipelineExecutionEvents events,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            return new ActionBlock<DataBatch<TData>>(
                x =>
                {
                    x.ProcessingStopwatch.Stop();
                    events?.OnBatchEnded(x.Number, x.Data, step, context.Task, x.ProcessingStopwatch.Elapsed);
                    PipelineProcessingUtils.ReportProgress(x.Number, step.ExpectedInputBatchesCount, progress);
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
