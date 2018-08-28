using Manisero.StreamProcessingModel.Core;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;

namespace Manisero.StreamProcessingModel.Samples.Utils
{
    public static class TaskExecutorFactory
    {
        public static TaskExecutor Create(
            ResolverType resolverType)
            => Create(TaskExecutorResolvers.Get(resolverType));

        public static TaskExecutor Create(
            ITaskStepExecutorResolver taskStepExecutorResolver)
            => new TaskExecutor(
                taskStepExecutorResolver,
                new ExecutionEventsBag());
    }
}
