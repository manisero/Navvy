using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

            var eventsBag = events.Any() ? new ExecutionEventsBag(events) : _globalEventsBag; // TODO: Merge bags

            var currentOutcome = TaskOutcome.Successful;
            var errors = new List<TaskExecutionException>();
            var taskEvents = eventsBag.TryGetEvents<TaskExecutionEvents>();
            var taskSw = new Stopwatch();
            var stepSw = new Stopwatch();
            
            taskEvents?.Raise(x => x.OnTaskStarted(task));
            taskSw.Start();

            foreach (var step in task.Steps)
            {
                if (!step.ExecutionCondition(currentOutcome))
                {
                    taskEvents?.Raise(x => x.OnStepSkipped(step, task));
                    continue;
                }

                taskEvents?.Raise(x => x.OnStepStarted(step, task));
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

                    taskEvents?.Raise(x => x.OnStepCanceled(step, task));
                }
                catch (TaskExecutionException e)
                {
                    errors.Add(e);

                    if (currentOutcome < TaskOutcome.Failed)
                    {
                        currentOutcome = TaskOutcome.Failed;
                    }

                    taskEvents?.Raise(x => x.OnStepFailed(e, step, task));
                }

                stepSw.Stop();
                taskEvents?.Raise(x => x.OnStepEnded(step, task, stepSw.Elapsed));
            }

            var result = new TaskResult(
                currentOutcome == TaskOutcome.Canceled,
                errors);

            taskSw.Stop();
            taskEvents?.Raise(x => x.OnTaskEnded(task, result, taskSw.Elapsed));
            
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
