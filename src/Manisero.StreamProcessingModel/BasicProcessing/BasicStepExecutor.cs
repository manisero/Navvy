using System;
using System.Threading;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;

namespace Manisero.StreamProcessingModel.BasicProcessing
{
    public class BasicStepExecutor : ITaskStepExecutor<BasicTaskStep>
    {
        public void Execute(
            BasicTaskStep step,
            TaskStepExecutionContext context,
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
