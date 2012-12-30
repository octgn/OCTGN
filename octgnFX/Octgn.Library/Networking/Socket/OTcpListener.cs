using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Library.Networking.Socket
{
    using System.Net;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Threading;

    using log4net;

    public class OTcpListener
    {
        protected TcpListener Listener { get; set; }
        protected Int32 ClientCount;

        private readonly ILog log = LogManager.GetLogger(typeof(OTcpListener));

        public OTcpListener( IPEndPoint bind )
        {
            if(bind == null)
                throw new ArgumentNullException("bind cannot be null.");
            
            Listener = new TcpListener(bind);
        }

        public void Start()
        {
            this.log.InfoFormat("Starting on {0}",((IPEndPoint)Listener.LocalEndpoint).ToString());
            Listener.Start(5);
            Listener.BeginAcceptTcpClient(this.AcceptClientCallback, Listener);
            this.log.InfoFormat("OTcpListener started on {0}",this.Listener.LocalEndpoint.ToString());
        }

        #region Callbacks

        private void AcceptClientCallback(IAsyncResult res)
        {
            try
            {
                var c = new OTcpClient(Listener.EndAcceptTcpClient(res));
                c.ClientClosed += OnClientClosed;
                Interlocked.Increment(ref ClientCount);
                this.log.InfoFormat("Client connected on {0}",c.EndPoint);
                this.log.InfoFormat("Client Count {0}", ClientCount);
            }
            catch (Exception e)
            {
                this.log.Error("AccpetClientCallback",e);
            }
        }

        private void OnClientClosed(EventArgs eventArgs, OTcpClient oTcpClient)
        {
            Interlocked.Decrement(ref ClientCount);
            this.log.InfoFormat("Client Count {0}", ClientCount);
        }

        #endregion

    }
}
