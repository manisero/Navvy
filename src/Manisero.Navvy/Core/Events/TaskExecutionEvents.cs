using System;
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

    public struct StepProgressedEvent
    {
        public byte ProgressPercentage;
        public TimeSpan DurationSoFar;
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
        private readonly Action<TaskStartedEvent> _taskStarted;
        private readonly Action<TaskEndedEvent> _taskEnded;
        private readonly Action<StepStartedEvent> _stepStarted;
        private readonly Action<StepProgressedEvent> _stepProgressed;
        private readonly Action<StepEndedEvent> _stepEnded;
        private readonly Action<StepSkippedEvent> _stepSkipped;
        private readonly Action<StepCanceledEvent> _stepCanceled;
        private readonly Action<StepFailedEvent> _stepFailed;

        public TaskExecutionEvents(
            Action<TaskStartedEvent> taskStarted = null,
            Action<TaskEndedEvent> taskEnded = null,
            Action<StepStartedEvent> stepStarted = null,
            Action<StepProgressedEvent> stepProgressed = null,
            Action<StepEndedEvent> stepEnded = null,
            Action<StepSkippedEvent> stepSkipped = null,
            Action<StepCanceledEvent> stepCanceled = null,
            Action<StepFailedEvent> stepFailed = null)
        {
            if (taskStarted != null)
            {
                _taskStarted = taskStarted;
            }

            if (taskEnded != null)
            {
                _taskEnded = taskEnded;
            }

            if (stepStarted != null)
            {
                _stepStarted = stepStarted;
            }

            if (stepProgressed != null)
            {
                _stepProgressed = stepProgressed;
            }

            if (stepEnded != null)
            {
                _stepEnded = stepEnded;
            }

            if (stepSkipped != null)
            {
                _stepSkipped = stepSkipped;
            }

            if (stepCanceled != null)
            {
                _stepCanceled = stepCanceled;
            }

            if (stepFailed != null)
            {
                _stepFailed = stepFailed;
            }
        }

        public void OnTaskStarted(TaskDefinition task)
        {
            _taskStarted?.Invoke(new TaskStartedEvent
            {
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnTaskEnded(TaskDefinition task, TaskResult result, TimeSpan duration)
        {
            _taskEnded?.Invoke(new TaskEndedEvent
            {
                Task = task,
                Result = result,
                Duration = duration,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnStepStarted(ITaskStep step, TaskDefinition task)
        {
            _stepStarted?.Invoke(new StepStartedEvent
            {
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnStepProgressed(byte progressPercentage, TimeSpan durationSoFar, ITaskStep step, TaskDefinition task)
        {
            _stepProgressed?.Invoke(new StepProgressedEvent
            {
                ProgressPercentage = progressPercentage,
                DurationSoFar = durationSoFar,
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnStepEnded(ITaskStep step, TaskDefinition task, TimeSpan duration)
        {
            _stepEnded?.Invoke(new StepEndedEvent
            {
                Step = step,
                Task = task,
                Duration = duration,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnStepSkipped(ITaskStep step, TaskDefinition task)
        {
            _stepSkipped?.Invoke(new StepSkippedEvent
            {
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnStepCanceled(ITaskStep step, TaskDefinition task)
        {
            _stepCanceled?.Invoke(new StepCanceledEvent
            {
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnStepFailed(TaskExecutionException exception, ITaskStep step, TaskDefinition task)
        {
            _stepFailed?.Invoke(new StepFailedEvent
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
