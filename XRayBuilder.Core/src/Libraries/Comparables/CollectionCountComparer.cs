using System;
using System.Collections;
using System.Collections.Generic;

namespace XRayBuilder.Core.Libraries.Comparables
{
    public sealed class CollectionCountComparer<TCollection, TItem> : IComparer where TCollection : ICollection<TItem>
    {
        public int Compare(object x, object y)
        {
            if (x == y)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            return x switch
            {
                TCollection x1 when y is TCollection y1 => x1.Count.CompareTo(y1.Count),
                _ => throw new ArgumentException($"Invalid collection {x}")
            };
        }
    }
}