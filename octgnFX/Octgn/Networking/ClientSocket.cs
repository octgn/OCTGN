namespace Octgn.Networking
{
    using System.Net;

    using Octgn.Core.Networking;

    public class ClientSocket : ReconnectingSocketBase
    {
		internal IServerCalls Rpc { get; set; }
		internal Handler Handler { get; set; }

        public ClientSocket(IPAddress address, int port)
            : base(10)
        {
			this.Setup(new IPEndPoint(address,port),new ClientMessageProcessor());
            Handler = new Handler();
            Rpc = new BinarySenderStub(this.Client);
        }

        public override void OnConnectionEvent(object sender, SocketConnectionEvent e)
        {
			base.OnConnectionEvent(sender,e);
        }

        public override void OnDataReceived(object sender, byte[] data)
        {
			Handler.ReceiveMessage(data);
        }
    }

    public class ClientMessageProcessor : SocketMessageProcessorBase
    {
        public override int ProcessBuffer(byte[] data)
        {
            if (data.Length < 4) return 0;
            var length = data[0] | data[1] << 8 | data[2] << 16 | data[3] << 24;
            if (data.Length < length + 4) return 0;
            return length + 4;
        }
    }
}