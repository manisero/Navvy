using System;
using System.Collections.Generic;

namespace Manisero.Navvy.PipelineProcessing.Models
{
    public interface IPipelineInput<TItem>
    {
        IEnumerable<TItem> Input { get; }

        int ExpectedItemsCount { get; }
    }

    public class PipelineInput<TItem> : IPipelineInput<TItem>
    {
        public IEnumerable<TItem> Input { get; }

        public int ExpectedItemsCount { get; }

        public PipelineInput(
            IEnumerable<TItem> input,
            int expectedItemsCount)
        {
            Input = input;
            ExpectedItemsCount = expectedItemsCount;
        }
    }

    public class LazyPipelineInput<TItem> : IPipelineInput<TItem>
    {
        private readonly Lazy<IEnumerable<TItem>> _lazyInput;

        private readonly Lazy<int> _lazyExpectedItemsCount;

        public IEnumerable<TItem> Input => _lazyInput.Value;

        public int ExpectedItemsCount => _lazyExpectedItemsCount.Value;

        public LazyPipelineInput(
            Func<IEnumerable<TItem>> inputFactory,
            Func<int> expectedItemsCountFactory)
        {
            _lazyInput = new Lazy<IEnumerable<TItem>>(inputFactory);
            _lazyExpectedItemsCount = new Lazy<int>(expectedItemsCountFactory);
        }
    }
}
