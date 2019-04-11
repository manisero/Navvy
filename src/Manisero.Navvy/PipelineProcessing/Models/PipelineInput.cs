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

        public PipelineInput(
            ICollection<TItem> input)
            : this(input, input.Count)
        {
        }
    }

    internal class LazyPipelineInput<TItem> : IPipelineInput<TItem>
    {
        private readonly Lazy<IPipelineInput<TItem>> _lazyInput;
        
        public IEnumerable<TItem> Input => _lazyInput.Value.Input;

        public int ExpectedItemsCount => _lazyInput.Value.ExpectedItemsCount;

        public LazyPipelineInput(
            Func<IPipelineInput<TItem>> inputFactory)
        {
            _lazyInput = new Lazy<IPipelineInput<TItem>>(inputFactory);
        }
    }
}
