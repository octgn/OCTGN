namespace Octgn.Library.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;

    using log4net;

    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        
        private bool isSorting = false;

        public void Sort()
        {
            Sort(Comparer<T>.Default);
        }

        public void Sort(IComparer<T> comparer)
        {
            if (isSorting) return;
            try
            {
                isSorting = true;
                var arr = this.Items.ToArray();
                bool changed = true;
                var count = 0;
                while (changed)
                {
                    if (count == 5) break;
                    var temp = new T[arr.Length];
                    Array.Copy(arr, temp, arr.Length);
                    Array.Sort(arr, comparer);
                    changed = arr.Where((t, i) => !Equals(temp[i], t)).Any();
                    count++;
                }
                for (int i = 0; i < arr.Length; i++)
                {
                    this[i] = arr[i];
                }

            }
            catch (Exception e)
            {
                Log.Error("Sort error",e);
            }
            finally
            {
                isSorting = false;
            }
        }
    }
}