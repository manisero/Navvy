using Manisero.Navvy.Core;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.Dataflow;

namespace Manisero.Navvy.Tests.Utils
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
