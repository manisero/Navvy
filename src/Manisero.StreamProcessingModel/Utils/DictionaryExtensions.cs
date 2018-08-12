using System.Collections.Generic;

namespace Manisero.StreamProcessingModel.Utils
{
    internal static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dict,
            TKey key)
            => dict.TryGetValue(key, out var value)
                ? value
                : default(TValue);
    }
}
