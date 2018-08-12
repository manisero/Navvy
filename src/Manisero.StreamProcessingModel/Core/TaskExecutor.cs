using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.Core
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

        public TaskResult Execute(
            TaskDescription taskDescription,
            IProgress<TaskProgress> progress,
            CancellationToken cancellation)
        {
            var currentOutcome = TaskOutcome.Successful;
            var errors = new List<TaskExecutionException>();

            foreach (var step in taskDescription.Steps)
            {
                if (!step.ExecutionCondition(currentOutcome))
                {
                    continue;
                }

                try
                {
                    ExecuteStepMethod.InvokeAsGeneric(
                        this,
                        new[] { step.GetType() },
                        step, progress, cancellation);
                }
                catch (OperationCanceledException e)
                {
                    if (currentOutcome < TaskOutcome.Canceled)
                    {
                        currentOutcome = TaskOutcome.Canceled;
                    }
                }
                catch (TaskExecutionException e)
                {
                    errors.Add(e);

                    if (currentOutcome < TaskOutcome.Failed)
                    {
                        currentOutcome = TaskOutcome.Failed;
                    }
                }
            }

            return new TaskResult(
                currentOutcome == TaskOutcome.Canceled,
                errors);
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
