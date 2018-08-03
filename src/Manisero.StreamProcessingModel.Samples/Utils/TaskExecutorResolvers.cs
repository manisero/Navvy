using System.Collections.Generic;
using Manisero.StreamProcessingModel.Executors;
using Manisero.StreamProcessingModel.Executors.StepExecutorResolvers;

namespace Manisero.StreamProcessingModel.Samples.Utils
{
    public static class TaskExecutorResolvers
    {
        private static readonly IDictionary<ResolverType, ITaskStepExecutorResolver> Resolvers
            = new Dictionary<ResolverType, ITaskStepExecutorResolver>
            {
                [ResolverType.Sequential] = new SequentialTaskExecutorResolver(),
                [ResolverType.Streaming] = new StreamingTaskExecutorResolver()
            };

        public static ITaskStepExecutorResolver Get(ResolverType type) => Resolvers[type];
    }

    public enum ResolverType
    {
        Sequential,
        Streaming
    }
}
