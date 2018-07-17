using System;
using Manisero.StreamProcessingModel.TaskExecution.Executors.StepExecutors;
using Manisero.StreamProcessingModel.TaskExecution.Models;
using Manisero.StreamProcessingModel.TaskExecution.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.TaskExecution.Executors.StepExecutorResolvers
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
