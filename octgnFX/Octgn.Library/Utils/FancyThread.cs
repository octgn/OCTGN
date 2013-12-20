namespace Octgn.Library.Utils
{
    using System;
    using System.Threading;

    public class FancyThread : IDisposable
    {
        public bool Running
        {
            get
            {
                return this.running;
            }
        }

        public Thread Thread
        {
            get
            {
                return this.watchThread;
            }
        }

        private readonly string name;

        private Thread watchThread;

        private readonly Action<FancyThread> threadAction;

        private readonly Timer watchTimer;

        private volatile bool running;

        private volatile bool disposed;

        private DateTime lastCheckInTime;

        public FancyThread(string name, Action<FancyThread> threadAction)
        {
            this.name = name;
            this.disposed = false;
            this.running = false;
            this.lastCheckInTime = DateTime.Now;
            this.threadAction = threadAction;
            this.watchTimer = new Timer(this.OnTimerElapsed, null, 2000, -1);
			this.SetupThread();
        }

        public void CheckIn()
        {
            this.lastCheckInTime = DateTime.Now;
        }

        public void Start()
        {
            if (this.disposed) throw new ObjectDisposedException(this.GetType().Name);
            this.running = true;
        }

        public void Stop()
        {
            if (this.disposed) throw new ObjectDisposedException(this.GetType().Name);
            this.running = false;
        }

        private void SetupThread()
        {
            this.watchThread = new Thread(this.Run);
            this.watchThread.Name = this.name;
            this.watchThread.Start();
        }

        private void Run()
        {
            // No exception handling in this method on purpose
            //  (Except ThreadAbort, because it allows us to reset the
            //  thread...but no other catches should go in here)
            try
            {
                while (!this.disposed)
                {
                    if (this.running)
                    {
                        this.CheckIn();
                        this.threadAction.Invoke(this);
                        this.CheckIn();
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                if (!this.disposed) new Action(this.SetupThread).BeginInvoke(null, null);
            }
        }

        private void OnTimerElapsed(object state)
        {
            if (this.disposed) return;

            if (DateTime.Now > this.lastCheckInTime.AddMilliseconds(10000) && this.running)
            {
                this.watchThread.Abort();
            }

            this.watchTimer.Change(2000, -1);
        }

        public void Dispose()
        {
            this.disposed = true;
            this.running = false;
            this.watchTimer.Dispose();
            this.watchThread.Join(11000);
            this.watchThread.Abort();
        }
    }
}