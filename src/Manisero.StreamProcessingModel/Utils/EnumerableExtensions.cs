using System;
using System.Collections.Generic;

namespace Manisero.StreamProcessingModel.Utils
{
    public static class EnumerableExtensions
    {
        public static void ForEach<TSource>(
            this IEnumerable<TSource> source,
            Action<TSource> body)
        {
            foreach (var item in source)
            {
                body(item);
            }
        }
    }
}
