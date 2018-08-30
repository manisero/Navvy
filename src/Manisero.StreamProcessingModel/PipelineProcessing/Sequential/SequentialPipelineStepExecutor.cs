using System;
using System.Diagnostics;
using System.Threading;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;
using Manisero.StreamProcessingModel.PipelineProcessing.Events;

namespace Manisero.StreamProcessingModel.PipelineProcessing.Sequential
{
    internal class SequentialPipelineStepExecutor<TData> : ITaskStepExecutor<PipelineTaskStep<TData>>
    {
        public void Execute(
            PipelineTaskStep<TData> step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var batchNumber = 0;
            var events = context.EventsBag.TryGetEvents<PipelineExecutionEvents>();

            foreach (var input in step.Input)
            {
                batchNumber++;
                events?.OnBatchStarted(batchNumber, input, step, context.TaskDescription);
                var batchSw = Stopwatch.StartNew();

                foreach (var block in step.Blocks)
                {
                    events?.OnBlockStarted(batchNumber, input, block, step, context.TaskDescription);
                    var blockSw = Stopwatch.StartNew();

                    try
                    {
                        block.Body(input);
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, block.GetExceptionData());
                    }

                    blockSw.Stop();
                    events?.OnBlockEnded(batchNumber, input, block, step, context.TaskDescription, blockSw.Elapsed);
                    cancellation.ThrowIfCancellationRequested();
                }

                batchSw.Stop();
                events?.OnBatchEnded(batchNumber, input, step, context.TaskDescription, batchSw.Elapsed);
                PipelineProcessingUtils.ReportProgress(batchNumber, step.ExpectedInputBatchesCount, progress);
            }
        }
    }
}
