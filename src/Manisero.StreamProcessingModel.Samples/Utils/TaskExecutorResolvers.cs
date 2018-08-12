using System;
using System.Collections.Generic;
using Manisero.StreamProcessingModel.BasicProcessing;
using Manisero.StreamProcessingModel.Core.StepExecution;
using Manisero.StreamProcessingModel.PipelineProcessing;
using Manisero.StreamProcessingModel.PipelineProcessing.Dataflow;
using Manisero.StreamProcessingModel.PipelineProcessing.Sequential;

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
