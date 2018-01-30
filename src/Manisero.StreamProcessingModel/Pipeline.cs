using System;
using System.Collections.Generic;
using System.Linq;

namespace Manisero.StreamProcessingModel
{
    public class Pipeline<TInput>
    {
        private readonly List<IBlock> _blocks = new List<IBlock>();

        public string Name { get; }

        /// <summary>Input items for first block. After iterating, first block will be completed.</summary>
        public IEnumerable<TInput> Input { get; }

        /// <summary>Used to report progress. Assumption: output count == input count.</summary>
        public int ExpectedInputCount { get; }

        public ICollection<IBlock> Blocks => _blocks;

        public Pipeline(
            string name,
            ICollection<TInput> input)
            : this(name, input, input.Count)
        {
        }

        public Pipeline(
            string name,
            IEnumerable<TInput> input,
            int expectedInputCount)
        {
            Name = name;
            Input = input;
            ExpectedInputCount = expectedInputCount;
        }

        public void AddBlock(IBlock block)
        {
            var lastBlock = _blocks.LastOrDefault();

            if (lastBlock != null && block.InputType != lastBlock.OutputType)
            {
                throw new InvalidOperationException($"Last block's output type is {lastBlock.OutputType}.");
            }

            _blocks.Add(block);
        }
    }
}
