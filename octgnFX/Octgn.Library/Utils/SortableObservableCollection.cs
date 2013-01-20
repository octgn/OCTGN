namespace Octgn.Library.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        private bool isSorting = false;

        public void Sort()
        {
            Sort(Comparer<T>.Default);
        }

        public void Sort(IComparer<T> comparer)
        {
            isSorting = true;
            var arr = this.Items.ToArray();
            Array.Sort(arr,comparer);
            for (int i = 0; i < arr.Length; i++)
            {
                this[i] = arr[i];
            }
            isSorting = false;
        }
    }
}