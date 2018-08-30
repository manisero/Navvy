using System;
using System.Threading;
using Manisero.StreamProcessingModel.Core.Models;

namespace Manisero.StreamProcessingModel.Samples.Utils
{
    public static class TaskDescriptionUtils
    {
        public static TaskResult Execute(
            this TaskDescription task,
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
