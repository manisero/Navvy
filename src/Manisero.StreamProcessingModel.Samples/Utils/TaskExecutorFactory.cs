using Manisero.StreamProcessingModel.Core;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Dataflow;

namespace Manisero.StreamProcessingModel.Samples.Utils
{
    public enum ResolverType
    {
        Sequential,
        Streaming
    }

    public static class TaskExecutorFactory
    {
        public static ITaskExecutor Create(
            ResolverType resolverType,
            params IExecutionEvents[] events)
        {
            var builder = new TaskExecutorBuilder();

            if (resolverType == ResolverType.Streaming)
            {
                builder.RegisterDataflowExecution();
            }

            foreach (var e in events)
            {
                builder.RegisterEvents(e);
            }

            return builder.Build();
        }
    }
}
