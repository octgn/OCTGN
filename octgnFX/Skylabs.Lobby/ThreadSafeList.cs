using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Skylabs.Lobby
{
    public class ThreadSafeList<T> : IList<T>, IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;
        private List<T> _list;

        public ThreadSafeList()
        {
            _list = new List<T>();
            _rwLock = new ReaderWriterLockSlim();
        }

        #region IDisposable Members

        public void Dispose()
        {
            _rwLock.Dispose();
            _list.Clear();
            _list = null;
        }

        #endregion

        #region IList<T> Members

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


        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>) _list.ToList()).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            _rwLock.EnterReadLock();
            _list.CopyTo(array,arrayIndex);
            _rwLock.ExitReadLock();
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        #endregion

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
    }
}