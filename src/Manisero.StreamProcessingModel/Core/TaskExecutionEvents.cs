using System;
using Manisero.StreamProcessingModel.Core.Models;

namespace Manisero.StreamProcessingModel.Core
{
    public class TaskExecutionEvents : IExecutionEvents
    {
        public void TaskStarted(TaskDescription task, DateTime timestamp)
        {
        }

        public void TaskEnded(TaskDescription task, TaskResult result, TimeSpan duration, DateTime timestamp)
        {
        }

        public void StepStarted(ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        public void StepEnded(ITaskStep step, TaskDescription task, TimeSpan duration, DateTime timestamp)
        {
        }

        public void StepSkipped(ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        public void StepCanceled(ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        public void StepFailed(ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }
    }
}
