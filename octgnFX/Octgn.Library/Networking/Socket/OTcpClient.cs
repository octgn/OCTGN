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
        public event Action<EventArgs,OTcpClient> ClientClosed;

        private readonly ILog log = LogManager.GetLogger(typeof(OTcpClient));

        public EndPoint EndPoint { get
        {
            return Client.Client.RemoteEndPoint;
        } }

        protected TcpClient Client { get; private set; }

        public OTcpClient(TcpClient client)
        {
            this.log.InfoFormat("Created from TcpClient {0}",client.Client.RemoteEndPoint);
            Client = client;
        }

        #region Event Invocators
        protected void OnClientClosed(OTcpClient client)
        {
            var handler = this.ClientClosed;
            if (handler != null)
            {
                handler(EventArgs.Empty, client);
            }
        }
        #endregion
    }
}
