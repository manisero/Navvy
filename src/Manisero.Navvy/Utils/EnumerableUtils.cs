using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Manisero.Navvy.Utils
{
    internal static class EnumerableUtils
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async IAsyncEnumerable<TSource> ToAsyncEnumerable<TSource>(
            this IEnumerable<TSource> source)
        {
            foreach (var item in source)
            {
                yield return item;
            }
        }
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously

        public static async IAsyncEnumerable<TResult> Select<TSource, TResult>(
            this IAsyncEnumerable<TSource> source,
            Func<TSource, Task<TResult>> mapping)
        {
            await foreach (var item in source)
            {
                yield return await mapping(item);
            }
        }

        public static async IAsyncEnumerable<List<TSource>> Batch<TSource>(
            this IAsyncEnumerable<TSource> source,
            int batchSize)
        {
            var batch = new List<TSource>(batchSize);

            await foreach (var item in source)
            {
                batch.Add(item);

                if (batch.Count == batchSize)
                {
                    yield return batch;
                    batch = new List<TSource>(batchSize);
                }
            }

            if (batch.Any())
            {
                batch.TrimExcess();
                yield return batch;
            }
        }
    }
}
