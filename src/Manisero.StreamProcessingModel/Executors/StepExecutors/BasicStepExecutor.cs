using System;
using System.Threading;
using Manisero.StreamProcessingModel.Models;
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
            try
            {
                step.Body();
            }
            catch (Exception e)
            {
                throw new TaskExecutionException(e, step);
            }

            progress.Report(100);
            cancellation.ThrowIfCancellationRequested();
        }
    }
}
