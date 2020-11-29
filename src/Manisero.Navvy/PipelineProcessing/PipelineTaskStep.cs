using System;
using System.Collections.Generic;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.PipelineProcessing
{
    public interface IPipelineTaskStep : ITaskStep
    {
        IPipelineInput Input { get; }

        IReadOnlyCollection<IPipelineBlock> Blocks { get; }
    }

    public class PipelineTaskStep<TItem> : IPipelineTaskStep
    {
        public string Name { get; }

        /// <inheritdoc />
        public Func<TaskOutcome, bool> ExecutionCondition { get; }

        public IPipelineInput<TItem> Input { get; }

        IPipelineInput IPipelineTaskStep.Input => Input;

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
            IEnumerable<TItem> inputItems,
            int expectedInputItemsCount,
            IReadOnlyList<PipelineBlock<TItem>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
            : this(name, new PipelineInput<TItem>(inputItems, expectedInputItemsCount), blocks, executionCondition)
        {
        }

        /// <param name="executionCondition">See <see cref="ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public PipelineTaskStep(
            string name,
            ICollection<TItem> inputItems,
            IReadOnlyList<PipelineBlock<TItem>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
            : this(name, new PipelineInput<TItem>(inputItems), blocks, executionCondition)
        {
        }
    }

    public class PipelineInputExceptionData
    {
        public string InputName { get; set; }

        public int ItemNumber { get; set; }
    }

    public static class PipelineBlockExtensions
    {
        public static PipelineInputExceptionData GetInputExceptionData(
            this IPipelineTaskStep step,
            int itemNumber)
            => new PipelineInputExceptionData
            {
                InputName = step.Input.Name,
                ItemNumber = itemNumber
            };
    }
}
