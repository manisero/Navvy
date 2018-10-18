using System.Threading;
using Manisero.Navvy.Core;

namespace Manisero.Navvy
{
    public interface ITaskExecutor
    {
        TaskResult Execute(
            TaskDefinition task,
            CancellationToken? cancellation = null,
            params IExecutionEvents[] events);
    }
}
