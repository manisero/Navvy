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

        /// <inheritdoc />
        public Func<TaskOutcome, bool> ExecutionCondition { get; }

        public IPipelineInput<TItem> Input { get; }

        public IReadOnlyList<PipelineBlock<TItem>> Blocks { get; }

        IReadOnlyCollection<IPipelineBlock> IPipelineTaskStep.Blocks => Blocks;

        /// <param name="executionCondition">See <see cref="ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public PipelineTaskStep(
            string name,
            IPipelineInput<TItem> input,
            IReadOnlyList<PipelineBlock<TItem>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
        {
            Name = name;
            ExecutionCondition = executionCondition ?? TaskStepUtils.DefaultExecutionCondition;
            Input = input;
            Blocks = blocks;
        }

        /// <param name="executionCondition">See <see cref="ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public PipelineTaskStep(
            string name,
            IEnumerable<TItem> input,
            int expectedItemsCount,
            IReadOnlyList<PipelineBlock<TItem>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
            : this(name, new PipelineInput<TItem>(input, expectedItemsCount), blocks, executionCondition)
        {
        }

        /// <param name="executionCondition">See <see cref="ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
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
