using System.Threading.Tasks.Dataflow;
using Manisero.StreamProcessingModel.Executors.StepExecutors.DataflowPipelineStepExecution;
using Manisero.StreamProcessingModel.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.Executors.StepExecutors
{
    public class DataflowPipelineStepExecutor<TData> : ITaskStepExecutor<PipelineTaskStep<TData>>
    {
        private readonly DataflowPipelineBuilder<TData> _dataflowPipelineBuilder = new DataflowPipelineBuilder<TData>();

        public void Execute(PipelineTaskStep<TData> step)
        {
            var pipeline = _dataflowPipelineBuilder.Build(step);
            var batchNumber = 1;

            foreach (var input in step.Input)
            {
                var batch = new DataBatch<TData>
                {
                    Number = batchNumber,
                    Data = input
                };

                pipeline.Item1.SendAsync(batch).Wait();
            }

            pipeline.Item1.Complete();
            pipeline.Item2.Wait();
        }
    }
}
