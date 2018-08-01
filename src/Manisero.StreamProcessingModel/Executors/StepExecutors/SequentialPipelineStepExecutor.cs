using System;
using System.Threading;
using Manisero.StreamProcessingModel.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.Executors.StepExecutors
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
                    block.Body(input);

                    cancellation.ThrowIfCancellationRequested();
                }

                batchNumber++;
                StepExecutionUtils.ReportProgress(batchNumber, step.ExpectedInputBatchesCount, progress);
            }
        }
    }
}
