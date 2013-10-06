namespace Octgn.Core.Networking
{
    using System;
    using System.Threading.Tasks;

    public abstract class ReconnectingSocketBase : SocketBase
    {
		public int MaxRetryCount { get; internal set; }
		public int RetryCount { get; internal set; }
		public bool Reconnecting { get; internal set; }
        internal bool ForcedDisconnect = false;

        protected ReconnectingSocketBase( int maxRetryCount )
        {
            if (maxRetryCount < 0) maxRetryCount = 0;
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
            this.Reconnecting = true;
            while (this.RetryCount < this.MaxRetryCount || this.MaxRetryCount == 0)
            {
                try
                {
                    if (this.Reconnecting == false) break;
                    this.Connect();
                    break;
                }
                catch (Exception e)
                {
                    Log.Error("DoReconnect", e);
                }
                this.RetryCount++;
            }
            this.Reconnecting = false;
            this.RetryCount = 0;
        }
    }
}