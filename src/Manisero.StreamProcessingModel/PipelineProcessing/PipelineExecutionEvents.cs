using System;

namespace Manisero.StreamProcessingModel.PipelineProcessing
{
    public class PipelineExecutionEvents
    {
        public static void BatchStarted(DateTime timestamp)
        {
        }

        public static void BatchEnded(DateTime timestamp, TimeSpan duration)
        {
        }

        public static void BlockStarted(DateTime timestamp)
        {
        }

        public static void BlockEnded(DateTime timestamp, TimeSpan duration)
        {
        }
    }
}
