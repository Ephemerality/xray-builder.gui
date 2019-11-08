using System;
using System.Collections.Generic;
using System.Linq;

namespace XRayBuilder.Core.Libraries.Enumerables.Extensions
{
    public static class DictionaryExtensions
    {
        public static void Replace<TKey>(this Dictionary<TKey, string> dic, string needle, string replacement)
        {
            foreach (var key in dic.Keys.ToList())
                dic[key] = dic[key].Replace(needle, replacement);
        }

        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
            => dic.TryGetValue(key, out var val) ? val : default;

        public static TValue GetOrThrow<TKey, TValue>(this Dictionary<TKey, TValue> dic, TKey key)
        {
            if (dic.TryGetValue(key, out var val))
                return val;

            throw new ArgumentException("Key not found", nameof(key));
        }

        public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> tuple, out T1 key, out T2 value)
        {
            key = tuple.Key;
            value = tuple.Value;
        }
    }
}