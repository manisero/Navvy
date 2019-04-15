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

        public PipelineInput(
            IEnumerable<TItem> items,
            int expectedItemsCount,
            string name = PipelineInput.DefaultName)
            : this(() => new PipelineInputItems<TItem>(items, expectedItemsCount), name)
        {
        }

        public PipelineInput(
            ICollection<TItem> items,
            string name = PipelineInput.DefaultName)
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
