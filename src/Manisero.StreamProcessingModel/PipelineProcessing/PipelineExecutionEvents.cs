using System;
using System.Collections;
using Manisero.StreamProcessingModel.Core.Models;

namespace Manisero.StreamProcessingModel.PipelineProcessing
{
    public class PipelineExecutionEvents : IExecutionEvents
    {
        public void BatchStarted(int batchNumber, IEnumerable batch, ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        public void BatchEnded(int batchNumber, IEnumerable batch, ITaskStep step, TaskDescription task, TimeSpan duration, DateTime timestamp)
        {
        }

        public void BlockStarted(int batchNumber, IEnumerable batch, IPipelineBlock block, ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        public void BlockEnded(int batchNumber, IEnumerable batch, IPipelineBlock block, ITaskStep step, TaskDescription task, TimeSpan duration, DateTime timestamp)
        {
        }
    }
}
