using System;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;

namespace Manisero.StreamProcessingModel.PipelineProcessing.Dataflow
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
