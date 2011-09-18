using System.Net.Sockets;
using Skylabs.ConsoleHelper;
using Skylabs.Net;
using Skylabs.Net.Sockets;

namespace Octgn.LobbyServer
{
    public class Client : SkySocket
    {
        public int ID { get; private set; }

        public Client(TcpClient client, int id)
            : base(client)
        {
            ID = id;
        }

        public void Stop()
        {
            SocketMessage sm = new SocketMessage("END");
            WriteMessage(sm);
            Close(DisconnectReason.CleanDisconnect);
        }

        public override void OnMessageReceived(SocketMessage sm)
        {
            ConsoleWriter.writeLine(sm.Header, true);
        }

        public override void OnDisconnect(DisconnectReason reason)
        {
        }
    }
}