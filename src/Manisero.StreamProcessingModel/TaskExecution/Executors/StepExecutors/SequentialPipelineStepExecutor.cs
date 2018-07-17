using Manisero.StreamProcessingModel.TaskExecution.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.TaskExecution.Executors.StepExecutors
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

                    block.OnCompleted();
                }
            }
        }
    }
}
