namespace Octgn.Core.Networking
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using log4net;

    public abstract class ReconnectingSocketBase : SocketBase
    {
		public int MaxRetryCount { get; internal set; }
		public int RetryCount { get; internal set; }
		public bool Reconnecting { get; internal set; }
        public TimeSpan TimeoutTime { get; internal set; }
        internal bool ForcedDisconnect = false;

        protected ReconnectingSocketBase( int maxRetryCount , TimeSpan timeoutTime, ILog log)
            : base(log)
        {
            if (maxRetryCount < 0) maxRetryCount = 0;
            TimeoutTime = timeoutTime;
            if (timeoutTime == TimeSpan.Zero)
                TimeoutTime = TimeSpan.MaxValue;
            this.MaxRetryCount = maxRetryCount;
            this.RetryCount = 0;
            this.Reconnecting = false;
        }

        public void ForceDisconnect()
        {
            this.ForcedDisconnect = true;
            this.Reconnecting = false;
			this.Disconnect();
            this.ForcedDisconnect = false;
        }

        public override void OnConnectionEvent(object sender, SocketConnectionEvent e)
        {
			Log.DebugFormat("OnConnectionEvent {0}",e);
            if (e == SocketConnectionEvent.Disconnected && !this.ForcedDisconnect)
            {
                Task.Factory.StartNew(this.DoReconnect);
                return;
            }
            if (e != SocketConnectionEvent.Disconnected)
			{
			    this.RetryCount = 0;
			    this.Reconnecting = false;
			    this.ForcedDisconnect = false;
			}
        }

        internal void DoReconnect()
        {
            if (this.ForcedDisconnect) return;
            Log.Debug("DoReconnect");
            this.Reconnecting = true;
            var startTime = DateTime.Now;
            while (this.RetryCount < this.MaxRetryCount || this.MaxRetryCount == 0 || new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalSeconds < TimeoutTime.TotalSeconds)
            {
                try
                {
                    if (this.Reconnecting == false)
                    {
                        Log.Debug("DoReconnect Finished due to ForceDisconnect");
                        break;
                    }
                    Log.Debug("DoReconnect Trying to reconnect");
                    this.Connect();
                    break;
                }
                catch (Exception e)
                {
                    Log.Error("DoReconnect", e);
                }
                this.RetryCount++;
                Thread.Sleep(1000);
            }
            this.Reconnecting = false;
            this.RetryCount = 0;
        }
    }
}