using Manisero.StreamProcessingModel.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.Executors.StepExecutors
{
    public class BasicStepExecutor : ITaskStepExecutor<BasicTaskStep>
    {
        public void Execute(BasicTaskStep step)
        {
            step.Body();
        }
    }
}
