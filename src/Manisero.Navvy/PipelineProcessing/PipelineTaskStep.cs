using System;
using System.Collections.Generic;
using Manisero.Navvy.Core.Models;

namespace Manisero.Navvy.PipelineProcessing
{
    public class PipelineTaskStep<TItem> : ITaskStep
    {
        public string Name { get; }

        public Func<TaskOutcome, bool> ExecutionCondition { get; }

        /// <summary>Items to input to first block. After iterating, first block will be completed.</summary>
        public IEnumerable<TItem> Input { get; }

        /// <summary>Used to report progress. Assumption: output count == input count.</summary>
        public int ExpectedItemsCount { get; }

        public IList<PipelineBlock<TItem>> Blocks { get; }

        public PipelineTaskStep(
            string name,
            ICollection<TItem> input,
            IList<PipelineBlock<TItem>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
            : this(name, input, input.Count, blocks, executionCondition)
        {
        }

        public PipelineTaskStep(
            string name,
            IEnumerable<TItem> input,
            int expectedItemsCount,
            IList<PipelineBlock<TItem>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
        {
            Name = name;
            ExecutionCondition = executionCondition ?? (x => x == TaskOutcome.Successful);
            Input = input;
            ExpectedItemsCount = expectedItemsCount;
            Blocks = blocks;
        }
    }

    public interface IPipelineBlock
    {
        string Name { get; }
    }

    public class PipelineBlock<TItem> : IPipelineBlock
    {
        public string Name { get; }

        public Action<TItem> Body { get; }

        public bool Parallel { get; }

        public PipelineBlock(
            string name,
            Action<TItem> body,
            bool parallel = false)
        {
            Name = name;
            Body = body;
            Parallel = parallel;
        }
    }

    public class PipelineBlockExceptionData
    {
        public string BlockName { get; set; }
    }

    public static class PipelineBlockExtensions
    {
        public static PipelineBlockExceptionData GetExceptionData<TData>(
            this PipelineBlock<TData> block)
            => new PipelineBlockExceptionData
            {
                BlockName = block.Name
            };
    }
}
