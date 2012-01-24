using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Skylabs.Lobby.Threading
{
    public sealed class Conductor2 : IDisposable
    {
        public bool IsDisposed { get; private set; }

        private Queue<ConductorAction> Q;

        private Timer DelegateTimer;

        private object Locker = new object();

        public Conductor2()
        {
            Q = new Queue<ConductorAction>();
            IsDisposed = false;
            DelegateTimer = new Timer(DelegateTimerTick, null, 5, 5);
        }

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
                ConductorAction ca;
                try
                {
                    if (Q.Count > 0)
                    {
                        ca = Q.Dequeue();
                        ca.Action.BeginInvoke(InvokeDone,null);
                    }
                }
                catch (Exception e)
                {
                    if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
                }
            }
        }
        private void InvokeDone(IAsyncResult result)
        {
            
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
    public sealed class ConductorAction
    {
        public Action Action { get; private set; }
        public String CalledFromMethod { get; private set; }
        public ConductorAction(Action a)
        {
            StackTrace st = new StackTrace();
            StackFrame[] frames = st.GetFrames();
            CalledFromMethod = "UnknownMethod";
            for (int i = 0; i < frames.Length; i++)
            {
                if (frames[i].GetMethod().Name == System.Reflection.MethodInfo.GetCurrentMethod().Name)
                {
                    if (i + 2 < frames.Length)
                    {
                        CalledFromMethod = frames[i + 2].GetMethod().Name;
                        break;
                    }
                }
            }
            Action = a;
        }
    }
}
