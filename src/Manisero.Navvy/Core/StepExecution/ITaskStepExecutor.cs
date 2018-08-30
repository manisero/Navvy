using System;
using System.Threading;
using Manisero.Navvy.Core.Models;

namespace Manisero.Navvy.Core.StepExecution
{
    public interface ITaskStepExecutor<TTaskStep>
        where TTaskStep : ITaskStep
    {
        void Execute(
            TTaskStep step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation);
    }
}