using System;
using Manisero.StreamProcessingModel.Executors.StepExecutors;
using Manisero.StreamProcessingModel.Models;
using Manisero.StreamProcessingModel.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.Executors.StepExecutorResolvers
{
    public class StreamingTaskExecutorResolver : ITaskStepExecutorResolver
    {
        public ITaskStepExecutor<TTaskStep> Resolve<TTaskStep>()
            where TTaskStep : ITaskStep
        {
            var taskStepType = typeof(TTaskStep);

            if (taskStepType == typeof(BasicTaskStep))
            {
                return (ITaskStepExecutor<TTaskStep>)new BasicStepExecutor();
            }
            else if (taskStepType.GetGenericTypeDefinition() == typeof(PipelineTaskStep<>))
            {
                return (ITaskStepExecutor<TTaskStep>)Activator
                    .CreateInstance(typeof(DataflowPipelineStepExecutor<>)
                                        .MakeGenericType(taskStepType.GenericTypeArguments[0]));
            }
            else
            {
                throw new NotSupportedException($"{taskStepType} task step not supported.");
            }
        }
    }
}
