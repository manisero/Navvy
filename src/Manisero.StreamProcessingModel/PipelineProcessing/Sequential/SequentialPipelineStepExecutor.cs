using System;
using System.Diagnostics;
using System.Threading;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.PipelineProcessing.Sequential
{
    public class SequentialPipelineStepExecutor<TData> : ITaskStepExecutor<PipelineTaskStep<TData>>
    {
        public void Execute(
            PipelineTaskStep<TData> step,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var batchNumber = 0;

            foreach (var input in step.Input)
            {
                batchNumber++;
                PipelineExecutionEvents.BatchStarted(DateTimeUtils.Now);
                var batchSw = Stopwatch.StartNew();

                foreach (var block in step.Blocks)
                {
                    PipelineExecutionEvents.BlockStarted(DateTimeUtils.Now);
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
                    PipelineExecutionEvents.BlockEnded(DateTimeUtils.Now, blockSw.Elapsed);
                    cancellation.ThrowIfCancellationRequested();
                }

                batchSw.Stop();
                PipelineExecutionEvents.BatchEnded(DateTimeUtils.Now, batchSw.Elapsed);
                StepExecutionUtils.ReportProgress(batchNumber, step.ExpectedInputBatchesCount, progress);
            }
        }
    }
}
