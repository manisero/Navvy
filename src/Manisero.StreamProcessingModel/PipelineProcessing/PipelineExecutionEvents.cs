using System;

namespace Manisero.StreamProcessingModel.PipelineProcessing
{
    public class PipelineExecutionEvents
    {
        public static void BlockStarted(BlockStartedEvent e)
        {
        }

        public static void BlockEnded(BlockEndedEvent e)
        {
        }
    }

    public class BlockStartedEvent
    {
        public DateTime Timestamp { get; set; }
    }

    public class BlockEndedEvent
    {
        public DateTime Timestamp { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
