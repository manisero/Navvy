using System;
using System.Collections.Generic;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.PipelineProcessing
{
    public class PipelineTaskStep<TData> : ITaskStep
    {
        public string Name { get; }

        public Func<TaskOutcome, bool> ExecutionCondition { get; }

        /// <summary>Batches of data to input to first block. After iterating, first block will be completed.</summary>
        public IEnumerable<ICollection<TData>> Input { get; }

        /// <summary>Used to report progress. Assumption: output count == input count.</summary>
        public int ExpectedInputBatchesCount { get; }

        public IList<PipelineBlock<TData>> Blocks { get; }

        public PipelineTaskStep(
            string name,
            ICollection<ICollection<TData>> input,
            IList<PipelineBlock<TData>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
            : this(name, input, input.Count, blocks, executionCondition)
        {
        }

        public PipelineTaskStep(
            string name,
            IEnumerable<ICollection<TData>> input,
            int expectedInputBatchesCount,
            IList<PipelineBlock<TData>> blocks,
            Func<TaskOutcome, bool> executionCondition = null)
        {
            Name = name;
            ExecutionCondition = executionCondition ?? (x => x == TaskOutcome.Successful);
            Input = input;
            ExpectedInputBatchesCount = expectedInputBatchesCount;
            Blocks = blocks;
        }
    }

    public class PipelineBlock<TData>
    {
        public string Name { get; }

        public Action<ICollection<TData>> Body { get; }

        public bool Parallel { get; }

        public PipelineBlock(
            string name,
            Action<ICollection<TData>> body,
            bool parallel = false)
        {
            Name = name;
            Body = body;
            Parallel = parallel;
        }

        public static PipelineBlock<TData> ItemBody(
            string name,
            Action<TData> body,
            bool parallel = false)
        {
            return new PipelineBlock<TData>(name, x => x.ForEach(body), parallel);
        }

        public static PipelineBlock<TData> BatchBody(
            string name,
            Action<ICollection<TData>> body,
            bool parallel = false)
        {
            return new PipelineBlock<TData>(name, body, parallel);
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
