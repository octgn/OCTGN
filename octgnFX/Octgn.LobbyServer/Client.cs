using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Reflection;
using Google.GData.Client;
using Skylabs.Lobby;
using Skylabs.Lobby.Sockets;
using Skylabs.Lobby.Threading;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public class Client : IEqualityComparer<Client>, IDisposable
    {
        private readonly object _clientLocker = new object();
        private readonly SkySocket _socket;
        private readonly Version _version = Assembly.GetExecutingAssembly().GetName().Version;
        private List<User> _friends;

        private bool _stopping;

        public Client(SkySocket socket, int id)
        {
            _stopping = false;
            Id = id;
            LoggedIn = false;
            Me = new User();
            Cup = new MySqlCup(Program.Settings["dbUser"], Program.Settings["dbPass"], Program.Settings["dbHost"],
                               Program.Settings["db"]);
            _socket = socket;
            _socket.OnMessageReceived += Socket_OnMessageReceived;
            _socket.OnConnectionClosed += Socket_OnConnectionClosed;
            IsDisposed = false;
            _friends = new List<User>();
        }

        public List<User> Friends
        {
            get
            {
                lock (_clientLocker)
                    return _friends;
            }
        }

        public bool IsDisposed { get; private set; }

        /// <summary>
        ///   A unique ID of the client. Server.cs decides this
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        ///   Is the user logged in?
        /// </summary>
        public bool LoggedIn { get; private set; }

        /// <summary>
        ///   A MySql class to handle all of the database work.
        /// </summary>
        private MySqlCup Cup { get; set; }

        /// <summary>
        ///   The user information on the currently connected user
        /// </summary>
        public User Me { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            lock (_clientLocker)
            {
                if (IsDisposed) return;
                _socket.OnMessageReceived -= Socket_OnMessageReceived;
                _socket.OnConnectionClosed -= Socket_OnConnectionClosed;
                _friends.Clear();
                _friends = null;
                Cup = null;
                if (_socket != null)
                    _socket.Dispose();
                IsDisposed = true;
            }
        }

        #endregion

        #region IEqualityComparer<Client> Members

        public bool Equals(Client x, Client y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(Client obj)
        {
            return obj.Id;
        }

        #endregion

        public event EventHandler OnDisconnect;

        private void Socket_OnConnectionClosed(SkySocket socket)
        {
            try
            {
                LoggedIn = false;
                if (OnDisconnect != null)
                {
                    OnDisconnect.Invoke(this, null);
                }
                _socket.Dispose();
            }
            catch (Exception e)
            {
                Trace.WriteLine("Socket_OnConnectionClosed error");
                Trace.WriteLine(e.Message);
                Trace.WriteLine(e.StackTrace);
                Trace.WriteLine("-------------------------------------");
            }
        }

        private void Socket_OnMessageReceived(SkySocket socket, SocketMessage message)
        {
            var head = message.Header.ToLower();
            switch (head)
            {
                case "login":
                    {
                        var email = (string) message["email"];
                        var token = (string) message["token"];
                        var stat = (UserStatus) message["status"];
                        Login(email, token, stat);
                        break;
                    }
                case "addfriend":
                    {
                        var email = (String) message["email"];
                        RequestFriend(email);
                        break;
                    }
                case "acceptfriend":
                    {
                        var uid = (int?) message["uid"];
                        var accept = (bool?) message["accept"];
                        if (uid == null || accept == null)
                            break;
                        AcceptFriend((int) uid, (bool) accept);
                        break;
                    }
                case "end":
                    {
                        LazyAsync.Invoke(() => Stop());
                        break;
                    }
                case "displayname":
                    {
                        var s = (string) message["name"];
                        SetDisplayName(s);
                        break;
                    }
                case "status":
                    {
                        var u = (UserStatus) message["status"];
                        if (u != UserStatus.Offline && u != UserStatus.Unknown)
                        {
                            Me.Status = u;
                            LazyAsync.Invoke(() => Server.OnUserEvent(u, this));
                        }
                        break;
                    }
                case "hostgame":
                    {
                        var g = (Guid) message["game"];
                        var v = (Version) message["version"];
                        var n = (string) message["name"];
                        var pass = (string) message["pass"];
                        if (v != null && n != null && pass != null)
                            HostGame(g, v, n, pass);
                        break;
                    }
                case "gamestarted":
                    {
                        Gaming.StartGame((int) message["port"]);
                        LazyAsync.Invoke(() => Server.AllUserMessage(message.Clone() as SocketMessage));
                        break;
                    }
                case "customstatus":
                    {
                        var s = (string) message["customstatus"];
                        if (s != null)
                            SetCustomStatus(s);
                        break;
                    }
                case "joinchatroom":
                    {
                        LazyAsync.Invoke(() => Chatting.JoinChatRoom(this, message));
                        break;
                    }
                case "addusertochat":
                    {
                        LazyAsync.Invoke(() => Chatting.AddUserToChat(this, message));
                        break;
                    }
                case "twopersonchat":
                    {
                        LazyAsync.Invoke(() => Chatting.TwoPersonChat(this, message));
                        break;
                    }
                case "leavechat":
                    {
                        var rid = (long?) message["roomid"];
                        if (rid != null)
                        {
                            var rid2 = (long) rid;
                            LazyAsync.Invoke(() => Chatting.UserLeaves(Me.Clone() as User, rid2));
                        }
                        break;
                    }
                case "chatmessage":
                    {
                        LazyAsync.Invoke(() => Chatting.ChatMessage(this, message));
                        break;
                    }
            }
        }

        public void Stop()
        {
            lock (_clientLocker)
            {
                if (_stopping) return;
                _stopping = true;
#if(TestServer)
                    Trace.WriteLine(String.Format("Client[{0}]Client.Stop",Id));
#endif
                LoggedIn = false;
                _socket.WriteMessage(new SocketMessage("end"));
                _socket.Stop();
            }
        }

        private void SetCustomStatus(string status)
        {
            lock (_clientLocker)
            {
                if (status.Length > 200)
                    status = status.Substring(0, 197) + "...";
                if (!Cup.SetCustomStatus(Me.Uid, status)) return;
                Me.CustomStatus = status;
                LazyAsync.Invoke(() => Server.OnUserEvent(Me.Status, this, false));
            }
        }

        private void HostGame(Guid g, Version v, String name, String pass)
        {
            lock (_clientLocker)
            {
                var port = Gaming.HostGame(g, v, name, pass, Me);
                var som = new SocketMessage("hostgameresponse");
                som.AddData("port", port);
                _socket.WriteMessage(som);

                if (port == -1) return;
                var smm = new SocketMessage("GameHosting");
                smm.AddData("name", name);
                smm.AddData("passrequired", !String.IsNullOrEmpty(pass));
                smm.AddData("guid", g);
                smm.AddData("version", v);
                smm.AddData("hoster", Me);
                smm.AddData("port", port);
                LazyAsync.Invoke(() => Server.AllUserMessage(smm.Clone() as SocketMessage));
            }
        }

        public void WriteMessage(SocketMessage sm)
        {
            lock (_clientLocker)
            {
#if(TestServer)
                Trace.WriteLine("#WriteTo[" + Id + "](" + sm.Header + ")");
#endif
                _socket.WriteMessage(sm);
            }
        }

        public void OnUserEvent(UserStatus e, User theuser)
        {
            lock (_clientLocker)
            {
                //if (theuser.Equals(Me))
                //return;
                var sm = new SocketMessage("status");
                if (e == UserStatus.Invisible)
                    e = UserStatus.Offline;
                theuser.Status = e;
                sm.AddData("user", theuser);
                LazyAsync.Invoke(() => WriteMessage(sm));
            }
        }

        private void SetDisplayName(string name)
        {
            lock (_clientLocker)
            {
                if (name.Length > 60)
                    name = name.Substring(0, 57) + "...";
                else if (String.IsNullOrWhiteSpace(name))
                    name = Me.DisplayName;
                if (!Cup.SetDisplayName(Me.Uid, name)) return;
                Me.DisplayName = name;
                LazyAsync.Invoke(() => Server.OnUserEvent(Me.Status, this, false));
            }
        }

        private void AcceptFriend(int uid, bool accept)
        {
            lock (_clientLocker)
            {
                var requestee = Cup.GetUser(uid);
                if (requestee == null)
                    return;
                if (accept)
                {
                    //Add friend to this list
                    if (!_friends.Contains(requestee))
                        _friends.Add(requestee);
                    //Add you to friends list
                    var c = Server.GetOnlineClientByUid(requestee.Uid);
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
                LazyAsync.Invoke(() => SendFriendsList());
            }
        }

        public void AddFriend(User friend)
        {
            lock (_clientLocker)
            {
                if (_friends.Contains(friend)) return;
                _friends.Add(friend);
                if (LoggedIn)
                {
                    LazyAsync.Invoke(() => SendFriendsList());
                }
            }
        }

        private void RequestFriend(string email)
        {
            lock (_clientLocker)
            {
                if (email == null)
                    return;
                if (String.IsNullOrWhiteSpace(email))
                    return;
                if (!Verify.IsEmail(email))
                    return;
                email = email.ToLower();
                Cup.AddFriendRequest(Me.Uid, email);
                var smm = new SocketMessage("friendrequest");
                smm.AddData("user", Me);
                Server.WriteMessageToClient(smm, email);
            }
        }

        private void Login(string email, string token, UserStatus status)
        {
            lock (_clientLocker)
            {
                SocketMessage sm;
                if (email != null && token != null)
                {
                    //Authenticate Google Token
                    try
                    {
                        Trace.WriteLine(String.Format("Client[{0}]:Verifying Token", Id));
                        var appName = "skylabs-LobbyServer-" + _version;
                        var s = new Service("code", appName);
                        s.SetAuthenticationToken(token);
                        s.QueryClientLoginToken();
                        Trace.WriteLine(String.Format("Client[{0}]:Token Verified", Id));
                    }
                    catch (AuthenticationException e)
                    {
                        Debug.WriteLine(e);
                        sm = new SocketMessage("loginfailed");
                        sm.AddData("message", "Invalid Token");
                        _socket.WriteMessage(sm);
                        Trace.TraceError("Login attempt with invalid token.");
                        return;
                    }
                    catch (WebException e)
                    {
                        sm = new SocketMessage("loginfailed");
                        sm.AddData("message", "Server error");
                        _socket.WriteMessage(sm);
                        Trace.TraceError("Client.Login: ", e);
                        return;
                    }
                    Trace.WriteLine(String.Format("Client[{0}]:Getting db User", Id));
                    var u = Cup.GetUser(email);
                    var emailparts = email.Split('@');
                    if (u == null)
                    {
                        if (!Cup.RegisterUser(email, emailparts[0]))
                        {
                            Trace.WriteLine(String.Format("Client[{0}]:Registering User", Id));
                            LoggedIn = false;
                            sm = new SocketMessage("loginfailed");
                            sm.AddData("message", "Server error");
                            _socket.WriteMessage(sm);
                            return;
                        }
                    }
                    u = Cup.GetUser(email);

                    if (u == null)
                    {
                        LoggedIn = false;
                        sm = new SocketMessage("loginfailed");
                        sm.AddData("message", "Server error");
                        _socket.WriteMessage(sm);
                        Trace.WriteLine(String.Format("Client[{0}]:Login Failed", Id));
                        return;
                    }
                    var banned = Cup.IsBanned(u.Uid, _socket.RemoteEndPoint);
                    if (banned == -1)
                    {
                        Trace.WriteLine(String.Format("Client[{0}]:Starting to Stop and remove by uid={1}", Id, u.Uid));
                        var res = Server.StopAndRemoveAllByUID(this, u.Uid);
                        Trace.WriteLine(String.Format("Client[{0}]:Done Stop and remove by uid={1}", Id, u.Uid));
                        Me = u;
                        if (status == UserStatus.Unknown || status == UserStatus.Offline)
                            status = UserStatus.Online;
                        Me.Status = status;
                        sm = new SocketMessage("loginsuccess");
                        sm.AddData("me", Me);
                        _socket.WriteMessage(sm);
                        Trace.WriteLine(String.Format("Client[{0}]:Login Success", Id));
                        LoggedIn = true;
                        if (res.Item1 == 0)
                            LazyAsync.Invoke(() => Server.OnUserEvent(status, this));
                        _friends = Cup.GetFriendsList(Me.Uid);


                        LazyAsync.Invoke(() => SendFriendsList());
                        LazyAsync.Invoke(() => SendFriendRequests());
                        LazyAsync.Invoke(() => SendHostedGameList());
                        return;
                    }
                    sm = new SocketMessage("banned");
                    sm.AddData("end", banned);
                    _socket.WriteMessage(sm);
                    LazyAsync.Invoke(() => Stop());
                    LoggedIn = false;
                    return;
                }
                Trace.TraceError("Login attempted failed. Email or token null.");
                sm = new SocketMessage("loginfailed");
                sm.AddData("message", "Server error");
                _socket.WriteMessage(sm);
            }
        }

        private void SendHostedGameList()
        {
            lock (_clientLocker)
            {
                var sendgames = Gaming.GetLobbyList();
                var sm = new SocketMessage("gamelist");
                sm.AddData("list", sendgames);
                _socket.WriteMessage(sm);
            }
        }

        private void SendFriendRequests()
        {
            lock (_clientLocker)
            {
                var r = Cup.GetFriendRequests(Me.Email);
                if (r == null)
                    return;
                foreach (var e in r)
                {
                    var smm = new SocketMessage("friendrequest");
                    var u = Cup.GetUser(e);
                    smm.AddData("user", u);
                    _socket.WriteMessage(smm);
                }
            }
        }

        private void SendFriendsList()
        {
            lock (_clientLocker)
            {
                var sm = new SocketMessage("friends");
                foreach (var u in _friends)
                {
                    var c = Server.GetOnlineClientByUid(u.Uid);
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
                    sm.AddData(n.Uid.ToString(CultureInfo.InvariantCulture), n);
                }
                _socket.WriteMessage(sm);
            }
        }
    }
}