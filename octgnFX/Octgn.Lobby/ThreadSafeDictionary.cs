using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Skylabs.Lobby
{
    public class ThreadSafeDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, ICollection, IEnumerable, IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;
        private Dictionary<TKey, TValue> _dict;

        public ThreadSafeDictionary()
        {
            _rwLock = new ReaderWriterLockSlim();
            _dict = new Dictionary<TKey, TValue>();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            IEnumerator<KeyValuePair<TKey, TValue>> ret;
            _rwLock.EnterReadLock();
            try
            {
                Dictionary<TKey, TValue> temp = new Dictionary<TKey, TValue>();
                foreach (KeyValuePair<TKey, TValue> kvi in _dict)
                {
                    temp.Add(kvi.Key, kvi.Value);
                }
                ret = temp.GetEnumerator();
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

        #region ReadLocking Functions

        public TValue this[TKey key]
        {
            get
            {
                TValue ret = default(TValue);
                _rwLock.EnterReadLock();
                try
                {
                    ret = _dict[key];
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
                return (ret);
            }
            set
            {
                Update(key, value);
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            bool ret = false;
            _rwLock.EnterReadLock();
            try
            {
                ret = _dict.Contains(item);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public bool ContainsKey(TKey key)
        {
            bool ret = false;
            _rwLock.EnterReadLock();
            try
            {
                ret = _dict.ContainsKey(key);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public bool ContainsValue(TValue value)
        {
            bool ret = false;
            _rwLock.EnterReadLock();
            try
            {
                ret = _dict.ContainsValue(value);
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
                int ret = 0;
                _rwLock.EnterReadLock();
                try
                {
                    ret = _dict.Count;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
                return (ret);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                ICollection<TKey> ret;
                _rwLock.EnterReadLock();
                try
                {
                    ret = _dict.Keys;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
                return (ret);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            bool ret = false;
            _rwLock.EnterReadLock();
            try
            {
                ret = _dict.TryGetValue(key, out value);
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
            return (ret);
        }

        public ICollection<TValue> Values
        {
            get
            {
                ICollection<TValue> ret;
                _rwLock.EnterReadLock();
                try
                {
                    ret = _dict.Values;
                }
                finally
                {
                    _rwLock.ExitReadLock();
                }
                return (ret);
            }
        }

        #endregion

        #region WriteLocking Functions

        public void Add(TKey key, TValue value)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _dict.Add(key, value);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _dict.Add(item.Key, item.Value);
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
                _dict.Clear();
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public bool Remove(TKey key)
        {
            bool ret = false;
            _rwLock.EnterWriteLock();
            try
            {
                ret = _dict.Remove(key);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            return (ret);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            bool ret = false;
            _rwLock.EnterWriteLock();
            try
            {
                ret = _dict.Remove(item.Key);
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
            return (ret);
        }

        public void Update(TKey key, TValue value)
        {
            _rwLock.EnterWriteLock();
            try
            {
                _dict[key] = value;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public void Update(KeyValuePair<TKey, TValue> item)
        {
            Update(item.Key, item.Value);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            _rwLock.Dispose();
            _dict.Clear();
            _dict = null;
        }

        #endregion

        public object SyncRoot()
        {
            return (new object());
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { }
        void ICollection.CopyTo(Array array, int index) { }



        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return new object(); }
        }





        public bool IsReadOnly
        {
            get { return false; }
        }
    }
}
