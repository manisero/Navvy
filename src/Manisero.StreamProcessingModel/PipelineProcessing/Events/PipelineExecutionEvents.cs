using System;
using System.Collections;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.PipelineProcessing.Events
{
    public struct BatchStartedEvent
    {
        public int BatchNumber;
        public IEnumerable Batch;
        public ITaskStep Step;
        public TaskDescription Task;
        public DateTime Timestamp;
    }

    public struct BatchEndedEvent
    {
        public int BatchNumber;
        public IEnumerable Batch;
        public ITaskStep Step;
        public TaskDescription Task;
        public TimeSpan Duration;
        public DateTime Timestamp;
    }

    public struct BlockStartedEvent
    {
        public int BatchNumber;
        public IEnumerable Batch;
        public IPipelineBlock Block;
        public ITaskStep Step;
        public TaskDescription Task;
        public DateTime Timestamp;
    }

    public struct BlockEndedEvent
    {
        public int BatchNumber;
        public IEnumerable Batch;
        public IPipelineBlock Block;
        public ITaskStep Step;
        public TaskDescription Task;
        public TimeSpan Duration;
        public DateTime Timestamp;
    }

    public class PipelineExecutionEvents : IExecutionEvents
    {
        public event ExecutionEventHandler<BatchStartedEvent> BatchStarted;
        public event ExecutionEventHandler<BatchEndedEvent> BatchEnded;
        public event ExecutionEventHandler<BlockStartedEvent> BlockStarted;
        public event ExecutionEventHandler<BlockEndedEvent> BlockEnded;

        internal void OnBatchStarted(int batchNumber, IEnumerable batch, ITaskStep step, TaskDescription task)
        {
            BatchStarted?.Invoke(new BatchStartedEvent
            {
                BatchNumber = batchNumber,
                Batch = batch,
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        internal void OnBatchEnded(int batchNumber, IEnumerable batch, ITaskStep step, TaskDescription task, TimeSpan duration)
        {
            BatchEnded?.Invoke(new BatchEndedEvent
            {
                BatchNumber = batchNumber,
                Batch = batch,
                Step = step,
                Task = task,
                Duration = duration,
                Timestamp = DateTimeUtils.Now
            });
        }

        internal void OnBlockStarted(int batchNumber, IEnumerable batch, IPipelineBlock block, ITaskStep step, TaskDescription task)
        {
            BlockStarted?.Invoke(new BlockStartedEvent
            {
                BatchNumber = batchNumber,
                Batch = batch,
                Block = block,
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        internal void OnBlockEnded(int batchNumber, IEnumerable batch, IPipelineBlock block, ITaskStep step, TaskDescription task, TimeSpan duration)
        {
            BlockEnded?.Invoke(new BlockEndedEvent
            {
                BatchNumber = batchNumber,
                Batch = batch,
                Block = block,
                Step = step,
                Task = task,
                Duration = duration,
                Timestamp = DateTimeUtils.Now
            });
        }
    }
}
