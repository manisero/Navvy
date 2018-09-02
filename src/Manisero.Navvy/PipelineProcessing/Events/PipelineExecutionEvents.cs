using System;
using System.Collections.Generic;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.PipelineProcessing.Models;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.PipelineProcessing.Events
{
    public struct ItemStartedEvent
    {
        public int ItemNumber;
        public object Item;
        public TimeSpan MaterializationDuration;
        public ITaskStep Step;
        public TaskDefinition Task;
        public DateTime Timestamp;
    }

    public struct ItemEndedEvent
    {
        public int ItemNumber;
        public object Item;
        public ITaskStep Step;
        public TaskDefinition Task;
        public TimeSpan Duration;
        public DateTime Timestamp;
    }

    public struct BlockStartedEvent
    {
        public IPipelineBlock Block;
        public int ItemNumber;
        public object Item;
        public ITaskStep Step;
        public TaskDefinition Task;
        public DateTime Timestamp;
    }

    public struct BlockEndedEvent
    {
        public IPipelineBlock Block;
        public int ItemNumber;
        public object Item;
        public ITaskStep Step;
        public TaskDefinition Task;
        public TimeSpan Duration;
        public DateTime Timestamp;
    }

    public struct PipelineEndedEvent
    {
        public IReadOnlyDictionary<string, TimeSpan> TotalBlockDurations;
        public ITaskStep Step;
        public TaskDefinition Task;
        public DateTime Timestamp;
    }

    public class PipelineExecutionEvents : IExecutionEvents
    {
        public event ExecutionEventHandler<ItemStartedEvent> ItemStarted;
        public event ExecutionEventHandler<ItemEndedEvent> ItemEnded;
        public event ExecutionEventHandler<BlockStartedEvent> BlockStarted;
        public event ExecutionEventHandler<BlockEndedEvent> BlockEnded;
        public event ExecutionEventHandler<PipelineEndedEvent> PipelineEnded;

        public PipelineExecutionEvents(
            ExecutionEventHandler<ItemStartedEvent> itemStarted = null,
            ExecutionEventHandler<ItemEndedEvent> itemEnded = null,
            ExecutionEventHandler<BlockStartedEvent> blockStarted = null,
            ExecutionEventHandler<BlockEndedEvent> blockEnded = null,
            ExecutionEventHandler<PipelineEndedEvent> pipelineEnded = null)
        {
            if (itemStarted != null)
            {
                ItemStarted += itemStarted;
            }

            if (itemEnded != null)
            {
                ItemEnded += itemEnded;
            }

            if (blockStarted != null)
            {
                BlockStarted += blockStarted;
            }

            if (blockEnded != null)
            {
                BlockEnded += blockEnded;
            }

            if (pipelineEnded != null)
            {
                PipelineEnded += pipelineEnded;
            }
        }

        public void OnItemStarted(int itemNumber, object item, TimeSpan materializationDuration, ITaskStep step, TaskDefinition task)
        {
            ItemStarted?.Invoke(new ItemStartedEvent
            {
                ItemNumber = itemNumber,
                Item = item,
                MaterializationDuration = materializationDuration,
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnItemEnded(int itemNumber, object item, ITaskStep step, TaskDefinition task, TimeSpan duration)
        {
            ItemEnded?.Invoke(new ItemEndedEvent
            {
                ItemNumber = itemNumber,
                Item = item,
                Step = step,
                Task = task,
                Duration = duration,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnBlockStarted(IPipelineBlock block, int itemNumber, object item, ITaskStep step, TaskDefinition task)
        {
            BlockStarted?.Invoke(new BlockStartedEvent
            {
                Block = block,
                ItemNumber = itemNumber,
                Item = item,
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnBlockEnded(IPipelineBlock block, int itemNumber, object item, ITaskStep step, TaskDefinition task, TimeSpan duration)
        {
            BlockEnded?.Invoke(new BlockEndedEvent
            {
                Block = block,
                ItemNumber = itemNumber,
                Item = item,
                Step = step,
                Task = task,
                Duration = duration,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnPipelineEnded(IReadOnlyDictionary<string, TimeSpan> totalBlockDurations, ITaskStep step, TaskDefinition task)
        {
            PipelineEnded?.Invoke(new PipelineEndedEvent
            {
                TotalBlockDurations = totalBlockDurations,
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }
    }

    public static class TaskExecutorBuilderExtensions
    {
        public static ITaskExecutorBuilder RegisterPipelineExecutionEvents(
            this ITaskExecutorBuilder builder,
            PipelineExecutionEvents events)
            => builder.RegisterEvents(events);
    }
}
