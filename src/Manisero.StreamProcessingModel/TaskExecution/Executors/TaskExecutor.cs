using System;
using System.Reflection;
using Manisero.StreamProcessingModel.TaskExecution.Models;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.TaskExecution.Executors
{
    public class TaskExecutor
    {
        private static readonly MethodInfo ExecuteStepMethod
            = typeof(TaskExecutor).GetMethod(nameof(ExecuteStep), BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly ITaskStepExecutorResolver _taskStepExecutorResolver;

        public TaskExecutor(
            ITaskStepExecutorResolver taskStepExecutorResolver)
        {
            _taskStepExecutorResolver = taskStepExecutorResolver;
        }

        public void Execute(TaskDescription taskDescription)
        {
            foreach (var step in taskDescription.Steps)
            {
                ExecuteStepMethod.InvokeAsGeneric(this, new[] { step.GetType() }, step);
            }
        }

        private void ExecuteStep<TStep>(TStep step)
            where TStep : ITaskStep
        {
            var stepExecutor = _taskStepExecutorResolver.Resolve<TStep>();
            stepExecutor.Execute(step);
        }
    }
}
