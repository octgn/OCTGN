using System;
using System.Net.Sockets;
using Skylabs.Net;
using Skylabs.Net.Sockets;

namespace Skylabs.Lobby
{
    public enum LoginResult { Success, Failure, Banned };

    public class LobbyClient : SkySocket
    {
        public delegate void LoginFinished(LoginResult success, DateTime BanEnd);

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
                    OnLoginFinished.Invoke(LoginResult.Success, DateTime.Now);
                    break;
                case "loginfailed":
                    OnLoginFinished.Invoke(LoginResult.Failure, DateTime.Now);
                    break;
                case "banned":
                    string stime = sm["end"];
                    if(stime != null)
                    {
                        int time = int.Parse(stime);

                        OnLoginFinished.Invoke(LoginResult.Banned, Skylabs.ValueConverters.fromPHPTime(time));
                    }
                    break;
            }
        }

        public override void OnDisconnect(Net.DisconnectReason reason)
        {
        }
    }
}