using System;
using System.Net.Sockets;
using System.Reflection;
using log4net;
using Octgn.Library;
using Octgn.Library.Networking;

namespace Octgn.Online.MessageBroker
{
    class Program
    {
        static void Main(string[] args)
        {
        }
    }
    public class StompServer : SocketBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal DateTime LastPingTime;

        public StompServer(TcpClient client)
            : base(new DeadLog())
        {
            client.Client.SendTimeout = 4000;
            this.Setup(client, new ServerMessageProcessor());
            LastPingTime = DateTime.Now;
        }

        public void OnPingReceived()
        {
            LastPingTime = DateTime.Now;
        }

        public override void OnConnectionEvent(object sender, SocketConnectionEvent e)
        {
            Log.DebugFormat("ConnectionEvent {0} {1}", this.EndPoint, e);
            switch (e)
            {
                case SocketConnectionEvent.Disconnected:
                    //var c = State.Instance.GetClient(this.Client);
                    //if (c != null)
                    //    c.OnDisconnect(true);
                    break;
                case SocketConnectionEvent.Connected:
                    break;
                case SocketConnectionEvent.Reconnected:
                    break;
            }
        }

        public override void OnDataReceived(object sender, byte[] data)
        {
            //lock (State.Instance.Handler)
            //    State.Instance.Handler.ReceiveMessage(data.Skip(4).ToArray(), this);
        }
    }

    public class ServerMessageProcessor : SocketMessageProcessorBase
    {
        public override int ProcessBuffer(byte[] data)
        {
            if (data.Length < 4) return 0;
            var length = data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24;
            if (data.Length < length) return 0;
            return length;
        }
    }
}
