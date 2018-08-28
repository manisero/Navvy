using System;

namespace Manisero.StreamProcessingModel.Core
{
    public class TaskExecutionEvents
    {
        public static void StepStarted(DateTime timestamp)
        {
        }

        public static void StepEnded(DateTime timestamp, TimeSpan duration)
        {
        }
    }
}
