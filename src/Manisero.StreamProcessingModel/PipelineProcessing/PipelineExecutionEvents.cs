using System;
using System.Collections;
using Manisero.StreamProcessingModel.Core.Models;

namespace Manisero.StreamProcessingModel.PipelineProcessing
{
    public class PipelineExecutionEvents : IExecutionEvents
    {
        internal void OnBatchStarted(int batchNumber, IEnumerable batch, ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        internal void OnBatchEnded(int batchNumber, IEnumerable batch, ITaskStep step, TaskDescription task, TimeSpan duration, DateTime timestamp)
        {
        }

        internal void OnBlockStarted(int batchNumber, IEnumerable batch, IPipelineBlock block, ITaskStep step, TaskDescription task, DateTime timestamp)
        {
        }

        internal void OnBlockEnded(int batchNumber, IEnumerable batch, IPipelineBlock block, ITaskStep step, TaskDescription task, TimeSpan duration, DateTime timestamp)
        {
        }
    }
}
