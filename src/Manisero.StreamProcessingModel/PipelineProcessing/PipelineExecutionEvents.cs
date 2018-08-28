using System;
using System.Collections;
using Manisero.StreamProcessingModel.Core.Models;

namespace Manisero.StreamProcessingModel.PipelineProcessing
{
    public class PipelineExecutionEvents
    {
        public static void BatchStarted(int batchNumber, IEnumerable batch, ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        public static void BatchEnded(int batchNumber, IEnumerable batch, ITaskStep step, TaskDescription task, TimeSpan duration, DateTime timestamp)
        {
        }

        public static void BlockStarted(int batchNumber, IEnumerable batch, IPipelineBlock block, ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        public static void BlockEnded(int batchNumber, IEnumerable batch, IPipelineBlock block, ITaskStep step, TaskDescription task, TimeSpan duration, DateTime timestamp)
        {
        }
    }
}
