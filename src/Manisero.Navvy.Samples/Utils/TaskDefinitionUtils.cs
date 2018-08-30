using System;
using System.Threading;
using Manisero.Navvy.Core.Models;

namespace Manisero.Navvy.Samples.Utils
{
    public static class TaskDefinitionUtils
    {
        public static TaskResult Execute(
            this TaskDefinition task,
            ResolverType resolverType = ResolverType.Sequential,
            IProgress<TaskProgress> progress = null,
            CancellationTokenSource cancellation = null,
            params IExecutionEvents[] events)
        {
            var executor = TaskExecutorFactory.Create(resolverType, events);

            return executor.Execute(task, progress, cancellation?.Token);
        }
    }
}
