using System;
using System.Collections.Generic;

namespace Octgn.Utils
{
    public static class LinqExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> src, T item) where T : class
        {
            var i = 0;
            foreach (var x in src)
            {
                if (x == item) return i;
                ++i;
            }
            return -1;
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<T> src, Func<T, IEnumerable<T>> children)
        {
            foreach (var item in src)
            {
                yield return item;

                var childItems = children(item);
                if (childItems == null) continue;

                foreach (var child in childItems.Flatten(children))
                    yield return child;
            }
        }
    }
}