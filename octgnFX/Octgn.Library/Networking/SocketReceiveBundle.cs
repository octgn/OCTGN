using System.Net.Sockets;

namespace Octgn.Library.Networking
{
    public class SocketReceiveBundle
    {
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public TcpClient TcpClient;
        public UdpClient UdpClient;

        public SocketReceiveBundle(TcpClient tcpClient)
        {
            this.TcpClient = tcpClient;
            this.UdpClient = null;
        }

        public SocketReceiveBundle(UdpClient udpClient)
        {
            this.TcpClient = null;
            this.UdpClient = udpClient;
        }

        ~SocketReceiveBundle()
        {
            this.TcpClient = null;
            this.UdpClient = null;
            this.Buffer = null;
        }
    }
}