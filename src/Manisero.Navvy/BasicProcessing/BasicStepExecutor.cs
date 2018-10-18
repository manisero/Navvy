using System;
using System.Diagnostics;
using System.Threading;
using Manisero.Navvy.Core.Events;
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
            var events = context.EventsBag.TryGetEvents<TaskExecutionEvents>();
            var sw = new Stopwatch();

            try
            {
                step.Body();
            }
            catch (Exception e)
            {
                throw new TaskExecutionException(e, step);
            }

            sw.Stop();
            events?.Raise(x => x.OnStepProgressed(100, sw.Elapsed, step, context.Task));
            progress.Report(100);
            cancellation.ThrowIfCancellationRequested();
        }
    }
}
