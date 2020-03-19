using System.Collections.Generic;
using JetBrains.Annotations;

namespace XRayBuilder.Core.Libraries.Enumerables.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Add <paramref name="value"/> to <paramref name="list"/> if the value is not null
        /// </summary>
        public static void AddNotNull<T>([NotNull] this IList<T> list, [CanBeNull] T value)
        {
            if (value != null) list.Add(value);
        }
    }
}