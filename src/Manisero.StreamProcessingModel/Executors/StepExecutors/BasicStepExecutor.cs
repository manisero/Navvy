using System;
using System.Threading;
using Manisero.StreamProcessingModel.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.Executors.StepExecutors
{
    public class BasicStepExecutor : ITaskStepExecutor<BasicTaskStep>
    {
        public void Execute(
            BasicTaskStep step,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            step.Body();

            progress.Report(100);
            cancellation.ThrowIfCancellationRequested();
        }
    }
}
