using System;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.Core.Events
{
    public struct TaskStartedEvent
    {
        public TaskDescription Task;
        public DateTime Timestamp;
    }

    public struct TaskEndedEvent
    {
        public TaskDescription Task;
        public TaskResult Result;
        public TimeSpan Duration;
        public DateTime Timestamp;
    }

    public struct StepStartedEvent
    {
        public ITaskStep Step;
        public TaskDescription Task;
        public DateTime Timestamp;
    }

    public struct StepEndedEvent
    {
        public ITaskStep Step;
        public TaskDescription Task;
        public TimeSpan Duration;
        public DateTime Timestamp;
    }

    public struct StepSkippedEvent
    {
        public ITaskStep Step;
        public TaskDescription Task;
        public DateTime Timestamp;
    }

    public struct StepCanceledEvent
    {
        public ITaskStep Step;
        public TaskDescription Task;
        public DateTime Timestamp;
    }

    public struct StepFailedEvent
    {
        public TaskExecutionException Exception;
        public ITaskStep Step;
        public TaskDescription Task;
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

        internal void OnTaskStarted(TaskDescription task)
        {
            TaskStarted?.Invoke(new TaskStartedEvent
            {
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        internal void OnTaskEnded(TaskDescription task, TaskResult result, TimeSpan duration)
        {
            TaskEnded?.Invoke(new TaskEndedEvent
            {
                Task = task,
                Result = result,
                Duration = duration,
                Timestamp = DateTimeUtils.Now
            });
        }

        internal void OnStepStarted(ITaskStep step, TaskDescription task)
        {
            StepStarted?.Invoke(new StepStartedEvent
            {
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        internal void OnStepEnded(ITaskStep step, TaskDescription task, TimeSpan duration)
        {
            StepEnded?.Invoke(new StepEndedEvent
            {
                Step = step,
                Task = task,
                Duration = duration,
                Timestamp = DateTimeUtils.Now
            });
        }

        internal void OnStepSkipped(ITaskStep step, TaskDescription task)
        {
            StepSkipped?.Invoke(new StepSkippedEvent
            {
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        internal void OnStepCanceled(ITaskStep step, TaskDescription task)
        {
            StepCanceled?.Invoke(new StepCanceledEvent
            {
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        internal void OnStepFailed(TaskExecutionException exception, ITaskStep step, TaskDescription task)
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
}
