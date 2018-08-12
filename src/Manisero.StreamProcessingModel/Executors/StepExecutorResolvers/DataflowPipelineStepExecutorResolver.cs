using System;
using Manisero.StreamProcessingModel.Executors.StepExecutors;
using Manisero.StreamProcessingModel.Models;

namespace Manisero.StreamProcessingModel.Executors.StepExecutorResolvers
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
}
