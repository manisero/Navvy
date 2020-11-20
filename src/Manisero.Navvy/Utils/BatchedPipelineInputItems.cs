using System;
using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.Utils
{
    public class BatchedPipelineInputItems<TItem, TBatch> : IPipelineInputItems<TBatch>
    {
        public IEnumerable<TBatch> Items { get; }

        public int ExpectedCount { get; }

        public BatchedPipelineInputItems(
            IEnumerable<TItem> items,
            int expectedItemsCount,
            int batchSize,
            Func<List<TItem>, TBatch> batchFactory)
        {
            Items = items.Batch(batchSize).Select(batchFactory);
            ExpectedCount = expectedItemsCount.CeilingOfDivisionBy(batchSize);
        }

        public BatchedPipelineInputItems(
            ICollection<TItem> items,
            int batchSize,
            Func<List<TItem>, TBatch> batchFactory)
            : this(items, items.Count, batchSize, batchFactory)
        {
        }

        public BatchedPipelineInputItems(
            IReadOnlyCollection<TItem> items,
            int batchSize,
            Func<List<TItem>, TBatch> batchFactory)
            : this(items, items.Count, batchSize, batchFactory)
        {
        }
    }

    public class BatchedPipelineInputItems<TItem> : BatchedPipelineInputItems<TItem, List<TItem>>
    {
        public BatchedPipelineInputItems(
            IEnumerable<TItem> items,
            int expectedItemsCount,
            int batchSize) 
            : base(items, expectedItemsCount, batchSize, x => x)
        {
        }

        public BatchedPipelineInputItems(
            ICollection<TItem> items,
            int batchSize)
            : this(items, items.Count, batchSize)
        {
        }

        public BatchedPipelineInputItems(
            IReadOnlyCollection<TItem> items,
            int batchSize)
            : this(items, items.Count, batchSize)
        {
        }
    }
}
