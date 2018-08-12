using System;
using System.Collections.Generic;
using Manisero.StreamProcessingModel.Executors;
using Manisero.StreamProcessingModel.Executors.StepExecutorResolvers;
using Manisero.StreamProcessingModel.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.Samples.Utils
{
    public static class TaskExecutorResolvers
    {
        private static readonly IDictionary<ResolverType, ITaskStepExecutorResolver> Resolvers
            = new Dictionary<ResolverType, ITaskStepExecutorResolver>
            {
                [ResolverType.Sequential] = new CompositeTaskStepExecutorResolver(
                    new Dictionary<Type, ITaskStepExecutorResolver>
                    {
                        [typeof(BasicTaskStep)] = new BasicStepExecutorResolver(),
                        [typeof(PipelineTaskStep<>)] = new SequentialPipelineStepExecutorResolver()
                    }),
                [ResolverType.Streaming] = new CompositeTaskStepExecutorResolver(
                    new Dictionary<Type, ITaskStepExecutorResolver>
                    {
                        [typeof(BasicTaskStep)] = new BasicStepExecutorResolver(),
                        [typeof(PipelineTaskStep<>)] = new DataflowPipelineStepExecutorResolver()
                    })
            };

        public static ITaskStepExecutorResolver Get(ResolverType type) => Resolvers[type];
    }

    public enum ResolverType
    {
        Sequential,
        Streaming
    }
}
