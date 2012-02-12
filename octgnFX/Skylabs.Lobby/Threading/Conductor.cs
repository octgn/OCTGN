using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Skylabs.Lobby.Threading
{
    public sealed class Conductor2 : IDisposable
    {
        private readonly Timer _delegateTimer;

        private readonly object _locker = new object();
        private Queue<ConductorAction> _q;

        public Conductor2()
        {
            _q = new Queue<ConductorAction>();
            IsDisposed = false;
            _delegateTimer = new Timer(DelegateTimerTick, null, 5, 5);
        }

        public bool IsDisposed { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            lock (_locker)
            {
                if (IsDisposed) return;
                _delegateTimer.Dispose();
                _q.Clear();
                _q = null;
            }
        }

        #endregion

        public void Add(Action a)
        {
            lock (_locker)
            {
                _q.Enqueue(new ConductorAction(a));
            }
        }

        private void DelegateTimerTick(object state)
        {
            lock (_locker)
            {
                try
                {
                    if (_q.Count <= 0)
                    {
                    }
                    else
                    {
                        var ca = _q.Dequeue();
                        ca.Action.BeginInvoke(InvokeDone, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
            }
        }

        private static void InvokeDone(IAsyncResult result)
        {
        }
    }

    public sealed class ConductorAction
    {
        public ConductorAction(Action a)
        {
            var st = new StackTrace();
            var frames = st.GetFrames();
            CalledFromMethod = "UnknownMethod";
            if (frames != null)
            {
                for (var i = 0; i < frames.Length; i++)
                {
                    if (frames[i].GetMethod().Name != MethodBase.GetCurrentMethod().Name) continue;
                    if (i + 2 >= frames.Length) continue;
                    CalledFromMethod = frames[i + 2].GetMethod().Name;
                    break;
                }
            }
            Action = a;
        }

        public Action Action { get; private set; }
        public String CalledFromMethod { get; private set; }
    }
}