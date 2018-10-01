using System;
using System.Threading;
using Manisero.Navvy.Core;

namespace Manisero.Navvy
{
    public interface ITaskExecutor
    {
        TaskResult Execute(
            TaskDefinition task,
            IProgress<TaskProgress> progress = null,
            CancellationToken? cancellation = null,
            params IExecutionEvents[] events);
    }
}
