using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.Core
{
    public class TaskExecutor : ITaskExecutor
    {
        private static readonly MethodInfo ExecuteStepMethod
            = typeof(TaskExecutor).GetMethod(nameof(ExecuteStep), BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly ITaskStepExecutorResolver _taskStepExecutorResolver;
        private readonly ExecutionEventsBag _globalEventsBag;

        public TaskExecutor(
            ITaskStepExecutorResolver taskStepExecutorResolver,
            ExecutionEventsBag globalEventsBag)
        {
            _taskStepExecutorResolver = taskStepExecutorResolver;
            _globalEventsBag = globalEventsBag;
        }

        public TaskResult Execute(
            TaskDefinition task,
            IProgress<TaskProgress> progress = null,
            CancellationToken? cancellation = null,
            params IExecutionEvents[] events)
        {
            if (progress == null)
            {
                progress = new EmptyProgress<TaskProgress>();
            }

            if (cancellation == null)
            {
                cancellation = CancellationToken.None;
            }

            var eventsBag = new ExecutionEventsBag(events); // TODO: Merge with _globalEventsBag

            var currentOutcome = TaskOutcome.Successful;
            var errors = new List<TaskExecutionException>();
            var taskEvents = eventsBag.TryGetEvents<TaskExecutionEvents>();
            var taskSw = new Stopwatch();
            var stepSw = new Stopwatch();
            
            taskEvents?.OnTaskStarted(task);
            taskSw.Start();

            foreach (var step in task.Steps)
            {
                if (!step.ExecutionCondition(currentOutcome))
                {
                    taskEvents?.OnStepSkipped(step, task);
                    continue;
                }

                taskEvents?.OnStepStarted(step, task);
                stepSw.Restart();

                try
                {
                    ExecuteStepMethod.InvokeAsGeneric(
                        this,
                        new[] { step.GetType() },
                        step, task, progress, cancellation, eventsBag);
                }
                catch (OperationCanceledException e)
                {
                    if (currentOutcome < TaskOutcome.Canceled)
                    {
                        currentOutcome = TaskOutcome.Canceled;
                    }

                    taskEvents?.OnStepCanceled(step, task);
                }
                catch (TaskExecutionException e)
                {
                    errors.Add(e);

                    if (currentOutcome < TaskOutcome.Failed)
                    {
                        currentOutcome = TaskOutcome.Failed;
                    }

                    taskEvents?.OnStepFailed(e, step, task);
                }

                stepSw.Stop();
                taskEvents?.OnStepEnded(step, task, stepSw.Elapsed);
            }

            var result = new TaskResult(
                currentOutcome == TaskOutcome.Canceled,
                errors);

            taskSw.Stop();
            taskEvents?.OnTaskEnded(task, result, taskSw.Elapsed);
            
            return result;
        }

        private void ExecuteStep<TStep>(
            TStep step,
            TaskDefinition task,
            IProgress<TaskProgress> progress,
            CancellationToken cancellation,
            ExecutionEventsBag eventsBag)
            where TStep : ITaskStep
        {
            var stepExecutor = _taskStepExecutorResolver.Resolve<TStep>();

            var context = new TaskStepExecutionContext
            {
                Task = task,
                EventsBag = eventsBag
            };

            var stepProgress = new SynchronousProgress<byte>(
                x => progress.Report(new TaskProgress
                {
                    StepName = step.Name,
                    ProgressPercentage = x
                }));
            
            stepExecutor.Execute(step, context, stepProgress, cancellation);
        }
    }
}
