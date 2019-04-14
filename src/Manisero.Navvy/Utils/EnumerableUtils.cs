using System.Collections.Generic;
using System.Linq;

namespace Manisero.Navvy.Utils
{
    internal static class EnumerableUtils
    {
        public static IEnumerable<ICollection<TSource>> Batch<TSource>(
            this IEnumerable<TSource> source,
            int batchSize)
        {
            var batch = new List<TSource>(batchSize);

            foreach (var item in source)
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
                yield return batch;
            }
        }
    }
}
