using System;
using Manisero.StreamProcessingModel.Core;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;
using Manisero.StreamProcessingModel.PipelineProcessing;

namespace Manisero.StreamProcessingModel.Dataflow
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
