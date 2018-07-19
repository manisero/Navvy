using System;
using Manisero.StreamProcessingModel.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.Executors.StepExecutors
{
    public class SequentialPipelineStepExecutor<TData> : ITaskStepExecutor<PipelineTaskStep<TData>>
    {
        public void Execute(
            PipelineTaskStep<TData> step,
            IProgress<byte> progress)
        {
            var batchNumber = 0;

            foreach (var input in step.Input)
            {
                foreach (var block in step.Blocks)
                {
                    foreach (var data in input)
                    {
                        block.Body(data);
                    }
                }

                batchNumber++;
                StepExecutionUtils.ReportProgress(batchNumber, step.ExpectedInputBatchesCount, progress);
            }
        }
    }
}
