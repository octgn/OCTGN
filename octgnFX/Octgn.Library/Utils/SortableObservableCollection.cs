namespace Octgn.Library.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Reflection;

    using log4net;

    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly object sortLock = new object();
        private bool sorting = false;
        private List<NotifyCollectionChangedEventArgs> notifyQueue;

        public void Sort()
        {
            Sort(Comparer<T>.Default);
        }

        public void Sort(IComparer<T> comparer)
        {
            lock (sortLock)
            {
                try
                {
                    sorting = true;
                    notifyQueue = new List<NotifyCollectionChangedEventArgs>();
                    var arr = this.ToList();
                    arr.Sort(comparer);
                    foreach (var i in arr)
                    {
                        var findex = this.IndexOf(i);
                        var toindex = arr.IndexOf(i);
                        this.Move(findex, toindex);
                    }
                    foreach (var nq in notifyQueue)
                    {
                        base.OnCollectionChanged(nq);
                    }
                }
                finally
                {
                    sorting = false;
                }
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!sorting)
            {
                base.OnCollectionChanged(e);
                return;
            }
            notifyQueue.Add(e);
        }
    }
}