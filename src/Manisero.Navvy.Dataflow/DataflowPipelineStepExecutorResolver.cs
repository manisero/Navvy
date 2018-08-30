using System;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.PipelineProcessing;

namespace Manisero.Navvy.Dataflow
{
    public class DataflowPipelineStepExecutorResolver : ITaskStepExecutorResolver
    {
        public ITaskStepExecutor<TTaskStep> Resolve<TTaskStep>()
            where TTaskStep : ITaskStep
        {
            var dataType = typeof(TTaskStep).GenericTypeArguments[0];
            var executorType = typeof(DataflowPipelineStepExecutor<>).MakeGenericType(dataType);

            return (ITaskStepExecutor<TTaskStep>)Activator.CreateInstance(executorType);
        }
    }

    public static class TaskExecutorBuilderExtensions
    {
        public static ITaskExecutorBuilder RegisterDataflowExecution(
            this ITaskExecutorBuilder builder)
            => builder.RegisterStepExecution(typeof(PipelineTaskStep<>), new DataflowPipelineStepExecutorResolver());
    }
}
