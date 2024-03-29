﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.BasicProcessing
{
    internal class BasicStepExecutor : ITaskStepExecutor<BasicTaskStep>
    {
        public async Task Execute(
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
                await step.Body(context.OutcomeSoFar, progress, cancellation);
            }
#pragma warning disable CS0168
            catch (OperationCanceledException e)
#pragma warning restore CS0168
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
