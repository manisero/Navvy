using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Manisero.StreamProcessingModel.Core.Events;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.Core
{
    public interface ITaskExecutor
    {
        TaskResult Execute(
            TaskDefinition task,
            IProgress<TaskProgress> progress = null,
            CancellationToken? cancellation = null);
    }

    public class TaskExecutor : ITaskExecutor
    {
        private static readonly MethodInfo ExecuteStepMethod
            = typeof(TaskExecutor).GetMethod(nameof(ExecuteStep), BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly ITaskStepExecutorResolver _taskStepExecutorResolver;
        private readonly ExecutionEventsBag _executionEventsBag;

        public TaskExecutor(
            ITaskStepExecutorResolver taskStepExecutorResolver,
            ExecutionEventsBag executionEventsBag)
        {
            _taskStepExecutorResolver = taskStepExecutorResolver;
            _executionEventsBag = executionEventsBag;
        }

        public TaskResult Execute(
            TaskDefinition task,
            IProgress<TaskProgress> progress = null,
            CancellationToken? cancellation = null)
        {
            if (progress == null)
            {
                progress = new EmptyProgress<TaskProgress>();
            }

            if (cancellation == null)
            {
                cancellation = CancellationToken.None;
            }

            var currentOutcome = TaskOutcome.Successful;
            var errors = new List<TaskExecutionException>();
            var events = _executionEventsBag.TryGetEvents<TaskExecutionEvents>();

            events?.OnTaskStarted(task);
            var taskSw = Stopwatch.StartNew();
            
            foreach (var step in task.Steps)
            {
                if (!step.ExecutionCondition(currentOutcome))
                {
                    events?.OnStepSkipped(step, task);
                    continue;
                }

                events?.OnStepStarted(step, task);
                var stepSw = Stopwatch.StartNew();

                try
                {
                    ExecuteStepMethod.InvokeAsGeneric(
                        this,
                        new[] { step.GetType() },
                        step, task, progress, cancellation);
                }
                catch (OperationCanceledException e)
                {
                    if (currentOutcome < TaskOutcome.Canceled)
                    {
                        currentOutcome = TaskOutcome.Canceled;
                    }

                    events?.OnStepCanceled(step, task);
                }
                catch (TaskExecutionException e)
                {
                    errors.Add(e);

                    if (currentOutcome < TaskOutcome.Failed)
                    {
                        currentOutcome = TaskOutcome.Failed;
                    }

                    events?.OnStepFailed(e, step, task);
                }

                stepSw.Stop();
                events?.OnStepEnded(step, task, stepSw.Elapsed);
            }

            var result = new TaskResult(
                currentOutcome == TaskOutcome.Canceled,
                errors);

            taskSw.Stop();
            events?.OnTaskEnded(task, result, taskSw.Elapsed);
            
            return result;
        }

        private void ExecuteStep<TStep>(
            TStep step,
            TaskDefinition task,
            IProgress<TaskProgress> progress,
            CancellationToken cancellation)
            where TStep : ITaskStep
        {
            var stepExecutor = _taskStepExecutorResolver.Resolve<TStep>();

            var context = new TaskStepExecutionContext
            {
                Task = task,
                EventsBag = _executionEventsBag
            };

            var stepProgress = new Progress<byte>(
                x => progress.Report(new TaskProgress
                {
                    StepName = step.Name,
                    ProgressPercentage = x
                }));
            
            stepExecutor.Execute(step, context, stepProgress, cancellation);
        }
    }
}
