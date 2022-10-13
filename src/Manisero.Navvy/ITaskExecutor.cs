using System;
using System.Threading;
using System.Threading.Tasks;
using Manisero.Navvy.Core;

namespace Manisero.Navvy
{
    /// <summary><see cref="TaskDefinition"/> execution engine. Default implementation can be built by <see cref="TaskExecutorBuilder"/>.</summary>
    public interface ITaskExecutor
    {
        /// <summary>Executes the task. Exceptions thrown during execution are gathered and thrown after the task is finished. If the task is canceled, <see cref="OperationCanceledException"/> is thrown.</summary>
        Task Execute(
            TaskDefinition task,
            CancellationToken? cancellation = null,
            params IExecutionEvents[] events);
    }
}
