using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Skylabs.Lobby.Threading
{
    public sealed class Conductor : IDisposable
    {
        public bool IsDisposed { get; private set; }

        private Queue<Action> Q;

        private Timer DelegateTimer;

        private object Locker = new object();

        public Conductor()
        {
            Q = new Queue<Action>();
            IsDisposed = false;
            DelegateTimer = new Timer(DelegateTimerTick, null, 5, 5);
        }

        public void Add(Action a)
        {
            lock (Locker)
            {
                Q.Enqueue(a);
            }
        }

        private void DelegateTimerTick(object state)
        {
            lock (Locker)
            {
                try
                {
                    if (Q.Count > 0)
                        Q.Dequeue().Invoke();
                }
                catch (Exception e)
                {
                    if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                }
            }
        }

        public void Dispose()
        {
            lock (Locker)
            {
                if (!IsDisposed)
                {
                    DelegateTimer.Dispose();
                    Q.Clear();
                    Q = null;
                }
            }
        }
    }
}
