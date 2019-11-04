using System.Collections.Concurrent;

namespace XRayBuilder.Core.Libraries.Enumerables.Extensions
{
    public static class ConcurrentBagExtensions
    {
        public static void AddNotNull<T>(this ConcurrentBag<T> list, T value)
        {
            if (value != null) list.Add(value);
        }
    }
}