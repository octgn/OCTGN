namespace Octgn.Utils
{
    using System;
    using System.Threading;

    using Octgn.Library;
    using Octgn.Networking;

    public class CompoundCall
    {
        private Action currentCall;
        private bool running;
        private DateTime endTime;
        private int curMuted = 0;

        public void Call(Action call)
        {
            curMuted = Program.Client.Muted;
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
                            using (new Mute(curMuted)) 
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