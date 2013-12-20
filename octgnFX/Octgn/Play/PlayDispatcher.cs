namespace Octgn.Play
{
    using System;
    using System.Reflection;
    using System.Threading;

    using log4net;

    using Octgn.Library.Utils;

    public class PlayDispatcher : IDisposable
    {
        #region Singleton

        internal static PlayDispatcher SingletonContext { get; set; }

        private static readonly object PlayDispatcherSingletonLocker = new object();

        public static PlayDispatcher Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (PlayDispatcherSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new PlayDispatcher();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly System.Collections.Concurrent.ConcurrentQueue<PlayDispatcherAction> actions; 

        private readonly FancyThread thread;

        private volatile bool running = true;

        public bool OnDispatcherThread
        {
            get
            {
                return Thread.CurrentThread == thread.Thread;
            }
        }

        public System.Windows.Threading.Dispatcher UIDispacher { get; set; }

        internal PlayDispatcher()
        {
            this.actions = new System.Collections.Concurrent.ConcurrentQueue<PlayDispatcherAction>();
            thread = new FancyThread("PlayDispatcher",PlayDispatcherRun);
			thread.Start();
        }

        private void PlayDispatcherRun(FancyThread t)
        {
			// No exception handling here on purpose
			// No try catch blocks should go in this method.
            while (running)
            {
                PlayDispatcherAction a;
                while (this.actions.TryDequeue(out a))
                {
                    t.CheckIn();
					a.Invoke();
                    t.CheckIn();
                }
                Thread.Sleep(1);
                t.CheckIn();
            }
        }

        public void Invoke(Action action)
        {
            var a = new PlayDispatcherAction(action);
            if (OnDispatcherThread)
            {
                this.thread.CheckIn();
                a.Invoke();
                this.thread.CheckIn();
            }
            else
            {
				this.actions.Enqueue(a);
				a.Wait();
            }
        }

        public T Invoke<T>(Func<T> action)
        {
            var a = new PlayDispatcherAction(action);
            if (OnDispatcherThread)
            {
                this.thread.CheckIn();
                a.Invoke();
                this.thread.CheckIn();
                return (T)a.ReturnValue;
            }
            this.actions.Enqueue(a);
            return (T)a.Wait();
        }

        public T Invoke<T, T1>(Func<T1, T> action, T1 arg1)
        {
            var a = new PlayDispatcherAction(action, arg1);
            if (OnDispatcherThread)
            {
                this.thread.CheckIn();
                a.Invoke();
                this.thread.CheckIn();
                return (T)a.ReturnValue;
            }
            this.actions.Enqueue(a);
            return (T)a.Wait();
        }

        public T Invoke<T,T1,T2>(Func<T1,T2,T> action, T1 arg1, T2 arg2)
        {
            var a = new PlayDispatcherAction(action, arg1, arg2);
            if (OnDispatcherThread)
            {
                this.thread.CheckIn();
                a.Invoke();
                this.thread.CheckIn();
                return (T)a.ReturnValue;
            }
            this.actions.Enqueue(a);
            return (T)a.Wait();
        }

        public T Invoke<T,T1,T2,T3>(Func<T1,T2,T3,T> action, T1 arg1, T2 arg2, T3 arg3)
        {
            var a = new PlayDispatcherAction(action, arg1, arg2, arg3);
            if (OnDispatcherThread)
            {
                this.thread.CheckIn();
                a.Invoke();
                this.thread.CheckIn();
                return (T)a.ReturnValue;
            }
            this.actions.Enqueue(a);
            return (T)a.Wait();
        }

        public void BeginInvoke(Action action)
        {
            var a = new PlayDispatcherAction(action);
            this.actions.Enqueue(a);
        }

        public void Dispose()
        {
            running = false;
            thread.Dispose();
        }
    }
}