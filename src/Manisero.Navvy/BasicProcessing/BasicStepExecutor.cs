using System;
using System.Diagnostics;
using System.Threading;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.BasicProcessing
{
    internal class BasicStepExecutor : ITaskStepExecutor<BasicTaskStep>
    {
        public void Execute(
            BasicTaskStep step,
            TaskStepExecutionContext context,
            CancellationToken cancellation)
        {
            var events = context.EventsBag.TryGetEvents<TaskExecutionEvents>();
            var sw = new Stopwatch();
            var progress = new SynchronousProgress<float>(p => events?.Raise(x => x.OnStepProgressed(p, sw.Elapsed, step, context.Task)));

            sw.Start();

            try
            {
                step.Body(progress, cancellation);
            }
            catch (OperationCanceledException e)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new TaskExecutionException(e, step);
            }

            sw.Stop();
            progress.Report(1f);
            cancellation.ThrowIfCancellationRequested();
        }
    }
}
