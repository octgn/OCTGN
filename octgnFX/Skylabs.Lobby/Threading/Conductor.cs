using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;

namespace Skylabs.Lobby.Threading
{
    public sealed class Conductor2 : IDisposable
    {
        private readonly Timer DelegateTimer;

        private readonly object Locker = new object();
        private Queue<ConductorAction> Q;

        public Conductor2()
        {
            Q = new Queue<ConductorAction>();
            IsDisposed = false;
            DelegateTimer = new Timer(DelegateTimerTick, null, 5, 5);
        }

        public bool IsDisposed { get; private set; }

        #region IDisposable Members

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

        #endregion

        public void Add(Action a)
        {
            lock (Locker)
            {
                Q.Enqueue(new ConductorAction(a));
            }
        }

        private void DelegateTimerTick(object state)
        {
            lock (Locker)
            {
                try
                {
                    if (Q.Count > 0)
                    {
                        ConductorAction ca = Q.Dequeue();
                        ca.Action.BeginInvoke(InvokeDone, null);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }
        }

        private void InvokeDone(IAsyncResult result)
        {
        }
    }

    public sealed class ConductorAction
    {
        public ConductorAction(Action a)
        {
            var st = new StackTrace();
            StackFrame[] frames = st.GetFrames();
            CalledFromMethod = "UnknownMethod";
            if (frames != null)
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    if (frames[i].GetMethod().Name == MethodBase.GetCurrentMethod().Name)
                    {
                        if (i + 2 < frames.Length)
                        {
                            CalledFromMethod = frames[i + 2].GetMethod().Name;
                            break;
                        }
                    }
                }
            }
            Action = a;
        }

        public Action Action { get; private set; }
        public String CalledFromMethod { get; private set; }
    }
}