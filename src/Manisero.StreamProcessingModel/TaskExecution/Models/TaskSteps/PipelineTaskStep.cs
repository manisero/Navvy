using System;
using System.Collections.Generic;

namespace Manisero.StreamProcessingModel.TaskExecution.Models.TaskSteps
{
    public class PipelineTaskStep<TData> : ITaskStep
    {
        public string Name { get; set; }

        /// <summary>Batches of data to input to first block. After iterating, first block will be completed.</summary>
        public IEnumerable<ICollection<TData>> Input { get; }

        /// <summary>Used to report progress. Assumption: output count == input count.</summary>
        public int ExpectedInputBatchesCount { get; }

        public ICollection<PipelineBlock<TData>> Blocks { get; }

        public PipelineTaskStep(
            string name,
            ICollection<ICollection<TData>> input,
            ICollection<PipelineBlock<TData>> blocks)
            : this(name, input, input.Count, blocks)
        {
        }

        public PipelineTaskStep(
            string name,
            IEnumerable<ICollection<TData>> input,
            int expectedInputBatchesCount,
            ICollection<PipelineBlock<TData>> blocks)
        {
            Name = name;
            Input = input;
            ExpectedInputBatchesCount = expectedInputBatchesCount;
            Blocks = blocks;
        }
    }

    public class PipelineBlock<TData>
    {
        public string Name { get; }

        public Action<TData> Body { get; }

        public bool Parallel { get; }

        public Action OnCompleted { get; }

        public PipelineBlock(
            string name,
            Action<TData> body,
            bool parallel = false,
            Action onCompleted = null)
        {
            Name = name;
            Body = body;
            Parallel = parallel;
            OnCompleted = onCompleted;
        }
    }
}
