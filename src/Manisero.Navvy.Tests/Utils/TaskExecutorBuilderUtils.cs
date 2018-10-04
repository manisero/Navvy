using Manisero.Navvy.Dataflow;

namespace Manisero.Navvy.Tests.Utils
{
    public static class TaskExecutorBuilderUtils
    {
        public static ITaskExecutorBuilder RegisterPipelineExecution(
            this ITaskExecutorBuilder builder,
            ResolverType resolverType)
            => resolverType == ResolverType.Streaming
                ? builder.UseDataflowPipelineExecution()
                : builder;
    }
}
