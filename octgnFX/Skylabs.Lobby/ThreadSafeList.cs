using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Skylabs.Lobby
{
    public class ThreadSafeList<T> : IList<T>, ICollection, IDisposable, IEnumerable, IEnumerable<T>
    {
        private readonly ReaderWriterLockSlim _rwLock;
        private List<T> _list;

        public ThreadSafeList()
        {
            _list = new List<T>();
            _rwLock = new ReaderWriterLockSlim();
        }

        #region ICollection Members

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return new object(); }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _rwLock.Dispose();
            _list.Clear();
            _list = null;
        }

        #endregion

        #region IList<T> Members



        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        #endregion

        #region WriteLocking Functions

        public void Add(T value)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.Add(value);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.AddRange(collection);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.Clear();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void ForEach(Action<T> action)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.ForEach(action);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Insert(int index, T item)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.Insert(index, item);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.InsertRange(index, collection);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool Remove(T value)
        {
            _rwLock.EnterWriteLock();
            bool ret;
            try
            {
                ret = _list.Remove(value);
            }
            catch
            {
                ret = false;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            return (ret);
        }

        public int RemoveAll(Predicate<T> match)
        {
            int ret = 0;
            _rwLock.EnterWriteLock();
            try
            {
                ret = _list.RemoveAll(match);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            return (ret);
        }

        public void RemoveAt(int index)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.RemoveAt(index);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void RemoveRange(int index, int count)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.RemoveRange(index, count);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Reverse()
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.Reverse();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Reverse(int index, int count)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.Reverse(index, count);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Sort()
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.Sort();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Sort(Comparison<T> comparison)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.Sort(comparison);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Sort(IComparer<T> comparer)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.Sort(comparer);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.Sort(index, count, comparer);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void TrimExcess()
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list.TrimExcess();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }
        
        public T[] ToArray()
        {
            _rwLock.EnterWriteLock();
            T[] ret = null;
            try
            {
                ret = _list.ToArray();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            return ret;
        }

        public void Update(T value)
        {
            _rwLock.EnterWriteLock();
            try
            {
                int index = _list.IndexOf(value);
                _list[index] = value;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void UpdateAt(int index, T value)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _list[index] = value;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }



        #endregion

        #region ReadLocking Functions

        public T this[int index]
        {
            get
            {
                _rwLock.EnterReadLock();
                try
                {
                    return (_list[index]);
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
            }
            set { UpdateAt(index, value); }
        }

        public int BinarySearch(T item)
        {
            int ret = -1;
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.BinarySearch(item);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }
        public int BinarySearch(T item, IComparer<T> comparer)
        {
            int ret = -1;
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.BinarySearch(item, comparer);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }
        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
        {
            int ret = -1;
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.BinarySearch(index, count, item, comparer);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public bool Contains(T value)
        {
            _rwLock.EnterReadLock();
            bool ret;
            try
            {
                ret = _list.Contains(value);
            }
            catch
            {
                ret = false;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            return (ret);
        }

        void ICollection.CopyTo(Array array, int index)
        {
            _rwLock.EnterReadLock();
            try
            {
                array.SetValue(_list[index], index);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        void ICollection<T>.CopyTo(T[] array, int index)
        {
            _rwLock.EnterReadLock();
            try
            {
                array.SetValue(_list[index], index);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public int Count
        {
            get
            {
                _rwLock.EnterReadLock();
                int ret;
                try
                {
                    ret = _list.Count;
                }
                catch (Exception)
                {
                    ret = -1;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
                return (ret);
            }
        }

        public bool Exists(Predicate<T> match)
        {
            bool ret = false;
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.Exists(match);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public T Find(Predicate<T> match)
        {
            T ret = default(T);
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.Find(match);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public List<T> FindAll(Predicate<T> match)
        {
            List<T> ret = new List<T>();
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.FindAll(match);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public int FindIndex(Predicate<T> match)
        {
            int ret = -1;
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.FindIndex(match);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            int ret = -1;
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.FindIndex(startIndex, match);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            int ret = -1;
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.FindIndex(startIndex, count, match);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public T FindLast(Predicate<T> match)
        {
            T ret = default(T);
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.FindLast(match);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            int ret = -1;
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.FindIndex(match);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
        {
            int ret = -1;
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.FindIndex(startIndex, match);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            int ret = -1;
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.FindIndex(startIndex, count, match);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public IEnumerator<T> GetEnumerator()
        {
            IEnumerator<T> ret;
            _rwLock.EnterReadLock();
            try
            {
                ret = ((IEnumerable<T>)_list.ToList()).GetEnumerator();
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public List<T> GetRange(int index, int count)
        {
            List<T> ret = new List<T>();
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.GetRange(index, count);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public int IndexOf(T value)
        {
            _rwLock.EnterReadLock();
            int ret;
            try
            {
                ret = _list.IndexOf(value);
            }
            catch
            {
                ret = -1;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }

            return (ret);
        }

        public bool TrueForAll(Predicate<T> match)
        {
            bool ret = true;
            _rwLock.EnterReadLock();
            try
            {
                ret = _list.TrueForAll(match);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }


        #endregion
    }
}
