using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Skylabs.Lobby;
using Skylabs.Net;
using Skylabs.Net.Sockets;

namespace Skylabs.LobbyServer
{
    public class Client : SkySocket, IEquatable<Client>
    {
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
        public MySqlCup Cup { get; private set; }
        /// <summary>
        /// The user information on the currently connected user
        /// </summary>
        public User Me { get { return _me; } private set { _me = value; } }

        private Boolean _SupressSendOfflineStatus = false;
        /// <summary>
        /// A list of all the users friends.
        /// </summary>
        public List<User> Friends
        {
            get
            {
                return _friends;
            }
            set
            {
                _friends = value;
                foreach (User f in _friends)
                {
                    Client cl = Parent.Clients.FirstOrDefault(c => c.Me.Uid == f.Uid);
                    if (cl != null)
                        f.Status = cl.Me.Status;
                }
                if(LoggedIn) SendFriendsList();
            }
        }

        private Server Parent;

        private bool _sentEndMessage;

        private bool _gotEndMessage;

        private List<User> _friends;

        private User _me = new User();
        /// <summary>
        /// Create a connection.
        /// </summary>
        /// <param name="client">Tcp Client</param>
        /// <param name="id">ID provided by Server.cs</param>
        /// <param name="server">Server.cs instance</param>
        public Client(TcpClient client, int id, Server server)
            : base(client)
        {
            Id = id;
            Parent = server;
            LoggedIn = false;
            Cup = new MySqlCup(Program.Settings.dbUser, Program.Settings.dbPass, Program.Settings.dbHost, Program.Settings.db);
            _friends = new List<User>();
        }

        /// <summary>
        /// Compares one Client to another based on Client.ID
        /// </summary>
        /// <param name="client">Client to compare to</param>
        /// <returns>True if equal, false otherwise.</returns>
        public bool Equals(Client client)
        {
            return (Id == client.Id);
        }

