using Manisero.StreamProcessingModel.TaskExecution.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.TaskExecution.Executors.StepExecutors
{
    public class BasicStepExecutor : ITaskStepExecutor<BasicTaskStep>
    {
        public void Execute(BasicTaskStep step)
        {
            step.Body();
        }
    }
}
