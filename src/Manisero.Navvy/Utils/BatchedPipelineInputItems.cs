using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.Utils
{
    public class BatchedPipelineInputItems<TItem, TBatch> : IPipelineInputItems<TBatch>
    {
        public IAsyncEnumerable<TBatch> Items { get; }

        public int ExpectedCount { get; }

        public BatchedPipelineInputItems(
            IAsyncEnumerable<TItem> items,
            int expectedItemsCount,
            int batchSize,
            Func<List<TItem>, TBatch> batchFactory)
            : this(
                items,
                expectedItemsCount,
                batchSize,
                x => Task.FromResult(batchFactory(x)))
        {
        }

        public BatchedPipelineInputItems(
            IAsyncEnumerable<TItem> items,
            int expectedItemsCount,
            int batchSize,
            Func<List<TItem>, Task<TBatch>> batchFactory)
        {
            Items = items.Batch(batchSize).Select(batchFactory);
            ExpectedCount = expectedItemsCount.CeilingOfDivisionBy(batchSize);
        }

        public BatchedPipelineInputItems(
            IEnumerable<TItem> items,
            int expectedItemsCount,
            int batchSize,
            Func<List<TItem>, TBatch> batchFactory)
            : this(items.ToAsyncEnumerable(), expectedItemsCount, batchSize, batchFactory)
        {
        }

        public BatchedPipelineInputItems(
            IEnumerable<TItem> items,
            int expectedItemsCount,
            int batchSize,
            Func<List<TItem>, Task<TBatch>> batchFactory)
            : this(items.ToAsyncEnumerable(), expectedItemsCount, batchSize, batchFactory)
        {
        }

        public BatchedPipelineInputItems(
            ICollection<TItem> items,
            int batchSize,
            Func<List<TItem>, TBatch> batchFactory)
            : this(items, items.Count, batchSize, batchFactory)
        {
        }

        public BatchedPipelineInputItems(
            ICollection<TItem> items,
            int batchSize,
            Func<List<TItem>, Task<TBatch>> batchFactory)
            : this(items, items.Count, batchSize, batchFactory)
        {
        }
    }

    public class BatchedPipelineInputItems<TItem> : BatchedPipelineInputItems<TItem, List<TItem>>
    {
        public BatchedPipelineInputItems(
            IAsyncEnumerable<TItem> items,
            int expectedItemsCount,
            int batchSize)
            : base(items, expectedItemsCount, batchSize, x => x)
        {
        }

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
            : base(items, batchSize, x => x)
        {
        }
    }
}
