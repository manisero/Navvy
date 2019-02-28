using System.Threading;
using Manisero.Navvy.Core;

namespace Manisero.Navvy
{
    /// <summary><see cref="TaskDefinition"/> execution engine. Default implementation can be built by <see cref="TaskExecutorBuilder"/>.</summary>
    public interface ITaskExecutor
    {
        /// <summary>Executes the task synchronously. Exceptions thrown during execution are gathered in <see cref="TaskResult"/>, so remember to check the returned value.</summary>
        TaskResult Execute(
            TaskDefinition task,
            CancellationToken? cancellation = null,
            params IExecutionEvents[] events);
    }
}
