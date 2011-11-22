using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using Octgn.LobbyServer;
using Skylabs.Lobby;
using Skylabs.Net;
using Skylabs.Net.Sockets;

namespace Skylabs.LobbyServer
{
    public class Client : SkySocket, IEquatable<Client>
    {
        public int Id { get; private set; }

        public bool LoggedIn { get; private set; }

        public MySqlCup Cup { get; private set; }

        public User Me { get; private set; }

        public List<User> Friends
        {
            get
            {
                return _friends;
            }
            set
            {
                _friends = value;
                if(LoggedIn) SendFriendsList();
            }
        }

        private Server Parent;

        private bool _sentEndMessage;

        private bool _gotEndMessage;

        private List<User> _friends;

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

        public void OnUserEvent(UserStatus e, User theuser)
        {
            if(theuser.Equals(Me))
                return;
            SocketMessage sm = new SocketMessage("userstatus");
            if(e == UserStatus.Invisible)
                e = UserStatus.Offline;
            theuser.Status = e;
            sm.AddData(new NameValuePair("user", theuser));
            WriteMessage(sm);
        }

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
                case "status":
                    UserStatus u = (UserStatus)sm["status"];
                    if (u != null)
                    {
                        if (u != UserStatus.Offline)
                        {
                            Me.Status = u;
                            Parent.OnUserEvent(u, this);
                        }
                    }
                    break;
                case "hostgame":
                    {
                        Guid g = (Guid) sm["game"];
                        Version v = (Version) sm["version"];
                        string n = (string) sm["name"];
                        string pass = (string) sm["password"];

                        HostedGame hs = new HostedGame(Parent,g,v,n,pass);

                        SocketMessage som = new SocketMessage("hostgameresponse");
                        som.AddData("port", hs.Port);
                        WriteMessage(som);
                        if (hs.Port != -1 && hs.IsRunning)
                        {
                            Parent.Games.Add(hs);
                            SocketMessage smm = new SocketMessage("GameHosting");
                            smm.AddData("name",n);
                            smm.AddData("passrequired",!String.IsNullOrEmpty(pass));
                            smm.AddData("guid",g);
                            smm.AddData("version",v);
                            smm.AddData("hoster",Me);
                            smm.AddData("port",hs.Port);
                            Parent.AllUserMessage(smm);
                        }
                        break;
                    }
                case "customstatus":
                    SetCustomStatus(sm);
                    break;
            }
        }

        public override void OnDisconnect(DisconnectReason reason)
        {
            if(LoggedIn) Parent.OnUserEvent(UserStatus.Offline, this);
        }

        private void AcceptFriend(SocketMessage sm)
        {
            //incomming data
            //int uid      uid of the requestee
            //bool accept  should we accept it

            if(sm.Data.Length != 2)
                return;
            int uid = (int)sm["uid"];
            bool accept = (bool)sm["accept"];
            User requestee = Cup.GetUser(uid);
            if(requestee == null)
                return;
            if(accept)
            {
                //Add friend to this list
                Friends.Add(requestee);
                //Add you to friends list
                Client c = Parent.GetOnlineClientByUid(requestee.Uid);
                if(c != null)
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
        }

        private void AddFriend(SocketMessage sm)
        {
            if(sm.Data.Length <= 0)
                return;
            string email = (string)sm.Data[0].Value;
            if(email == null)
                return;
            if(String.IsNullOrWhiteSpace(email))
                return;
            if(!Verify.IsEmail(email))
                return;
            email = email.ToLower();
            Client c = Parent.GetOnlineClientByEmail(email);
            //If user exists and is online
            Cup.AddFriendRequest(Me.Uid, email);
            if(c != null)
            {
                SocketMessage smm = new SocketMessage("friendrequest");
                smm.AddData("user", Me);
                c.WriteMessage(smm);
            }
        }

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

        private void SendFriendsList()
        {
            SocketMessage sm = new SocketMessage("friends");
            foreach(User u in Friends)
            {
                Client c = Parent.GetOnlineClientByUid(u.Uid);
                User n;
                if(c == null)
                {
                    n = u;
                    n.Status = UserStatus.Offline;
                }
                else
                {
                    n = c.Me;
                    if(n.Status == UserStatus.Invisible)
                        n.Status = UserStatus.Offline;
                }
                sm.AddData(new NameValuePair(n.Uid.ToString(), n));
            }
            WriteMessage(sm);
        }

        private void SendUsersOnline()
        {
            SocketMessage sm = new SocketMessage("onlinelist");
            foreach(Client c in Parent.Clients)
            {
                if(c.LoggedIn)
                {
                    if(c.Me.Status != UserStatus.Unknown)
                    {
                        User n = (User)c.Me.Clone();
                        if(n.Status == UserStatus.Invisible)
                            n.Status = UserStatus.Offline;
                        sm.AddData(new NameValuePair(c.Me.Email, c.Me));
                    }
                }
            }
            WriteMessage(sm);
        }

        private void SetCustomStatus(SocketMessage sm)
        {
            string s = (string)sm["customstatus"];
            if(s != null)
            {
                if(s.Length > 200)
                    s = s.Substring(0, 197) + "...";
                if(Cup.SetCustomStatus(Me.Uid, s))
                {
                    sm.Data = new NameValuePair[] { };
                    sm.AddData("user", Me);
                    sm.AddData("status", s);
                    Parent.AllUserMessage(sm);
                }
            }
        }

        private void Login(SocketMessage insm)
        {
            string email = (string)insm["email"];
            string token = (string)insm["token"];
            UserStatus stat = (UserStatus)insm["status"];
            SocketMessage sm;
            if(email != null && token != null)
            {
                User u = Cup.GetUser(email);
                if(u != null)
                {
                    int banned = Cup.IsBanned(u.Uid, this);
                    if(banned == -1)
                    {
                        LoggedIn = true;
                        u.Status = stat;
                        Me = u;
                        sm = new SocketMessage("loginsuccess");
                        sm.AddData("me", Me);
                        WriteMessage(sm);
                        Friends = Cup.GetFriendsList(Me.Uid);
                        Parent.OnUserEvent(stat, this);

                        SendUsersOnline();
                        SendFriendRequests();
                        return;
                    }
                    sm = new SocketMessage("banned");
                    sm.AddData(new NameValuePair("end", banned));
                    WriteMessage(sm);
                    Stop();
                    LoggedIn = false;
                    return;
                }
                if(Cup.RegisterUser(email, email))
                {
                    Me = Cup.GetUser(email);
                    if(Me == null)
                    {
                        LoggedIn = false;
                        sm = new SocketMessage("loginfailed");
                        sm.AddData("message", "Server error");
                        WriteMessage(sm);
                        return;
                    }
                    LoggedIn = true;
                    u.Status = stat;
                    Me = u;
                    sm = new SocketMessage("loginsuccess");
                    sm.AddData("me", Me);
                    WriteMessage(sm);
                    Friends = Cup.GetFriendsList(Me.Uid);
                    Parent.OnUserEvent(stat, this);

                    SendUsersOnline();
                    return;
                }
                LoggedIn = false;
                sm = new SocketMessage("loginfailed");
                sm.AddData("message", "Server error");
                WriteMessage(sm);
                return;
            }
            sm = new SocketMessage("loginfailed");
            sm.AddData("message", "Server error");
            WriteMessage(sm);
        }
    }
}