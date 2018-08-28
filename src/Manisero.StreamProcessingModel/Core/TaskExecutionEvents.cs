using System;
using Manisero.StreamProcessingModel.Core.Models;

namespace Manisero.StreamProcessingModel.Core
{
    public class TaskExecutionEvents : IExecutionEvents
    {
        public void StepStarted(ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        public void StepEnded(ITaskStep step, TaskDescription task, TimeSpan duration, DateTime timestamp)
        {
        }
    }
}
