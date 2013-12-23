namespace Octgn.Utils
{
    using System;
    using System.Threading;

    using Octgn.Library;

    public class CompoundCall
    {
        private Action currentCall;
        private bool running;
        private DateTime endTime;

        public void Call(Action call)
        {
            lock (this)
            {
                currentCall = call;
                endTime = DateTime.Now.AddSeconds(1);
                if (!running)
                {
                    running = true;
					var t = new Thread(this.RunLoop);
                    t.Name = "CompoundCall RunLoop";
                    t.Start();
                }
            }
        }

        private void RunLoop()
        {
            try
            {
                while (true)
                {
                    Thread.Sleep(5);
                    lock (this)
                    {
                        if (DateTime.Now >= endTime)
                        {
							// Do the shit
 							X.Instance.Try(currentCall);
                            return;
                        }
                    }
                }
				
            }
            finally
            {
                lock (this)
                {
                    running = false;
                    currentCall = null;
                    endTime = DateTime.MaxValue;
                }
            }
        }
    }
}