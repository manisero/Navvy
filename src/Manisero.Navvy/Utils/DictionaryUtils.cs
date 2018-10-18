using System;
using System.Collections.Generic;

namespace Manisero.Navvy.Utils
{
    internal static class DictionaryUtils
    {
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key)
            => dict.TryGetValue(key, out var value)
                ? value
                : default(TValue);

        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(
            this IDictionary<TKey, TValue> first,
            IDictionary<TKey, TValue> second,
            Func<TValue, TValue, TValue> merge)
        {
            var result = new Dictionary<TKey, TValue>();

            foreach (var entry in first)
            {
                result.Add(
                    entry.Key,
                    second.TryGetValue(entry.Key, out var secondValue)
                        ? merge(entry.Value, secondValue)
                        : entry.Value);
            }

            foreach (var entry in second)
            {
                if (!result.ContainsKey(entry.Key))
                {
                    result.Add(entry.Key, entry.Value);
                }
            }

            return result;
        }
    }
}
