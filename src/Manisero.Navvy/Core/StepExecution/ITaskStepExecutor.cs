using System;
using System.Threading;
using Manisero.StreamProcessingModel.Core.Models;

namespace Manisero.StreamProcessingModel.Core.StepExecution
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