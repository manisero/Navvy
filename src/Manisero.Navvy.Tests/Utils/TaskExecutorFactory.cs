using Manisero.Navvy.Core;

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
            var builder = new TaskExecutorBuilder()
                .RegisterPipelineExecution(resolverType);

            foreach (var e in events)
            {
                builder.RegisterEvents(e);
            }

            return builder.Build();
        }
    }
}
