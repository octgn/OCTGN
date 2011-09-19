using System.Net.Sockets;
using Skylabs.Net;
using Skylabs.Net.Sockets;

namespace Skylabs.Lobby
{
    public class LobbyClient : SkySocket
    {
        public delegate void LoginFinished(bool success);

        private LoginFinished OnLoginFinished;

        public LobbyClient()
            : base()
        {
        }

        public LobbyClient(TcpClient c)
            : base(c)
        {
        }

        public void Login(LoginFinished onFinish, string email, string password)
        {
            if(Connected)
            {
                OnLoginFinished = onFinish;
                SocketMessage sm = new SocketMessage("login");
                sm.Add_Data(new NameValuePair("email", email));
                sm.Add_Data(new NameValuePair("password", password));
                WriteMessage(sm);
            }
        }

        public override void OnMessageReceived(Net.SocketMessage sm)
        {
            switch(sm.Header.ToLower())
            {
                case "loginsuccess":
                    OnLoginFinished.Invoke(true);
                    break;
                case "loginfailed":
                    OnLoginFinished.Invoke(false);
                    break;
            }
        }

        public override void OnDisconnect(Net.DisconnectReason reason)
        {
        }
    }
}