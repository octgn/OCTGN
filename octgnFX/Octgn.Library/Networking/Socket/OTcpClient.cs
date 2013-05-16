using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Library.Networking.Socket
{
    using System.Net;
    using System.Net.Sockets;

    using log4net;

    public class OTcpClient
    {
        public event Action<EventArgs,OTcpClient> Closed;

        private readonly ILog log = LogManager.GetLogger(typeof(OTcpClient));

        public EndPoint EndPoint { get{return Client.Client.RemoteEndPoint;} }

        protected TcpClient Client { get; private set; }

        public OTcpClient(TcpClient client, OTcpListener listener)
        {
            this.log.InfoFormat("Created from TcpClient {0}",client.Client.RemoteEndPoint);
            Client = client;
            listener.Stopping += ListenerOnStopping;
        }

        public void Close()
        {
            try
            {
                Client.Close();
            }
            catch (Exception e)
            {
                log.Error("Close failure",e);
            }
            this.OnClosed();
        }

        #region Callbacks
        private void ListenerOnStopping(EventArgs eventArgs, OTcpListener oTcpListener)
        {
            log.Info("Listener stopping, stopping client.");
            this.Close();
        }
        #endregion

        #region Event Invocators
        private void OnClosed()
        {
            var handler = this.Closed;
            if (handler != null)
            {
                handler(EventArgs.Empty, this);
            }
        }
        #endregion
    }
}
