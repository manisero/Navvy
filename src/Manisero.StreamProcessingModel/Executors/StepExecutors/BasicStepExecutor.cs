using System;
using Manisero.StreamProcessingModel.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.Executors.StepExecutors
{
    public class BasicStepExecutor : ITaskStepExecutor<BasicTaskStep>
    {
        public void Execute(
            BasicTaskStep step,
            IProgress<byte> progress)
        {
            step.Body();
            progress.Report(100);
        }
    }
}
