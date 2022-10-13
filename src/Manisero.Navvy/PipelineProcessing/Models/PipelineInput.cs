using System;
using System.Collections.Generic;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.PipelineProcessing.Models
{
    public interface IPipelineInput
    {
        string Name { get; }
    }

    public interface IPipelineInput<TItem> : IPipelineInput
    {
        Func<IPipelineInputItems<TItem>> ItemsFactory { get; }
    }

    public interface IPipelineInputItems<TItem>
    {
        IAsyncEnumerable<TItem> Items { get; }

        int ExpectedCount { get; }
    }

    public static class PipelineInput
    {
        public const string DefaultName = "Input";
    }

    public class PipelineInput<TItem> : IPipelineInput<TItem>
    {
        public string Name { get; }

        public Func<IPipelineInputItems<TItem>> ItemsFactory { get; }

        public PipelineInput(
            Func<IPipelineInputItems<TItem>> itemsFactory,
            string name = PipelineInput.DefaultName)
        {
            Name = name;
            ItemsFactory = itemsFactory;
        }
    }

    public class PipelineInputItems<TItem> : IPipelineInputItems<TItem>
    {
        public IAsyncEnumerable<TItem> Items { get; }

        public int ExpectedCount { get; }

        public PipelineInputItems(
            IAsyncEnumerable<TItem> items,
            int expectedCount)
        {
            Items = items;
            ExpectedCount = expectedCount;
        }

        public PipelineInputItems(
            IEnumerable<TItem> items,
            int expectedCount)
            : this(items.ToAsyncEnumerable(), expectedCount)
        {
        }
    }
}
