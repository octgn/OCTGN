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
    public class Client : IEqualityComparer<Client>,IDisposable
    {
        public event EventHandler OnDisconnect;
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

        private SkySocket Socket;

        private object ClientLocker = new object();

        private List<User> _friends;

        private bool _stopping;

        private readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version;

        public Client(SkySocket socket,int id)
        {
            _stopping = false;
            Id = id;
            LoggedIn = false;
            Me = new User();
            Cup = new MySqlCup(Program.Settings["dbUser"], Program.Settings["dbPass"], Program.Settings["dbHost"], Program.Settings["db"]);
            Socket = socket;
            Socket.OnMessageReceived += new SkySocket.MessageReceived(Socket_OnMessageReceived);
            Socket.OnConnectionClosed += new SkySocket.ConnectionClosed(Socket_OnConnectionClosed);
            IsDisposed = false;
            _friends = new List<User>();
        }

        void Socket_OnConnectionClosed(SkySocket socket)
        {
            if(OnDisconnect != null)
                new Action(()=>OnDisconnect.Invoke(this,null)).BeginInvoke(null,null);
            Socket.Dispose();
            //TODO make and call disconnected event.
        }

        void Socket_OnMessageReceived(SkySocket socket, Net.SocketMessage message)
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
                case "addfriend":
                    {
                        string email = (String)message["email"];
                        RequestFriend(email);
                        break;
                    }
                case "acceptfriend":
                    {
                        int? uid = (int?)message["uid"];
                        bool? accept = (bool?)message["accept"];
                        if(uid == null || accept == null)
                            break;
                        AcceptFriend((int)uid,(bool)accept);
                        break;
                    }
                case "end":
                    {
                        new Action(()=>Stop()).BeginInvoke(null,null);
                        break;
                    }
                case "displayname":
                    {
                        string s = (string)message["name"];
                        if(message != null)
                            SetDisplayName(s);
                        break;
                    }
                case "status":
                    {
                        UserStatus u = (UserStatus)message["status"];
                        if(u != UserStatus.Offline && u != UserStatus.Unknown)
                        {
                            Me.Status = u;
                            new Action(()=>Server.OnUserEvent(u,this)).BeginInvoke(null,null);
                        }
                        break;
                    }
                case "hostgame":
                    {
                        Guid g = (Guid)message["game"];
                        Version v = (Version)message["version"];
                        string n = (string)message["name"];
                        string pass = (string)message["pass"];
                        if(g != null && v != null && n != null && pass != null)
                            HostGame(g,v,n,pass);
                        break;
                    }
                case "gamestarted":
                    {
                        Gaming.StartGame((int)message["port"]);
                        new Action(()=>Server.AllUserMessage(message.Clone() as SocketMessage)).BeginInvoke(null,null);
                        break;
                    }
                case "customstatus":
                    {
                        string s = (string)message["customstatus"];
                        if(s != null)
                            SetCustomStatus(s);
                        break;
                    }
                case "joinchatroom":
                    {
                        new Action(()=>Chatting.JoinChatRoom(this,message)).BeginInvoke(null,null);
                        break;
                    }
                case "addusertochat":
                    {
                        new Action(()=>Chatting.AddUserToChat(this, message)).BeginInvoke(null,null);
                        break;
                    }
                case "twopersonchat":
                    {
                        new Action(()=>Chatting.TwoPersonChat(this, message)).BeginInvoke(null,null);
                        break;
                    }
                case "leavechat":
                    {
                        long? rid = (long?)message["roomid"];
                        if (rid != null)
                        {
                            long rid2 = (long)rid;
                            new Action(()=>Chatting.UserLeaves(Me.Clone() as User, rid2)).BeginInvoke(null,null);
                        }
                        break;
                    }
                case "chatmessage":
                    {
                        new Action(()=>Chatting.ChatMessage(this,message)).BeginInvoke(null,null);
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
                    _stopping = true;
                    Trace.TraceInformation("Stopping Client.");
                    LoggedIn = false;
                    Socket.WriteMessage(new SocketMessage("end"));
                    Socket.Stop();
                }
            }
        }
        void SetCustomStatus(string status)
        {
            lock(ClientLocker)
            {
                if(status.Length > 200)
                    status = status.Substring(0, 197) + "...";
                if(Cup.SetCustomStatus(Me.Uid, status))
                {
                    Me.CustomStatus = status;
                    new Action(()=>Server.OnUserEvent(Me.Status, this, false)).BeginInvoke(null,null);
                }
            }
        }
        void HostGame(Guid g, Version v, String name, String pass)
        {
            lock(ClientLocker)
            {
                int port = Gaming.HostGame(g,v,name,pass,Me);
                SocketMessage som = new SocketMessage("hostgameresponse");
                som.AddData("port", port);
                Socket.WriteMessage(som);

                if (port != -1 )
                {
                    SocketMessage smm = new SocketMessage("GameHosting");
                    smm.AddData("name", name);
                    smm.AddData("passrequired", !String.IsNullOrEmpty(pass));
                    smm.AddData("guid", g);
                    smm.AddData("version", v);
                    smm.AddData("hoster", Me);
                    smm.AddData("port", port);
                    new Action(()=>Server.AllUserMessage(smm.Clone() as SocketMessage)).BeginInvoke(null,null);
                }
            }
        }
        public void WriteMessage(SocketMessage sm)
        {
            lock (ClientLocker)
            {
                Socket.WriteMessage(sm);
            }
        }
        public void OnUserEvent(UserStatus e, User theuser)
        {
            lock (ClientLocker)
            {
                //if (theuser.Equals(Me))
                    //return;
                SocketMessage sm = new SocketMessage("status");
                if (e == UserStatus.Invisible)
                    e = UserStatus.Offline;
                theuser.Status = e;
                sm.AddData("user", theuser);
                new Action(()=>WriteMessage(sm)).BeginInvoke(null,null);
            }
        }
        void SetDisplayName(string name)
        {
            lock(ClientLocker)
            {
                if (name.Length > 60)
                    name = name.Substring(0, 57) + "...";
                else if (String.IsNullOrWhiteSpace(name))
                    name = Me.DisplayName;
                if (Cup.SetDisplayName(Me.Uid, name))
                {
                    Me.DisplayName = name;
                    new Action(()=>Server.OnUserEvent(Me.Status, this, false)).BeginInvoke(null,null);
                }
            }
        }
        void AcceptFriend(int uid,bool accept)
        {
            lock(ClientLocker)
            {
                User requestee = Cup.GetUser(uid);
                if (requestee == null)
                    return;
                if (accept)
                {
                    //Add friend to this list
                    if(!_friends.Contains(requestee))
                        _friends.Add(requestee);
                    //Add you to friends list
                    Client c = Server.GetOnlineClientByUid(requestee.Uid);
                    if (c != null)
                    {
                        c.AddFriend(Me);
                    }
                    //Add to database
                    Cup.AddFriend(Me.Uid, requestee.Uid);
                }
                //Remove any database friend requests
                Cup.RemoveFriendRequest(requestee.Uid, Me.Email);
                if (!Me.Equals(requestee))
                    Cup.RemoveFriendRequest(Me.Uid, requestee.Email);
                new Action(()=>SendFriendsList()).BeginInvoke(null,null);
            }
        }
        public void AddFriend(User friend)
        {
            lock (ClientLocker)
            {
                if (!_friends.Contains(friend))
                {
                    _friends.Add(friend);
                    if (LoggedIn)
                        new Action(() => SendFriendsList()).BeginInvoke(null,null);
                }
            }
        }
        void RequestFriend(string email)
        {
            lock (ClientLocker)
            {
                if (email == null)
                    return;
                if (String.IsNullOrWhiteSpace(email))
                    return;
                if (!Verify.IsEmail(email))
                    return;
                email = email.ToLower();
                Cup.AddFriendRequest(Me.Uid, email);
                SocketMessage smm = new SocketMessage("friendrequest");
                smm.AddData("user", Me);
                Server.WriteMessageToClient(smm, email);
            }
        }
        void Login(string email, string token, UserStatus status)
        {
            lock (ClientLocker)
            {
                SocketMessage sm;
                if (email != null && token != null)
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
                        Debug.WriteLine(e);
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
                            new Action(()=>Server.OnUserEvent(status, this)).BeginInvoke(null,null);
                        _friends = Cup.GetFriendsList(Me.Uid);

                        LoggedIn = true;
                        new Action(()=>SendFriendsList()).BeginInvoke(null,null);
                        new Action(()=>SendFriendRequests()).BeginInvoke(null,null);
                        new Action(()=>SendHostedGameList()).BeginInvoke(null,null);
                        return;
                    }
                    else
                    {
                        sm = new SocketMessage("banned");
                        sm.AddData("end", banned);
                        Socket.WriteMessage(sm);
                        new Action(()=>Stop()).BeginInvoke(null,null);
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
        public bool Equals(Client x, Client y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Client obj)
        {
            return obj.Id;
        }

        public void Dispose()
        {
            lock(ClientLocker)
            {
                if (!IsDisposed)
                {
                    Socket.OnMessageReceived  -= Socket_OnMessageReceived;
                    Socket.OnConnectionClosed -= Socket_OnConnectionClosed;
                    _friends.Clear();
                    _friends = null;
                    Cup = null;
                    if (Socket != null)
                        Socket.Dispose();
                    IsDisposed = true;
                }
            }
        }
    }
}
