using System;
using System.Collections.Generic;

namespace Octgn.Extentions
{
    public static class LinqExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> src, T item) where T : class
        {
            int i = 0;
            foreach (T x in src)
            {
                if (x == item) return i;
                ++i;
            }
            return -1;
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> src, Func<T, IEnumerable<T>> children)
        {
            foreach (T item in src)
            {
                yield return item;

                IEnumerable<T> childItems = children(item);
                if (childItems == null) continue;

                foreach (T child in childItems.Flatten(children))
                    yield return child;
            }
        }
    }
}