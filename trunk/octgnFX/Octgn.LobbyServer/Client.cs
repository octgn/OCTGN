using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Octgn.LobbyServer;
using Skylabs.Lobby;
using Skylabs.Net;
using Skylabs.Net.Sockets;

namespace Skylabs.LobbyServer
{
    public class Client : SkySocket, IEquatable<Client>
    {
        public int ID { get; private set; }

        public bool LoggedIn { get; private set; }

        public MySqlCup Cup { get; private set; }

        public User Me { get; private set; }

        public List<User> Friends
        {
            get { return _Friends; }
            set { _Friends = value; if(LoggedIn)sendFriendsList(); }
        }

        private Server Parent;

        private bool SentENDMessage = false;

        private bool GotENDMessage = false;

        private List<User> _Friends;

        public Client(TcpClient client, int id, Server server)
            : base(client)
        {
            ID = id;
            Parent = server;
            LoggedIn = false;
            Cup = new MySqlCup(Program.Settings.dbUser, Program.Settings.dbPass, Program.Settings.dbHost, Program.Settings.db);
            _Friends = new List<User>();
        }

        /// <summary>
        /// Compares one Client to another based on Client.ID
        /// </summary>
        /// <param name="client">Client to compare to</param>
        /// <returns>True if equal, false otherwise.</returns>
        public bool Equals(Client client)
        {
            return (ID == client.ID);
        }

        /// <summary>
        /// Disconnect cleanly
        /// </summary>
        public void Stop()
        {
            if(!SentENDMessage)
            {
                WriteMessage(new SocketMessage("end"));
                SentENDMessage = true;
            }
            if(GotENDMessage)
            {
                Close(DisconnectReason.CleanDisconnect);
            }
        }

        public void OnUserEvent(UserEvent e, User theuser)
        {
            SocketMessage sm;
            if(e == UserEvent.Online)
                sm = new SocketMessage("useronline");
            else
                sm = new SocketMessage("useroffline");
            sm.Add_Data(new NameValuePair("user", theuser));
            WriteMessage(sm);
        }

        public override void OnMessageReceived(SocketMessage sm)
        {
            switch(sm.Header.ToLower())
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
                    GotENDMessage = true;
                    Stop();
                    break;
            }
        }

        public override void OnDisconnect(DisconnectReason reason)
        {
            if(LoggedIn) Parent.On_User_Event(UserEvent.Offline, this);
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
                Client c = Parent.GetOnlineClientByUID(requestee.UID);
                if(c != null)
                {
                    c.Friends.Add(Me);
                }
                //Add to database
                Cup.AddFriend(uid, requestee.UID);
            }
            //Remove any database friend requests
            Cup.RemoveFriendRequest(requestee.UID, Me.Email);
            Cup.RemoveFriendRequest(Me.UID, requestee.Email);
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
            if(c != null)
            {
                SocketMessage smm = new SocketMessage("friendrequest");
                smm.Add_Data("uid", Me.UID);
                c.WriteMessage(smm);
            }
            else
            {
                Cup.AddFriendRequest(Me.UID, email);
            }
        }

        private void sendFriendsList()
        {
            SocketMessage sm = new SocketMessage("friends");
            foreach(User u in Friends)
                sm.Add_Data(new NameValuePair(u.UID.ToString(), u.UID));
            WriteMessage(sm);
        }

        private void sendUsersOnline()
        {
            SocketMessage sm = new SocketMessage("onlinelist");
            foreach(Client c in Parent.Clients)
            {
                if(c.LoggedIn)
                {
                    sm.Add_Data(new NameValuePair(c.Me.Email, c.Me));
                }
            }
            WriteMessage(sm);
        }

        private void Login(SocketMessage insm)
        {
            string email = (string)insm["email"];
            string token = (string)insm["token"];
            SocketMessage sm;
            if(email != null && token != null)
            {
                User u = Cup.GetUser(email);
                if(u != null)
                {
                    int banned = Cup.IsBanned(u.UID, this);
                    if(banned == -1)
                    {
                        LoggedIn = true;
                        Me = u;
                        WriteMessage(new SocketMessage("loginsuccess"));
                        Friends = Cup.GetFriendsList(Me.UID);
                        Parent.On_User_Event(UserEvent.Online, this);

                        sendUsersOnline();
                        return;
                    }
                    else
                    {
                        sm = new SocketMessage("banned");
                        sm.Add_Data(new NameValuePair("end", banned));
                        WriteMessage(sm);
                        Stop();
                        LoggedIn = false;
                        return;
                    }
                }
                else
                {
                    if(Cup.RegisterUser(email, email))
                    {
                        Me = Cup.GetUser(email);
                        if(Me == null)
                        {
                            LoggedIn = false;
                            sm = new SocketMessage("loginfailed");
                            sm.Add_Data("message", "Server error");
                            WriteMessage(sm);
                        }
                        else
                        {
                            LoggedIn = true;
                            WriteMessage(new SocketMessage("loginsuccess"));
                            Friends = Cup.GetFriendsList(Me.UID);
                            Parent.On_User_Event(UserEvent.Online, this);
                            sendUsersOnline();
                        }
                    }
                    else
                    {
                        LoggedIn = false;
                        sm = new SocketMessage("loginfailed");
                        sm.Add_Data("message", "Server error");
                        WriteMessage(sm);
                    }
                    return;
                }
            }
            sm = new SocketMessage("loginfailed");
            sm.Add_Data("message", "Server error");
            WriteMessage(sm);
        }
    }
}