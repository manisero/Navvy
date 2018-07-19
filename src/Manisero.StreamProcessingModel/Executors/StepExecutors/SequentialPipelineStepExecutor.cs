using Manisero.StreamProcessingModel.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.Executors.StepExecutors
{
    public class SequentialPipelineStepExecutor<TData> : ITaskStepExecutor<PipelineTaskStep<TData>>
    {
        public void Execute(PipelineTaskStep<TData> step)
        {
            foreach (var input in step.Input)
            {
                foreach (var block in step.Blocks)
                {
                    foreach (var data in input)
                    {
                        block.Body(data);
                    }
                }
            }
        }
    }
}
