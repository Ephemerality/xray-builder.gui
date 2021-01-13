using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using XRayBuilder.Core.Libraries.Comparables;

namespace XRayBuilder.Core.Libraries.Enumerables
{
    /// <summary>
    /// As taken from System.Data.Linq, with support for sorting collection properties by their size
    /// </summary>
    public class SortableBindingList<T> : BindingList<T>
    {
        private bool isSorted;
        private PropertyDescriptor sortProperty;
        private ListSortDirection sortDirection;

        public SortableBindingList(IList<T> list) : base(list) { }

        protected override void RemoveSortCore()
        {
            isSorted = false;
            sortProperty = null;
        }

        protected override ListSortDirection SortDirectionCore => sortDirection;

        protected override PropertyDescriptor SortPropertyCore => sortProperty;

        protected override bool IsSortedCore => isSorted;

        protected override bool SupportsSortingCore => true;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            if (!PropertyComparer.IsAllowable(prop.PropertyType))
                return;
            ((List<T>) Items).Sort(new PropertyComparer(prop, direction));
            sortDirection = direction;
            sortProperty = prop;
            isSorted = true;
            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        private class PropertyComparer : Comparer<T>
        {
            private readonly PropertyDescriptor prop;
            private readonly IComparer comparer;
            private readonly ListSortDirection direction;
            private readonly bool useToString;

            internal PropertyComparer(PropertyDescriptor prop, ListSortDirection direction)
            {
                this.prop = !(prop.ComponentType != typeof(T)) ? prop : throw new MissingMemberException(typeof(T).Name, prop.Name);
                this.direction = direction;
                if (OkWithIComparable(prop.PropertyType))
                {
                    comparer = (IComparer) typeof(Comparer<>).MakeGenericType(prop.PropertyType).GetProperty(nameof(Comparer.Default))?.GetValue(null, null)
                               ?? throw new InvalidOperationException($"Invalid comprarer for {prop.PropertyType}");
                    useToString = false;
                }
                else
                {
                    if (OkWithToString(prop.PropertyType))
                    {
                        comparer = StringComparer.CurrentCultureIgnoreCase;
                        useToString = true;
                    }
                    else if (OkAsCountableCollection(prop.PropertyType))
                    {
                        var collectionItemType = prop.PropertyType.GenericTypeArguments.FirstOrDefault()
                                             ?? throw new InvalidOperationException($"Missing collection type on {prop.PropertyType}");
                        var collectionType = typeof(ICollection<>).MakeGenericType(collectionItemType);
                        var asd = typeof(CollectionCountComparer<,>).MakeGenericType(collectionType, collectionItemType);
                        var asd2 = Activator.CreateInstance(asd);
                        comparer = (IComparer) asd2;
                    }
                }
            }

            public override int Compare(T x, T y)
            {
                var obj1 = prop.GetValue(x);
                var obj2 = prop.GetValue(y);
                if (useToString)
                {
                    obj1 = obj1?.ToString();
                    obj2 = obj2?.ToString();
                }

                return direction == ListSortDirection.Ascending ? comparer.Compare(obj1, obj2) : comparer.Compare(obj2, obj1);
            }

            private static bool OkWithToString(Type t) => t == typeof(XNode) || t.IsSubclassOf(typeof(XNode));

            private static bool OkWithIComparable(Type t)
            {
                if (t.GetInterface(nameof(IComparable)) != null)
                    return true;
                return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);
            }

            private static bool OkAsCountableCollection(Type t)
                => t.GetInterfaces().Any(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(ICollection<>));

            public static bool IsAllowable(Type t) => OkWithToString(t) || OkWithIComparable(t) || OkAsCountableCollection(t);
        }
    }
}