﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.PipelineProcessing
{
    public static class TaskStepBuilderUtils
    {
        /// <summary>See <see cref="PipelineTaskStep{TItem}"/>.</summary>
        /// <param name="executionCondition">See <see cref="ITaskStep.ExecutionCondition"/>. If null, <see cref="TaskStepUtils.DefaultExecutionCondition"/> will be used.</param>
        public static PipelineTaskStepBuilder<TItem> Pipeline<TItem>(
            this TaskStepBuilder _,
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

    public class PipelineTaskStepBuilder<TItem>
    {
        private string _name;
        private Func<TaskOutcome, bool> _executionCondition;
        private IPipelineInput<TItem> _input;
        private List<PipelineBlock<TItem>> _blocks = new List<PipelineBlock<TItem>>();

        public PipelineTaskStepBuilder<TItem> WithName(
            string name)
        {
            _name = name;
            return this;
        }

        public PipelineTaskStepBuilder<TItem> WithExecutionCondition(
            Func<TaskOutcome, bool> executionCondition)
        {
            _executionCondition = executionCondition;
            return this;
        }

        public PipelineTaskStepBuilder<TItem> WithInput(
            IPipelineInput<TItem> input)
        {
            _input = input;
            return this;
        }

        public PipelineTaskStepBuilder<TItem> WithInput(
            Func<IPipelineInputItems<TItem>> itemsFactory,
            string name = PipelineInput.DefaultName)
            => WithInput(new PipelineInput<TItem>(itemsFactory, name));

        public PipelineTaskStepBuilder<TItem> WithInput(
            IAsyncEnumerable<TItem> items,
            int expectedItemsCount,
            string name = PipelineInput.DefaultName)
            => WithInput(
                () => new PipelineInputItems<TItem>(items, expectedItemsCount),
                name);

        public PipelineTaskStepBuilder<TItem> WithInput(
            IEnumerable<TItem> items,
            int expectedItemsCount,
            string name = PipelineInput.DefaultName)
            => WithInput(
                () => new PipelineInputItems<TItem>(items, expectedItemsCount),
                name);

        public PipelineTaskStepBuilder<TItem> WithInput(
            ICollection<TItem> items,
            string name = PipelineInput.DefaultName)
            => WithInput(items, items.Count, name);

        public PipelineTaskStepBuilder<TItem> WithBlock(
            PipelineBlock<TItem> block)
        {
            _blocks.Add(block);
            return this;
        }

        public PipelineTaskStepBuilder<TItem> WithBlock(
            string name,
            Action<TItem> body,
            int maxDegreeOfParallelism = 1)
            => WithBlock(new PipelineBlock<TItem>(
                name,
                body,
                maxDegreeOfParallelism));

        public PipelineTaskStepBuilder<TItem> WithBlock(
            string name,
            Func<TItem, Task> body,
            int maxDegreeOfParallelism = 1)
            => WithBlock(new PipelineBlock<TItem>(name, body, maxDegreeOfParallelism));

        public PipelineTaskStep<TItem> Build()
        {
            if (_input == null)
            {
                throw new InvalidOperationException($"Error while building {nameof(PipelineTaskStep<TItem>)}<{typeof(TItem).Name}> '{_name}'. {nameof(PipelineTaskStep<TItem>.Input)} is not configured.");
            }

            return new PipelineTaskStep<TItem>(
                _name,
                _input,
                _blocks,
                _executionCondition);
        }
    }
}
