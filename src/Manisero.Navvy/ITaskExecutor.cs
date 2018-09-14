using System;
using System.Threading;

namespace Manisero.Navvy
{
    public interface ITaskExecutor
    {
        TaskResult Execute(
            TaskDefinition task,
            IProgress<TaskProgress> progress = null,
            CancellationToken? cancellation = null);
    }
}
