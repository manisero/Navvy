using System;
using System.Collections.Generic;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.PipelineProcessing
{
    public static class PipelineTaskStep
    {
        public static PipelineTaskStepBuilder<TItem> Builder<TItem>(
            string name = null,
            Func<TaskOutcome, bool> executionCondition = null)
        {
            var builder = new PipelineTaskStepBuilder<TItem>();

            if (name != null)
            {
                builder = builder.WithName(name);
            }

            if (executionCondition != null)
            {
                builder = builder.WithExecutionCondition(executionCondition);
            }

            return builder;
        }
    }

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
