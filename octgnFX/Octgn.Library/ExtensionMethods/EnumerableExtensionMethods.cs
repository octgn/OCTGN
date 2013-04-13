namespace Octgn.Library.ExtensionMethods
{
    using System;
    using System.Collections.Generic;

    public static class EnumerableExtensionMethods
    {
        internal static Random Random = new Random();
        public static T RandomElement<T>(this IEnumerable<T> source)
        {
            T current = default(T);
            int count = 0;
            foreach (T element in source)
            {
                count++;
                if (Random.Next(count) == 0)
                {
                    current = element;
                }
            }
            return current;
        } 
    }
}