using System.Net.Sockets;
using Skylabs.Net.Sockets;

namespace Skylabs.Lobby
{
    public class LobbyClient : SkySocket
    {
        public LobbyClient()
            : base()
        {
        }

        public LobbyClient(TcpClient c)
            : base(c)
        {
        }

        public override void OnMessageReceived(Net.SocketMessage sm)
        {
        }

        public override void OnDisconnect(Net.DisconnectReason reason)
        {
        }
    }
}