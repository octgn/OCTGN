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
            Friends = new List<User>();
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
            Parent.Client_Disconnect(this);
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

        private void Register(SocketMessage sm)
        {
            string email = (string)sm["email"];
            string password = (string)sm["password"];
            string username = (string)sm["username"];
            string eemail = "";
            string epassword = "";
            string eusername = "";
            bool problem = false;
            if(email == null || String.IsNullOrWhiteSpace(email))
            {
                eemail = "Email empty";
                problem = true;
            }
            else if(!Verify.IsEmail(email))
            {
                eemail = "Email is not valid";
                problem = true;
            }
            if(password == null || String.IsNullOrWhiteSpace(password))
            {
                epassword = "Password empty";
                problem = true;
            }
            if(username == null || String.IsNullOrWhiteSpace(username))
            {
                eusername = "Username empty";
                problem = true;
            }
            if(problem)
            {
                SocketMessage smm = new SocketMessage("registerfailed");
                smm.Add_Data(new NameValuePair("email", eemail));
                smm.Add_Data(new NameValuePair("password", epassword));
                smm.Add_Data(new NameValuePair("username", eusername));
                WriteMessage(smm);
                return;
            }
            if(Cup.GetUser(email) != null)
            {
                eemail = "Email taken.";
                problem = true;
            }
            if(problem)
            {
                SocketMessage smm = new SocketMessage("registerfailed");
                smm.Add_Data(new NameValuePair("email", eemail));
                smm.Add_Data(new NameValuePair("password", epassword));
                smm.Add_Data(new NameValuePair("username", eusername));
                WriteMessage(smm);
                return;
            }
            //If we wind up here, everything has checked out.
            if(Cup.RegisterUser(email, username))
            {
                SocketMessage smm = new SocketMessage("registersuccess");
                WriteMessage(smm);
            }
            else
            {
                SocketMessage smm = new SocketMessage("registerfailed");
                smm.Add_Data(new NameValuePair("email", eemail));
                smm.Add_Data(new NameValuePair("password", epassword));
                smm.Add_Data(new NameValuePair("username", eusername));
                WriteMessage(smm);
            }
        }

        private void Login(SocketMessage sm)
        {
            string email = (string)sm["email"];
            string token = (string)sm["token"];

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
                        Parent.User_Login(this);
                        sendUsersOnline();
                        return;
                    }
                    else
                    {
                        SocketMessage smm = new SocketMessage("banned");
                        smm.Add_Data(new NameValuePair("end", banned));
                        WriteMessage(smm);
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
                            WriteMessage(new SocketMessage("loginfailed"));
                        }
                        else
                        {
                            LoggedIn = true;
                            WriteMessage(new SocketMessage("loginsuccess"));
                            Friends = Cup.GetFriendsList(Me.UID);
                            Parent.User_Login(this);
                            sendUsersOnline();
                        }
                    }
                    else
                    {
                        LoggedIn = false;
                        WriteMessage(new SocketMessage("loginfailed"));
                    }
                    return;
                }
            }
            WriteMessage(new SocketMessage("loginfailed"));
        }
    }
}