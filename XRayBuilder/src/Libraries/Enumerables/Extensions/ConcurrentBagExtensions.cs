using System.Collections.Concurrent;

namespace XRayBuilderGUI.Libraries.Enumerables.Extensions
{
    public static class ConcurrentBagExtensions
    {
        public static void AddNotNull<T>(this ConcurrentBag<T> list, T value)
        {
            if (value != null) list.Add(value);
        }
    }
}