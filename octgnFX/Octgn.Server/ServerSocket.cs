using System;
using System.Net.Sockets;
using System.Reflection;
using log4net;
using Octgn.Library.Networking;

namespace Octgn.Server
{
    public class ServerSocket : SocketBase
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        internal Server Server;
        internal DateTime LastPingTime;
        internal int PingsReceived;
        public IClientCalls Rpc { get; }
        public BinaryParser Serializer { get; }
        public Handler Handler { get; }

        public ServerSocket(TcpClient client, Server server)
        {
            client.Client.SendTimeout = 4000;
            this.Setup(client,new ServerMessageProcessor());
            Server = server;
            LastPingTime = DateTime.Now;

            Handler = new Handler(server.Context);
            Rpc = new BinarySenderStub(this);
            Serializer = new BinaryParser(this);
        }

        public void OnPingReceived()
        {
            LastPingTime = DateTime.Now;
            PingsReceived++;
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