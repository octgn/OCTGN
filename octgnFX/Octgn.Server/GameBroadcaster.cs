using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Timers;
using log4net;

namespace Octgn.Core.Networking
{
    public class GameBroadcaster : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public bool IsBroadcasting{ get; internal set; }

        internal UdpClient Client { get; set; }
        internal Timer SendTimer { get; set; }

        public GameBroadcaster()
        {
            IsBroadcasting = false;
            SendTimer = new Timer(1000);
            SendTimer.Elapsed += SendTimerOnElapsed;
        }

        public void StartBroadcasting()
        {
            lock (this)
            {
                if (IsBroadcasting)
                {
                    return;
                }
                try
                {
                    if (Client == null)
                    {
                        Client = new UdpClient(new IPEndPoint(IPAddress.Broadcast, 9999));
                    }

                    IsBroadcasting = true;
                }
                catch (Exception e)
                {
                    Log.Error("Error broadcasting", e);
                }
            }
        }

        public void StopBroadcasting()
        {
            lock (this)
            {
                if (!IsBroadcasting)
                    return;

                try
                {
                    Client.Close();
                }
                catch
                {
                }

                Client = null;
                IsBroadcasting = false;
            }
        }

        private void SendTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (!IsBroadcasting)
                return;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            try
            {
                StopBroadcasting();
            }
            catch{}
            this.SendTimer.Elapsed -= SendTimerOnElapsed;
            try
            {
                this.SendTimer.Dispose();
            }
            catch { }
        }

        #endregion
    }
}