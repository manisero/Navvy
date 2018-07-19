using System;
using System.Reflection;
using System.Threading;
using Manisero.StreamProcessingModel.Models;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.Executors
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

        public void Execute(
            TaskDescription taskDescription,
            IProgress<TaskProgress> progress,
            CancellationToken cancellation)
        {
            foreach (var step in taskDescription.Steps)
            {
                ExecuteStepMethod.InvokeAsGeneric(
                    this,
                    new[] { step.GetType() },
                    step, progress, cancellation);
            }
        }

        private void ExecuteStep<TStep>(
            TStep step,
            IProgress<TaskProgress> progress,
            CancellationToken cancellation)
            where TStep : ITaskStep
        {
            var stepExecutor = _taskStepExecutorResolver.Resolve<TStep>();

            var stepProgress = new Progress<byte>(
                x => progress.Report(new TaskProgress
                {
                    StepName = step.Name,
                    ProgressPercentage = x
                }));

            stepExecutor.Execute(step, stepProgress, cancellation);
        }
    }
}
