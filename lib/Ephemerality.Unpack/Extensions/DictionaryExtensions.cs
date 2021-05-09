using System.Collections.Generic;

namespace Ephemerality.Unpack.Extensions
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
            => dic.TryGetValue(key, out var val) ? val : default;
    }
}