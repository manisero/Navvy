using System;
using Manisero.Navvy.Core.StepExecution;

namespace Manisero.Navvy.PipelineProcessing
{
    public class SequentialPipelineStepExecutorResolver : ITaskStepExecutorResolver
    {
        public ITaskStepExecutor<TTaskStep> Resolve<TTaskStep>()
            where TTaskStep : ITaskStep
        {
            var itemType = typeof(TTaskStep).GenericTypeArguments[0];
            var executorType = typeof(SequentialPipelineStepExecutor<>).MakeGenericType(itemType);

            return (ITaskStepExecutor<TTaskStep>)Activator.CreateInstance(executorType);
        }
    }
}
