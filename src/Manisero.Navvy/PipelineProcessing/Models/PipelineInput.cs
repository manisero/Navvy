using System;
using System.Collections.Generic;

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
        IEnumerable<TItem> Items { get; }

        int ExpectedCount { get; }
    }

    public class PipelineInput<TItem> : IPipelineInput<TItem>
    {
        public const string DefaultName = "Input";

        public string Name { get; }

        public Func<IPipelineInputItems<TItem>> ItemsFactory { get; }

        public PipelineInput(
            Func<IPipelineInputItems<TItem>> itemsFactory,
            string name = DefaultName)
        {
            Name = name;
            ItemsFactory = itemsFactory;
        }

        public PipelineInput(
            IEnumerable<TItem> items,
            int expectedItemsCount,
            string name = DefaultName)
            : this(() => new PipelineInputItems<TItem>(items, expectedItemsCount), name)
        {
        }

        public PipelineInput(
            ICollection<TItem> items,
            string name = DefaultName)
            : this(items, items.Count, name)
        {
        }
    }

    public class PipelineInputItems<TItem> : IPipelineInputItems<TItem>
    {
        public IEnumerable<TItem> Items { get; }

        public int ExpectedCount { get; }

        public PipelineInputItems(
            IEnumerable<TItem> items,
            int expectedCount)
        {
            Items = items;
            ExpectedCount = expectedCount;
        }
    }
}