        /// <summary>
        /// Disconnect cleanly
        /// </summary>
        public void Stop()
        {
            if(!_sentEndMessage)
            {
                WriteMessage(new SocketMessage("end"));
                _sentEndMessage = true;
            }
            if(_gotEndMessage)
            {
                Close(DisconnectReason.CleanDisconnect);
            }
        }
        /// <summary>
        /// Disconnect cleanly, with the option to suppress any message sending.
        /// Only useful if the connection is already closed, though it won't error out anyways.
        /// </summary>
        /// <param name="SuppressSendUserOfflineMessage"></param>
        public void Stop(bool SuppressSendUserOfflineMessage)
        {
            _SupressSendOfflineStatus = SuppressSendUserOfflineMessage;
            Stop();
        }
        /// <summary>
        /// This gets called when Server.cs wants to tell each client that a user event occured.
        /// </summary>
        /// <param name="e">User Status</param>
        /// <param name="theuser">The user who's status changed.</param>
        public void OnUserEvent(UserStatus e, User theuser)
        {
            //if(theuser.Equals(Me))
                //return;
            SocketMessage sm = new SocketMessage("status");
            if(e == UserStatus.Invisible)
                e = UserStatus.Offline;
            theuser.Status = e;
            sm.AddData("user", theuser);
            WriteMessage(sm);
        }
        /// <summary>
        /// Whenever a message is received, it goes here.
        /// </summary>
        /// <param name="sm">Message</param>
        public override void OnMessageReceived(SocketMessage sm)
        {
            switch (sm.Header.ToLower())
            {
                case "login":
                    Login(sm);
                    break;
                case "addfriend":
                    AddFriend(sm);
                    break;
                case "acceptfriend":
                    AcceptFriend(sm);
                    break;
                case "end":
                    _gotEndMessage = true;
                    Stop();
                    break;
                case "displayname":
                    {
                        SetDisplayName(sm);
                        break;
                    }
                case "status":
                    UserStatus u = (UserStatus)sm["status"];
                    if (u != UserStatus.Offline)
                    {
                        Me.Status = u;
                        Parent.OnUserEvent(u, this);
                    }
                    break;
                case "hostgame":
                    {
                        Guid g = (Guid)sm["game"];
                        Version v = (Version)sm["version"];
                        string n = (string)sm["name"];
                        string pass = (string)sm["password"];

                        HostedGame hs = new HostedGame(Parent, g, v, n, pass, Me);

                        SocketMessage som = new SocketMessage("hostgameresponse");
                        som.AddData("port", hs.Port);
                        WriteMessage(som);
                        if (hs.Port != -1 && hs.IsRunning)
                        {
                            Parent.Games.Add(hs);
                            SocketMessage smm = new SocketMessage("GameHosting");
                            smm.AddData("name", n);
                            smm.AddData("passrequired", !String.IsNullOrEmpty(pass));
                            smm.AddData("guid", g);
                            smm.AddData("version", v);
                            smm.AddData("hoster", Me);
                            smm.AddData("port", hs.Port);
                            Parent.AllUserMessage(smm);
                        }
                        break;
                    }
                case "gamestarted":
                    {
                        HostedGame g = Parent.Games.FirstOrDefault(hg => hg.Hoster == Me && hg.Port == (int)sm["port"]);
                        g.Status = Lobby.HostedGame.eHostedGame.GameInProgress;
                        Parent.AllUserMessage(sm);
                        break;
                    }
                case "customstatus":
                    SetCustomStatus(sm);
                    break;
                case "joinchatroom":
                    {
                        Chatting.JoinChatRoom(this, sm);
                        break;
                    }
                case "addusertochat":
                    {
                        Chatting.AddUserToChat(this, sm);
                        break;
                    }
                case "twopersonchat":
                    {
                        Chatting.TwoPersonChat(this, sm);
                        break;
                    }
                case "leavechat":
                    {
                        long? rid = (long?)sm["roomid"];
                        if (rid != null)
                        {
                            long rid2 = (long)rid;
                            Chatting.UserLeaves(Me, rid2);
                        }
                        break;
                    }
                case "chatmessage":
                    {
                        Chatting.ChatMessage(this,sm);
                        break;
                    }
            }
        }
        /// <summary>
        /// Don't call this, it gets called automatically on disconnect.
        /// </summary>
        /// <param name="reason">Reason for the disconnect</param>
        public override void OnDisconnect(DisconnectReason reason)
        {
            if(LoggedIn) Parent.OnUserEvent(UserStatus.Offline, this,_SupressSendOfflineStatus);
        }
        /// <summary>
        /// Accept a friend request. Handles the socket message data.
        /// </summary>
        /// <param name="sm">message</param>
        private void AcceptFriend(SocketMessage sm)
        {
            lock (Parent.Clients)
            {
                //incomming data
                //int uid      uid of the requestee
                //bool accept  should we accept it

                if (sm.Data.Length != 2)
                    return;
                int uid = (int)sm["uid"];
                bool accept = (bool)sm["accept"];
                User requestee = Cup.GetUser(uid);
                if (requestee == null)
                    return;
                if (accept)
                {
                    //Add friend to this list
                    Friends.Add(requestee);
                    //Add you to friends list
                    Client c = Parent.GetOnlineClientByUid(requestee.Uid);
                    if (c != null)
                    {
                        c.Friends.Add(Me);
                    }
                    //Add to database
                    Cup.AddFriend(Me.Uid, requestee.Uid);
                }
                //Remove any database friend requests
                Cup.RemoveFriendRequest(requestee.Uid, Me.Email);
                Cup.RemoveFriendRequest(Me.Uid, requestee.Email);
                this.SendFriendsList();
                Client rclient = Parent.GetOnlineClientByUid(requestee.Uid);
                if (rclient != null)
                    rclient.SendFriendsList();
            }
        }
        /// <summary>
        /// Add a friend. This happens after the friend request is accepted.
        /// </summary>
        /// <param name="sm">Message from client</param>
        private void AddFriend(SocketMessage sm)
        {
            lock (Parent.Clients)
            {
                if (sm.Data.Length <= 0)
                    return;
                string email = (string)sm.Data[0].Value;
                if (email == null)
                    return;
                if (String.IsNullOrWhiteSpace(email))
                    return;
                if (!Verify.IsEmail(email))
                    return;
                email = email.ToLower();
                Client c = Parent.GetOnlineClientByEmail(email);
                //If user exists and is online
                Cup.AddFriendRequest(Me.Uid, email);
                if (c != null)
                {
                    SocketMessage smm = new SocketMessage("friendrequest");
                    smm.AddData("user", Me);
                    c.WriteMessage(smm);
                }
            }
        }
        /// <summary>
        /// Send all the friend requests in the database for the current user, to the user.
        /// </summary>
        private void SendFriendRequests()
        {
            List<int> r = Cup.GetFriendRequests(Me.Email);
            if (r == null)
                return;
            foreach (int e in r)
            {
                SocketMessage smm = new SocketMessage("friendrequest");
                User u = Cup.GetUser(e);
                smm.AddData("user", u);
                WriteMessage(smm);
            }
        }
        /// <summary>
        /// Sends the user there friends list.
        /// </summary>
        private void SendFriendsList()
        {
            lock (Parent.Clients)
            {
                SocketMessage sm = new SocketMessage("friends");
                foreach (User u in Friends)
                {
                    Client c = Parent.GetOnlineClientByUid(u.Uid);
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
                    sm.AddData(new NameValuePair(n.Uid.ToString(), n));
                }
                WriteMessage(sm);
            }
        }
        /// <summary>
        /// Send the user a list of users online.
        /// This probably doesn't need to even exist, as the user doesn't benefit from knowing everyone online
        /// and could potentially slow things down. I don't even think that LobbyClient even cares about this list,
        /// but I can't be sure.
        /// </summary>
        private void SendUsersOnline()
        {
            lock (Parent.Clients)
            {
                SocketMessage sm = new SocketMessage("onlinelist");
                foreach (Client c in Parent.Clients)
                {
                    if (c.LoggedIn)
                    {
                        if (c.Me.Status != UserStatus.Unknown)
                        {
                            User n = (User)c.Me.Clone();
                            if (n.Status == UserStatus.Invisible)
                                n.Status = UserStatus.Offline;
                            sm.AddData(new NameValuePair(c.Me.Email, c.Me));
                        }
                    }
                }
                WriteMessage(sm);
            }
        }
        /// <summary>
        /// Set the users display name
        /// </summary>
        /// <param name="sm">Message containting display name data.</param>
        private void SetDisplayName(SocketMessage sm)
        {
            string s = (string)sm["name"];
            if (s != null)
            {
                if (s.Length > 60)
                    s = s.Substring(0, 57) + "...";
                else if (String.IsNullOrWhiteSpace(s))
                    s = Me.Email;
                if (Cup.SetDisplayName(Me.Uid, s))
                {
                    Me.DisplayName = s;
                    Parent.OnUserEvent(Me.Status, this, false);
                }
            }
        }
        /// <summary>
        /// Set users custom status
        /// </summary>
        /// <param name="sm">Message containing custom status</param>
        private void SetCustomStatus(SocketMessage sm)
        {
            string s = (string)sm["customstatus"];
            if(s != null)
            {
                if(s.Length > 200)
                    s = s.Substring(0, 197) + "...";
                if(Cup.SetCustomStatus(Me.Uid, s))
                {
                    Me.CustomStatus = s;
                    Parent.OnUserEvent(Me.Status, this, false);
                }
            }
        }
        /// <summary>
        /// Send the user a list of current hosted games.
        /// </summary>
        private void SendHostedGameList()
        {
            lock (Parent.Games)
            {
                List<Skylabs.Lobby.HostedGame> sendgames = new List<Lobby.HostedGame>();
                foreach (HostedGame hg in Parent.Games)
                {
                    sendgames.Add(new Lobby.HostedGame(hg.GameGuid, hg.GameVersion, hg.Port, hg.Name, !String.IsNullOrWhiteSpace(hg.Password), hg.Hoster));
                }
                SocketMessage sm = new SocketMessage("gamelist");
                sm.AddData("list", sendgames);
                WriteMessage(sm);
            }
        }
        /// <summary>
        /// Login function. When a user tries to login, the message gets sent here to get processed.
        /// It will send a message back if they failed or not.
        /// </summary>
        /// <param name="insm">message</param>
        private void Login(SocketMessage insm)
        {
            lock (Parent.Clients)
            {
                string email = (string)insm["email"];
                string token = (string)insm["token"];
                UserStatus stat = (UserStatus)insm["status"];
                SocketMessage sm;
                if (email != null && token != null)
                {
                    User u = Cup.GetUser(email);
                    if (u == null)
                    {
                        if (!Cup.RegisterUser(email, email))
                        {
                            LoggedIn = false;
                            sm = new SocketMessage("loginfailed");
                            sm.AddData("message", "Server error");
                            WriteMessage(sm);
                            return;
                        }
                    }
                    u = Cup.GetUser(email);
                    if (u == null)
                    {
                        LoggedIn = false;
                        sm = new SocketMessage("loginfailed");
                        sm.AddData("message", "Server error");
                        WriteMessage(sm);
                        return;
                    }
                    int banned = Cup.IsBanned(u.Uid, this);
                    if (banned == -1)
                    {
                        bool foundOne = false;
                        foreach (Client c in Parent.Clients)
                        {
                            if (c != null)
                            {
                                if (c.Me.Uid == u.Uid)
                                {
                                    if (c.LoggedIn)
                                        foundOne = true;
                                    c.Stop(true);
                                }
                            }
                        }
                        try
                        {
                            Parent.Clients.RemoveAll(c => c.Me.Uid == u.Uid);
                        }
                        catch (Exception)
                        {

                        }
                        Me = u;
                        Me.Status = stat;
                        sm = new SocketMessage("loginsuccess");
                        sm.AddData("me", Me);
                        WriteMessage(sm);
                        if (!foundOne)
                            Parent.OnUserEvent(stat, this);
                        Friends = Cup.GetFriendsList(Me.Uid);

                        LoggedIn = true;
                        SendFriendsList();
                        SendUsersOnline();
                        SendFriendRequests();
                        SendHostedGameList();
                        return;
                    }
                    else
                    {
                        sm = new SocketMessage("banned");
                        sm.AddData(new NameValuePair("end", banned));
                        WriteMessage(sm);
                        Stop();
                        LoggedIn = false;
                        return;
                    }
                }
                sm = new SocketMessage("loginfailed");
                sm.AddData("message", "Server error");
                WriteMessage(sm);
            }
        }
    }
}