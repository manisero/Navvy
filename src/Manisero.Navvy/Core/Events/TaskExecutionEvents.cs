﻿using System;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.Core.Events
{
    public struct TaskStartedEvent
    {
        public TaskDefinition Task;
        public DateTime Timestamp;
    }

    public struct TaskEndedEvent
    {
        public TaskDefinition Task;
        public TaskResult Result;
        public TimeSpan Duration;
        public DateTime Timestamp;
    }

    public struct StepStartedEvent
    {
        public ITaskStep Step;
        public TaskDefinition Task;
        public DateTime Timestamp;
    }

    public struct StepEndedEvent
    {
        public ITaskStep Step;
        public TaskDefinition Task;
        public TimeSpan Duration;
        public DateTime Timestamp;
    }

    public struct StepSkippedEvent
    {
        public ITaskStep Step;
        public TaskDefinition Task;
        public DateTime Timestamp;
    }

    public struct StepCanceledEvent
    {
        public ITaskStep Step;
        public TaskDefinition Task;
        public DateTime Timestamp;
    }

    public struct StepFailedEvent
    {
        public TaskExecutionException Exception;
        public ITaskStep Step;
        public TaskDefinition Task;
        public DateTime Timestamp;
    }

    public class TaskExecutionEvents : IExecutionEvents
    {
        public event ExecutionEventHandler<TaskStartedEvent> TaskStarted;
        public event ExecutionEventHandler<TaskEndedEvent> TaskEnded;
        public event ExecutionEventHandler<StepStartedEvent> StepStarted;
        public event ExecutionEventHandler<StepEndedEvent> StepEnded;
        public event ExecutionEventHandler<StepSkippedEvent> StepSkipped;
        public event ExecutionEventHandler<StepCanceledEvent> StepCanceled;
        public event ExecutionEventHandler<StepFailedEvent> StepFailed;

        public TaskExecutionEvents(
            ExecutionEventHandler<TaskStartedEvent> taskStarted = null,
            ExecutionEventHandler<TaskEndedEvent> taskEnded = null,
            ExecutionEventHandler<StepStartedEvent> stepStarted = null,
            ExecutionEventHandler<StepEndedEvent> stepEnded = null,
            ExecutionEventHandler<StepSkippedEvent> stepSkipped = null,
            ExecutionEventHandler<StepCanceledEvent> stepCanceled = null,
            ExecutionEventHandler<StepFailedEvent> stepFailed = null)
        {
            if (taskStarted != null)
            {
                TaskStarted += taskStarted;
            }

            if (taskEnded != null)
            {
                TaskEnded += taskEnded;
            }

            if (stepStarted != null)
            {
                StepStarted += stepStarted;
            }

            if (stepEnded != null)
            {
                StepEnded += stepEnded;
            }

            if (stepSkipped != null)
            {
                StepSkipped += stepSkipped;
            }

            if (stepCanceled != null)
            {
                StepCanceled += stepCanceled;
            }

            if (stepFailed != null)
            {
                StepFailed += stepFailed;
            }
        }

        public void OnTaskStarted(TaskDefinition task)
        {
            TaskStarted?.Invoke(new TaskStartedEvent
            {
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnTaskEnded(TaskDefinition task, TaskResult result, TimeSpan duration)
        {
            TaskEnded?.Invoke(new TaskEndedEvent
            {
                Task = task,
                Result = result,
                Duration = duration,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnStepStarted(ITaskStep step, TaskDefinition task)
        {
            StepStarted?.Invoke(new StepStartedEvent
            {
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnStepEnded(ITaskStep step, TaskDefinition task, TimeSpan duration)
        {
            StepEnded?.Invoke(new StepEndedEvent
            {
                Step = step,
                Task = task,
                Duration = duration,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnStepSkipped(ITaskStep step, TaskDefinition task)
        {
            StepSkipped?.Invoke(new StepSkippedEvent
            {
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnStepCanceled(ITaskStep step, TaskDefinition task)
        {
            StepCanceled?.Invoke(new StepCanceledEvent
            {
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnStepFailed(TaskExecutionException exception, ITaskStep step, TaskDefinition task)
        {
            StepFailed?.Invoke(new StepFailedEvent
            {
                Exception = exception,
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }
    }

    public static class TaskExecutorBuilderExtensions
    {
        public static ITaskExecutorBuilder RegisterTaskExecutionEvents(
            this ITaskExecutorBuilder builder,
            TaskExecutionEvents events)
            => builder.RegisterEvents(events);
    }
}
