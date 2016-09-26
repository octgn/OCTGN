using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Octgn.Library.Utils
{
    public interface ILibraryRepository<TKey, TValue>
    {
        Task<LibraryItem<TValue>> Checkout(TKey id);
        void Checkin(TKey id, TValue value);
    }

    public abstract class LibraryRepositoryBase<TKey, TValue> : ILibraryRepository<TKey, TValue>
    {
        private ConcurrentDictionary<TKey, LibraryItem<TValue>> _items;

        protected LibraryRepositoryBase() {
            _items = new ConcurrentDictionary<TKey, LibraryItem<TValue>>();
        }

        public async Task<LibraryItem<TValue>> Checkout(TKey id) {
            LibraryItem<TValue> item = null;
            if(!_items.TryGetValue(id, out item)) {
                throw new IndexOutOfRangeException();
            }
            await item.CheckoutAsync();
            return item;
        }

        public void Checkin(TKey id, TValue value) {
            _items.TryAdd(id, new LibraryItem<TValue>(value));
        }
    }

    public class LibraryItem<T> : IDisposable
    {
        public T Item { get; private set; }

        private Queue<AsyncAutoResetEvent> _requests;
        public LibraryItem(T item) {
            Item = item;
            _requests = new Queue<AsyncAutoResetEvent>();
        }
        internal async Task CheckoutAsync() {
            AsyncAutoResetEvent next = null;
            lock (_requests) {
                if (_requests.Count == 0) {
                    return;
                }
                next = new AsyncAutoResetEvent(false);
                _requests.Enqueue(next);
            }
            await next.WaitAsync();
        }

        private void ProcessNextRequest() {
            lock (_requests) {
                if (_requests.Count == 0) return;
                var next = _requests.Dequeue();
                next.Set();
            }
        }

        public void Dispose() {
            ProcessNextRequest();
        }
    }
}
