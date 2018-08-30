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
    public class TaskExecutor
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
            TaskDescription taskDescription,
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

            events?.OnTaskStarted(taskDescription);
            var taskSw = Stopwatch.StartNew();
            
            foreach (var step in taskDescription.Steps)
            {
                if (!step.ExecutionCondition(currentOutcome))
                {
                    events?.OnStepSkipped(step, taskDescription);
                    continue;
                }

                events?.OnStepStarted(step, taskDescription);
                var stepSw = Stopwatch.StartNew();

                try
                {
                    ExecuteStepMethod.InvokeAsGeneric(
                        this,
                        new[] { step.GetType() },
                        step, taskDescription, progress, cancellation);
                }
                catch (OperationCanceledException e)
                {
                    if (currentOutcome < TaskOutcome.Canceled)
                    {
                        currentOutcome = TaskOutcome.Canceled;
                    }

                    events?.OnStepCanceled(step, taskDescription);
                }
                catch (TaskExecutionException e)
                {
                    errors.Add(e);

                    if (currentOutcome < TaskOutcome.Failed)
                    {
                        currentOutcome = TaskOutcome.Failed;
                    }

                    events?.OnStepFailed(step, taskDescription);
                }

                stepSw.Stop();
                events?.OnStepEnded(step, taskDescription, stepSw.Elapsed);
            }

            var result = new TaskResult(
                currentOutcome == TaskOutcome.Canceled,
                errors);

            taskSw.Stop();
            events?.OnTaskEnded(taskDescription, result, taskSw.Elapsed);
            
            return result;
        }

        private void ExecuteStep<TStep>(
            TStep step,
            TaskDescription taskDescription,
            IProgress<TaskProgress> progress,
            CancellationToken cancellation)
            where TStep : ITaskStep
        {
            var stepExecutor = _taskStepExecutorResolver.Resolve<TStep>();

            var context = new TaskStepExecutionContext
            {
                TaskDescription = taskDescription,
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
