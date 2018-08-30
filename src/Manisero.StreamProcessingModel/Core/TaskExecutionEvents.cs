using System;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.Core
{
    public struct TaskStartedEvent
    {
        public TaskDescription Task;
        public DateTime Timestamp;
    }

    public class TaskExecutionEvents : IExecutionEvents
    {
        public event ExecutionEventHandler<TaskStartedEvent> TaskStarted;

        internal void OnTaskStarted(TaskDescription task)
        {
            TaskStarted?.Invoke(new TaskStartedEvent
            {
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        internal void OnTaskEnded(TaskDescription task, TaskResult result, TimeSpan duration, DateTime timestamp)
        {
        }

        internal void OnStepStarted(ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        internal void OnStepEnded(ITaskStep step, TaskDescription task, TimeSpan duration, DateTime timestamp)
        {
        }

        internal void OnStepSkipped(ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        internal void OnStepCanceled(ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        internal void OnStepFailed(ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }
    }
}
