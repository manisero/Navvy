using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
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

        public async Task<TaskResult> Execute(
            TaskDefinition task,
            CancellationToken? cancellation = null,
            params IExecutionEvents[] events)
        {
            if (cancellation == null)
            {
                cancellation = CancellationToken.None;
            }

            var eventsBag = new ExecutionEventsBag(_globalEventsBag, new ExecutionEventsBag(events));

            var outcomeSoFar = TaskOutcome.Successful;
            var errors = new List<TaskExecutionException>();
            var taskEvents = eventsBag.TryGetEvents<TaskExecutionEvents>();
            var taskSw = new Stopwatch();
            var stepSw = new Stopwatch();
            
            taskEvents?.Raise(x => x.OnTaskStarted(task));
            taskSw.Start();

            foreach (var step in task.Steps)
            {
                if (!step.ExecutionCondition(outcomeSoFar))
                {
                    taskEvents?.Raise(x => x.OnStepSkipped(step, task));
                    continue;
                }

                taskEvents?.Raise(x => x.OnStepStarted(step, task));
                stepSw.Restart();

                try
                {
                    await ExecuteStepMethod.InvokeAsGenericAsync(
                        this,
                        new[] { step.GetType() },
                        step, task, outcomeSoFar, cancellation, eventsBag);
                }
#pragma warning disable CS0168
                catch (OperationCanceledException e)
#pragma warning restore CS0168
                {
                    if (outcomeSoFar < TaskOutcome.Canceled)
                    {
                        outcomeSoFar = TaskOutcome.Canceled;
                    }

                    taskEvents?.Raise(x => x.OnStepCanceled(step, task));
                }
                catch (TaskExecutionException e)
                {
                    errors.Add(e);

                    if (outcomeSoFar < TaskOutcome.Failed)
                    {
                        outcomeSoFar = TaskOutcome.Failed;
                    }

                    taskEvents?.Raise(x => x.OnStepFailed(e, step, task));
                }

                stepSw.Stop();
                taskEvents?.Raise(x => x.OnStepEnded(step, task, stepSw.Elapsed));
            }

            var result = new TaskResult(
                outcomeSoFar == TaskOutcome.Canceled,
                errors);

            taskSw.Stop();
            taskEvents?.Raise(x => x.OnTaskEnded(task, result, taskSw.Elapsed));
            
            return result;
        }

        private async Task ExecuteStep<TStep>(
            TStep step,
            TaskDefinition task,
            TaskOutcome outcomeSoFar,
            CancellationToken cancellation,
            ExecutionEventsBag eventsBag)
            where TStep : ITaskStep
        {
            var stepExecutor = _taskStepExecutorResolver.Resolve<TStep>();

            var context = new TaskStepExecutionContext(
                task,
                eventsBag,
                outcomeSoFar);
            
            await stepExecutor.Execute(step, context, cancellation);
        }
    }
}
