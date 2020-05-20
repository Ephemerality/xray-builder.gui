using System.Collections.Generic;

namespace XRayBuilder.Core.Libraries.Enumerables.Extensions
{
    public static class EnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable) => new HashSet<T>(enumerable);
    }
}