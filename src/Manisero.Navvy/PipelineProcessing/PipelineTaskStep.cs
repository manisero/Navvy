using System;
using System.Collections.Generic;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.PipelineProcessing
{
    public interface IPipelineTaskStep : ITaskStep
    {
        IReadOnlyCollection<IPipelineBlock> Blocks { get; }
    }

    public class PipelineTaskStep<TItem> : IPipelineTaskStep
    {
        public string Name { get; }

        public Func<TaskOutcome, bool> ExecutionCondition { get; }

        public IPipelineInput<TItem> Input { get; }

        public IReadOnlyList<PipelineBlock<TItem>> Blocks { get; }

        IReadOnlyCollection<IPipelineBlock> IPipelineTaskStep.Blocks => Blocks;

        public PipelineTaskStep(
            string name,
            IPipelineInput<TItem> input,
            IReadOnlyList<PipelineBlock<TItem>> blocks,
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
            IReadOnlyList<PipelineBlock<TItem>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
            : this(name, new PipelineInput<TItem>(input, expectedItemsCount), blocks, executionCondition)
        {
        }

        public PipelineTaskStep(
            string name,
            ICollection<TItem> input,
            IReadOnlyList<PipelineBlock<TItem>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
            : this(name, input, input.Count, blocks, executionCondition)
        {
        }
    }
}
