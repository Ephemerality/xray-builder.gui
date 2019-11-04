using System.Collections.Generic;

namespace XRayBuilder.Core.Libraries.Enumerables.Extensions
{
    public static class ListExtensions
    {
        public static void AddNotNull<T>(this IList<T> list, T value)
        {
            if (value != null) list.Add(value);
        }
    }
}