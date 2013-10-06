namespace Octgn.Core.Networking
{
    using System.Net.Sockets;

    public class SocketReceiveBundle
    {
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public TcpClient Client;

        public SocketReceiveBundle(TcpClient client)
        {
            this.Client = client;
        }

        ~SocketReceiveBundle()
        {
            this.Client = null;
            this.Buffer = null;
        }
    }
}