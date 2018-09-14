using System;
using System.Collections.Generic;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.PipelineProcessing
{
    public class PipelineTaskStep<TItem> : ITaskStep
    {
        public string Name { get; }

        public Func<TaskOutcome, bool> ExecutionCondition { get; }

        public IPipelineInput<TItem> Input { get; set; }

        public IList<PipelineBlock<TItem>> Blocks { get; }

        public PipelineTaskStep(
            string name,
            IPipelineInput<TItem> input,
            IList<PipelineBlock<TItem>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
        {
            Name = name;
            ExecutionCondition = executionCondition ?? (x => x == TaskOutcome.Successful);
            Input = input;
            Blocks = blocks;
        }

        public PipelineTaskStep(
            string name,
            IEnumerable<TItem> input,
            int expectedItemsCount,
            IList<PipelineBlock<TItem>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
            : this(name, new PipelineInput<TItem>(input, expectedItemsCount), blocks, executionCondition)
        {
        }

        public PipelineTaskStep(
            string name,
            ICollection<TItem> input,
            IList<PipelineBlock<TItem>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
            : this(name, input, input.Count, blocks, executionCondition)
        {
        }
    }
}
