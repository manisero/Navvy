using System;
using System.Collections.Concurrent;

namespace Manisero.Navvy.Core
{
    public class TaskExtras
    {
        private readonly ConcurrentDictionary<string, object> _dict = new ConcurrentDictionary<string, object>();

        public void Set(string key, object item) => _dict[key] = item;

        public TItem Get<TItem>(
            string key)
            => _dict.TryGetValue(key, out var item)
                ? (TItem)item
                : throw new InvalidOperationException($"Task extras do not contain any item under key '{key}'.");

        public TItem TryGet<TItem>(
            string key)
            => _dict.TryGetValue(key, out var item)
                ? (TItem)item
                : default(TItem);
    }
}
