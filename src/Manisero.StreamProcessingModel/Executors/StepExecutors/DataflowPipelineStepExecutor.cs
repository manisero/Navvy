using System;
using System.Threading.Tasks.Dataflow;
using Manisero.StreamProcessingModel.Executors.StepExecutors.DataflowPipelineStepExecution;
using Manisero.StreamProcessingModel.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.Executors.StepExecutors
{
    public class DataflowPipelineStepExecutor<TData> : ITaskStepExecutor<PipelineTaskStep<TData>>
    {
        private readonly DataflowPipelineBuilder<TData> _dataflowPipelineBuilder = new DataflowPipelineBuilder<TData>();

        public void Execute(
            PipelineTaskStep<TData> step,
            IProgress<byte> progress)
        {
            var pipeline = _dataflowPipelineBuilder.Build(step, progress);
            var batchNumber = 1;

            foreach (var input in step.Input)
            {
                var batch = new DataBatch<TData>
                {
                    Number = batchNumber,
                    Data = input
                };

                pipeline.InputBlock.SendAsync(batch).Wait();
            }

            pipeline.InputBlock.Complete();
            pipeline.Completion.Wait();
        }
    }
}
