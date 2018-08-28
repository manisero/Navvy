using System;
using Manisero.StreamProcessingModel.Core.Models;

namespace Manisero.StreamProcessingModel.Core
{
    public class TaskExecutionEvents
    {
        public static void StepStarted(ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        public static void StepEnded(ITaskStep step, TaskDescription task, TimeSpan duration, DateTime timestamp)
        {
        }
    }
}
