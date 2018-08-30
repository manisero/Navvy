using Manisero.StreamProcessingModel.Core;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;

namespace Manisero.StreamProcessingModel.Samples.Utils
{
    public static class TaskExecutorFactory
    {
        public static TaskExecutor Create(
            ResolverType resolverType,
            params IExecutionEvents[] events)
            => Create(TaskExecutorResolvers.Get(resolverType), events);

        public static TaskExecutor Create(
            ITaskStepExecutorResolver taskStepExecutorResolver,
            params IExecutionEvents[] events)
            => new TaskExecutor(
                taskStepExecutorResolver,
                new ExecutionEventsBag(events));
    }
}
