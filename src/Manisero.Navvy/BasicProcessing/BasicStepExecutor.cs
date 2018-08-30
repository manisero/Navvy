using System;
using System.Threading;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.Core.StepExecution;

namespace Manisero.Navvy.BasicProcessing
{
    internal class BasicStepExecutor : ITaskStepExecutor<BasicTaskStep>
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
