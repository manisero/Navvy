using System;
using System.Threading;
using Manisero.StreamProcessingModel.Models;

namespace Manisero.StreamProcessingModel.Executors
{
    public interface ITaskStepExecutor<TTaskStep>
        where TTaskStep : ITaskStep
    {
        void Execute(TTaskStep step, IProgress<byte> progress, CancellationToken cancellation);
    }
}