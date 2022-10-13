using System.Threading;
using System.Threading.Tasks;
using Manisero.Navvy.Core;

namespace Manisero.Navvy.Tests.Utils
{
    public static class TaskDefinitionUtils
    {
        public static async Task Execute(
            this TaskDefinition task,
            ResolverType resolverType = ResolverType.Sequential,
            CancellationTokenSource cancellation = null,
            params IExecutionEvents[] events)
        {
            var executor = TaskExecutorFactory.Create(resolverType);
            
            await executor.Execute(task, cancellation?.Token, events);
        }
    }
}
