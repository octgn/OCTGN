using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections;

namespace Skylabs.Lobby
{
    public class ThreadSafeList<T> : IList<T>, ICollection<T>, IEnumerable<T>, ICollection, IEnumerable, IDisposable
    {
        private List<T> _list;
        private ReaderWriterLockSlim rwLock;

        public ThreadSafeList()
        {
            _list = new List<T>();
            rwLock = new ReaderWriterLockSlim();
        }

        public void Add(T value)
        {
            rwLock.EnterWriteLock();
            try
            {
                _list.Add(value);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            rwLock.EnterWriteLock();
            try
            {
                _list.Clear();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public bool Contains(T value)
        {
            bool ret = false;
            rwLock.EnterReadLock();
            try
            {
                ret = _list.Contains(value);
            }
            finally
            {
                rwLock.ExitReadLock();
            }

            return (ret);
        }

        public int Count
        {
            get
            {
                int ret = -1;
                rwLock.EnterReadLock();
                try
                {
                    ret = _list.Count;
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
                return (ret);
            }
        }

        public int IndexOf(T value)
        {
            int ret = -1;

            rwLock.EnterReadLock();
            try
            {
                ret = _list.IndexOf(value);
            }
            finally
            {
                rwLock.ExitReadLock();
            }

            return (ret);
        }

        public void Insert(int index, T item)
        {
            rwLock.EnterWriteLock();
            try
            {
                _list.Insert(index, item);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public bool Remove(T value)
        {
            bool ret = false;
            rwLock.EnterWriteLock();
            try
            {
                ret = _list.Remove(value);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
            return (ret);
        }

        public void RemoveAt(int index)
        {
            rwLock.EnterWriteLock();
            try
            {
                _list.RemoveAt(index);
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public void Update(T value)
        {
            rwLock.EnterWriteLock();
            try
            {
                int index = _list.IndexOf(value);
                _list[index] = value;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public void UpdateAt(int index, T value)
        {
            rwLock.EnterWriteLock();
            try
            {
                _list[index] = value;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        public T this[int index]
        {
            get
            {
                rwLock.EnterReadLock();
                try
                {
                    return (_list[index]);
                }
                finally
                {
                    rwLock.ExitReadLock();
                }
            }
            set
            {
                UpdateAt(index, value);
            }
        }


        public IEnumerator<T> GetEnumerator()
        {
            foreach (T item in _list.ToList<T>())
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            rwLock.Dispose();
            _list.Clear();
            _list = null;
        }



        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        
    }
}
