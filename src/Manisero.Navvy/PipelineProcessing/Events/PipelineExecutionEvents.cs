using System;
using System.Collections.Generic;
using Manisero.Navvy.Core;
using Manisero.Navvy.PipelineProcessing.Models;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.PipelineProcessing.Events
{
    public struct ItemMaterializedEvent
    {
        public int ItemNumber;
        public object Item;
        public DateTime ItemStartTimestamp;
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
        public TimeSpan TotalInputMaterializationDuration;
        public IReadOnlyDictionary<string, TimeSpan> TotalBlockDurations;
        public ITaskStep Step;
        public TaskDefinition Task;
        public DateTime Timestamp;
    }

    public class PipelineExecutionEvents : IExecutionEvents
    {
        private readonly Action<ItemMaterializedEvent> _itemMaterialized;
        private readonly Action<ItemEndedEvent> _itemEnded;
        private readonly Action<BlockStartedEvent> _blockStarted;
        private readonly Action<BlockEndedEvent> _blockEnded;
        private readonly Action<PipelineEndedEvent> _pipelineEnded;

        public PipelineExecutionEvents(
            Action<ItemMaterializedEvent> itemMaterialized = null,
            Action<ItemEndedEvent> itemEnded = null,
            Action<BlockStartedEvent> blockStarted = null,
            Action<BlockEndedEvent> blockEnded = null,
            Action<PipelineEndedEvent> pipelineEnded = null)
        {
            if (itemMaterialized != null)
            {
                _itemMaterialized += itemMaterialized;
            }

            if (itemEnded != null)
            {
                _itemEnded += itemEnded;
            }

            if (blockStarted != null)
            {
                _blockStarted += blockStarted;
            }

            if (blockEnded != null)
            {
                _blockEnded += blockEnded;
            }

            if (pipelineEnded != null)
            {
                _pipelineEnded += pipelineEnded;
            }
        }

        public void OnItemMaterialized(int itemNumber, object item, DateTimeOffset itemStartTimestamp, TimeSpan materializationDuration, ITaskStep step, TaskDefinition task)
        {
            _itemMaterialized?.Invoke(new ItemMaterializedEvent
            {
                ItemNumber = itemNumber,
                Item = item,
                ItemStartTimestamp = itemStartTimestamp.UtcDateTime,
                MaterializationDuration = materializationDuration,
                Step = step,
                Task = task,
                Timestamp = DateTimeUtils.Now
            });
        }

        public void OnItemEnded(int itemNumber, object item, ITaskStep step, TaskDefinition task, TimeSpan duration)
        {
            _itemEnded?.Invoke(new ItemEndedEvent
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
            _blockStarted?.Invoke(new BlockStartedEvent
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
            _blockEnded?.Invoke(new BlockEndedEvent
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

        public void OnPipelineEnded(TimeSpan totalInputMaterializationDuration, IReadOnlyDictionary<string, TimeSpan> totalBlockDurations, ITaskStep step, TaskDefinition task)
        {
            _pipelineEnded?.Invoke(new PipelineEndedEvent
            {
                TotalInputMaterializationDuration = totalInputMaterializationDuration,
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
