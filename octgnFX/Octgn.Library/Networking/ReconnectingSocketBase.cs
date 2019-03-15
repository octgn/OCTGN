/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Octgn.Library.Networking
{
    public abstract class ReconnectingSocketBase : SocketBase
    {
        private static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public int MaxRetryCount { get; internal set; }
        public int RetryCount { get; internal set; }
        public bool Reconnecting { get; internal set; }
        public TimeSpan TimeoutTime { get; internal set; }
        internal bool ForcedDisconnect = false;

        private bool _reportedDisconnect = false;

        protected ReconnectingSocketBase(int maxRetryCount, TimeSpan timeoutTime)
        {
            if (maxRetryCount < 0) maxRetryCount = 0;
            TimeoutTime = timeoutTime;
            if (timeoutTime == TimeSpan.Zero)
                TimeoutTime = TimeSpan.MaxValue;
            this.MaxRetryCount = maxRetryCount;
            this.RetryCount = 0;
            this.Reconnecting = false;
            this._reportedDisconnect = false;
        }

        public void Shutdown()
        {
            this.ForcedDisconnect = true;
            this.Reconnecting = false;
            this.Disconnect();
            this.ForcedDisconnect = false;
            this._reportedDisconnect = false;
        }

        public override void OnConnectionEvent(object sender, SocketConnectionEvent e)
        {
            Log.DebugFormat("OnConnectionEvent {0}", e);
            if (e == SocketConnectionEvent.Disconnected && !this.ForcedDisconnect)
            {
                if (_reportedDisconnect == false)
                {
                    _reportedDisconnect = true;
                    Log.ErrorFormat("Disconnect Event {0}",this.EndPoint);
                }
                Task.Factory.StartNew(this.DoReconnect);
                return;
            }
            if (e != SocketConnectionEvent.Disconnected)
            {
                this.RetryCount = 0;
                this.Reconnecting = false;
                this.ForcedDisconnect = false;
                this._reportedDisconnect = false;
            }
        }

        internal async void DoReconnect()
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
                    await this.Connect();
                    break;
                }
                catch (Exception e)
                {
                    Log.Warn("DoReconnect", e);
                }
                this.RetryCount++;
                Thread.Sleep(1000);
            }
            this.Reconnecting = false;
            this.RetryCount = 0;
        }
    }
}