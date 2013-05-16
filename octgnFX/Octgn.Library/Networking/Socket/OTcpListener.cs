using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Octgn.Library.Networking.Socket
{
    using System.IO;
    using System.IO.Pipes;
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
        private bool started;
        private bool stop;

        #region Events
        public event Action<EventArgs, OTcpListener> Stopping;
        #endregion

        public OTcpListener( IPEndPoint bind )
        {
            if(bind == null)
                throw new ArgumentNullException("bind cannot be null.");
            Listener = new TcpListener(bind);
        }

        public void Start()
        {
            lock (this)
            {
                if (started) return;
                started = true;
                this.log.InfoFormat("Starting on {0}", ((IPEndPoint)Listener.LocalEndpoint));
                Listener.Start(5);
                Listener.BeginAcceptTcpClient(this.AcceptClientCallback, Listener);
                this.log.InfoFormat("OTcpListener started on {0}", this.Listener.LocalEndpoint);
            }
        }

        #region Callbacks

        private void AcceptClientCallback(IAsyncResult res)
        {
            try
            {
                var c = new OTcpClient(Listener.EndAcceptTcpClient(res),this);
                c.Closed += this.OnClosed;
                Interlocked.Increment(ref ClientCount);
                this.log.InfoFormat("Client connected on {0}",c.EndPoint);
                this.log.InfoFormat("Client Count {0}", ClientCount);
                Listener.BeginAcceptTcpClient(this.AcceptClientCallback, Listener);
            }
            catch (Exception e)
            {
                this.log.Error("Client connection error",e);
            }
        }

        private void OnClosed(EventArgs eventArgs, OTcpClient oTcpClient)
        {
            Interlocked.Decrement(ref ClientCount);
            this.log.InfoFormat("Client Count {0}", ClientCount);
        }

        #endregion

        #region Event Invocators
        private void OnStopping(OTcpListener listener)
        {
            var handler = this.Stopping;
            if (handler != null)
            {
                handler(EventArgs.Empty, listener);
            }
        }
        #endregion

    }
}
