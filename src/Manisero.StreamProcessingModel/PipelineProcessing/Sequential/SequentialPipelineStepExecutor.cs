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
                foreach (var block in step.Blocks)
                {
                    PipelineExecutionEvents.BlockStarted(
                        new BlockStartedEvent
                        {
                            Timestamp = DateTimeUtils.Now
                        });

                    var sw = Stopwatch.StartNew();

                    try
                    {
                        block.Body(input);
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, block.GetExceptionData());
                    }

                    sw.Stop();

                    PipelineExecutionEvents.BlockEnded(
                        new BlockEndedEvent
                        {
                            Timestamp = DateTimeUtils.Now,
                            Duration = sw.Elapsed
                        });

                    cancellation.ThrowIfCancellationRequested();
                }

                batchNumber++;
                StepExecutionUtils.ReportProgress(batchNumber, step.ExpectedInputBatchesCount, progress);
            }
        }
    }
}
