using System.Threading;
using System.Threading.Tasks;
using Manisero.Navvy.Core;

namespace Manisero.Navvy
{
    /// <summary><see cref="TaskDefinition"/> execution engine. Default implementation can be built by <see cref="TaskExecutorBuilder"/>.</summary>
    public interface ITaskExecutor
    {
        /// <summary>Executes the task. Exceptions thrown during execution are gathered in <see cref="TaskResult"/>, so remember to check the returned value.</summary>
        Task<TaskResult> Execute(
            TaskDefinition task,
            CancellationToken? cancellation = null,
            params IExecutionEvents[] events);
    }
}
