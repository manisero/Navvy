using System.Collections.Generic;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.Utils
{
    public class BatchedPipelineInputItems<TItem> : IPipelineInputItems<ICollection<TItem>>
    {
        public IEnumerable<ICollection<TItem>> Items { get; }

        public int ExpectedCount { get; }

        public BatchedPipelineInputItems(
            IEnumerable<TItem> items,
            int expectedItemsCount,
            int batchSize)
        {
            Items = items.Batch(batchSize);
            ExpectedCount = expectedItemsCount.CeilingOfDivisionBy(batchSize);
        }

        public BatchedPipelineInputItems(
            ICollection<TItem> items,
            int batchSize)
            : this(items, items.Count, batchSize)
        {
        }
    }
}
