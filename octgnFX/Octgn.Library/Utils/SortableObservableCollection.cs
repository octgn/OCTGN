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
            if (isSorting) return;
            var arr = this.Items.ToArray();
            bool changed = true;
            while (changed)
            {
                var temp = new T[arr.Length];
                Array.Copy(arr,temp,arr.Length);
                Array.Sort(arr, comparer);
                changed = arr.Where((t, i) => !Equals(temp[i], t)).Any();
            }
            for (int i = 0; i < arr.Length; i++)
            {
                this[i] = arr[i];
            }
            isSorting = false;
        }
    }
}