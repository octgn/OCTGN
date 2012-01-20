using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.Lobby;
using Skylabs.Lobby.Sockets;
using Google.GData.Client;
using System.Net;
using System.Diagnostics;
using Skylabs.Net;
using Skylabs.Lobby.Threading;
using System.Reflection;

namespace Skylabs.LobbyServer
{
    public class Client2 : IEqualityComparer<Client2>,IDisposable
    {
        public List<User>Friends
        {
            get
            {
                lock (ClientLocker)
                    return _friends;
            }
        }
        public bool IsDisposed { get; private set; }
        /// <summary>
        /// A unique ID of the client. Server.cs decides this
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Is the user logged in?
        /// </summary>
        public bool LoggedIn { get; private set; }
        /// <summary>
        /// A MySql class to handle all of the database work.
        /// </summary>
        private MySqlCup Cup { get; set; }
        /// <summary>
        /// The user information on the currently connected user
        /// </summary>
        public User Me {get;private set; }

        private SkySocket2 Socket;

        private object ClientLocker = new object();

        private List<User> _friends;

        private bool _stopping;

        private Conductor Conductor;

        private readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;

        public Client2(SkySocket2 socket,int id)
        {
            Conductor = new Lobby.Threading.Conductor();
            _stopping = false;
            Id = id;
            LoggedIn = false;
            Cup = new MySqlCup(Program.Settings["dbUser"], Program.Settings["dbPass"], Program.Settings["dbHost"], Program.Settings["db"]);
            Socket = socket;
            Socket.OnMessageReceived += new SkySocket2.MessageReceived(Socket_OnMessageReceived);
            Socket.OnConnectionClosed += new SkySocket2.ConnectionClosed(Socket_OnConnectionClosed);
            IsDisposed = false;
            _friends = new List<User>();
        }

        void Socket_OnConnectionClosed(SkySocket2 socket)
        {
            Socket.Dispose();
            //TODO make and call disconnected event.
        }

        void Socket_OnMessageReceived(SkySocket2 socket, Net.SocketMessage message)
        {
            string head = message.Header.ToLower();
            switch (head)
            {
                case "login":
                    {
                        string email = (string)message["email"];
                        string token = (string)message["token"];
                        UserStatus stat = (UserStatus)message["status"];
                        Login(email, token, stat);
                        break;
                    }
            }
        }
        public void Stop()
        {
            lock (ClientLocker)
            {
                if (!_stopping)
                {
                    Trace.TraceInformation("Stopping Client.");
                    LoggedIn = false;
                    Socket.WriteMessage(new SocketMessage("end"));
                    Socket.Stop();
                }
            }
        }
        void Login(string email, string token, UserStatus status)
        {
            lock (ClientLocker)
            {
                SocketMessage sm;
                if (email == null || token == null)
                {
                    //Authenticate Google Token
                    try
                    {
                        String appName = "skylabs-LobbyServer-" + Version;
                        Service s = new Service("code", appName);
                        s.SetAuthenticationToken(token);
                        s.QueryClientLoginToken();
                    }
                    catch (AuthenticationException e)
                    {
                        sm = new SocketMessage("loginfailed");
                        sm.AddData("message", "Invalid Token");
                        Socket.WriteMessage(sm);
                        Trace.TraceError("Login attempt with invalid token.");
                        return;
                    }
                    catch (WebException e)
                    {
                        sm = new SocketMessage("loginfailed");
                        sm.AddData("message", "Server error");
                        Socket.WriteMessage(sm);
                        Trace.TraceError("Client.Login: ",e);
                        return;
                    }
                    User u = Cup.GetUser(email);
                    string[] emailparts = email.Split('@');
                    if (u == null)
                    {
                        if (!Cup.RegisterUser(email, emailparts[0]))
                        {
                            LoggedIn = false;
                            sm = new SocketMessage("loginfailed");
                            sm.AddData("message", "Server error");
                            Socket.WriteMessage(sm);
                            return;
                        }
                    }
                    u = Cup.GetUser(email);
                    if (u == null)
                    {
                        LoggedIn = false;
                        sm = new SocketMessage("loginfailed");
                        sm.AddData("message", "Server error");
                        Socket.WriteMessage(sm);
                        return;
                    }
                    int banned = Cup.IsBanned(u.Uid, Socket.RemoteEndPoint);
                    if (banned == -1)
                    {
                        Tuple<int, int> res = Server.StopAndRemoveAllByUID(u.Uid);
                        Me = u;
                        if (status == UserStatus.Unknown || status == UserStatus.Offline)
                            status = UserStatus.Online;
                        Me.Status = status;
                        sm = new SocketMessage("loginsuccess");
                        sm.AddData("me", Me);
                        Socket.WriteMessage(sm);
                        if (res.Item1 == 0)
                            Server.OnUserEvent(status, this);
                        _friends = Cup.GetFriendsList(Me.Uid);

                        LoggedIn = true;
                        SendFriendsList();
                        SendFriendRequests();
                        SendHostedGameList();
                        return;
                    }
                    else
                    {
                        sm = new SocketMessage("banned");
                        sm.AddData(new NameValuePair("end", banned));
                        Socket.WriteMessage(sm);
                        Conductor.Add(()=>Stop());
                        LoggedIn = false;
                        return;
                    }
                }
                else
                {
                    Trace.TraceError("Login attempted failed. Email or token null.");
                }
                sm = new SocketMessage("loginfailed");
                sm.AddData("message", "Server error");
                Socket.WriteMessage(sm);
            }
        }
        private void SendHostedGameList()
        {
            lock (ClientLocker)
            {
                List<Skylabs.Lobby.HostedGame> sendgames = Gaming.GetLobbyList();
                SocketMessage sm = new SocketMessage("gamelist");
                sm.AddData("list", sendgames);
                Socket.WriteMessage(sm);
            }
        }
        private void SendFriendRequests()
        {
            lock (ClientLocker)
            {
                List<int> r = Cup.GetFriendRequests(Me.Email);
                if (r == null)
                    return;
                foreach (int e in r)
                {
                    SocketMessage smm = new SocketMessage("friendrequest");
                    User u = Cup.GetUser(e);
                    smm.AddData("user", u);
                    Socket.WriteMessage(smm);
                }
            }
        }
        private void SendFriendsList()
        {
            lock (ClientLocker)
            {
                SocketMessage sm = new SocketMessage("friends");
                foreach (User u in _friends)
                {
                    Client c = Server.GetOnlineClientByUid(u.Uid);
                    User n;
                    if (c == null)
                    {
                        n = u;
                        n.Status = UserStatus.Offline;
                    }
                    else
                    {
                        n = c.Me;
                        if (n.Status == UserStatus.Invisible)
                            n.Status = UserStatus.Offline;
                    }
                    sm.AddData(n.Uid.ToString(), n);
                }
                Socket.WriteMessage(sm);
            }
            
        }
        public bool Equals(Client2 x, Client2 y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Client2 obj)
        {
            return obj.Id;
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Socket.OnMessageReceived -= Socket_OnMessageReceived;
                _friends.Clear();
                _friends = null;
                Cup = null;
                if (Socket != null)
                    Socket.Dispose();
                Conductor.Dispose();
                IsDisposed = true;
            }
        }
    }
}
