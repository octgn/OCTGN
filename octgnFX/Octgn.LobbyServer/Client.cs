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

        public List<User> Friends { get; private set; }

        private Server Parent;

        private bool SentENDMessage = false;

        private bool GotENDMessage = false;

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
                case "ban":

                    break;
                case "end":
                    GotENDMessage = true;
                    Stop();
                    break;
                case "register":
                    Register(sm);
                    break;
            }
        }

        public override void OnDisconnect(DisconnectReason reason)
        {
            Parent.Client_Disconnect(this);
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
            if(Cup.GetUserByUsername(username) != null)
            {
                eusername = "Username taken.";
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
            if(Cup.RegisterUser(email, password, username))
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
            string pass = (string)sm["password"];
            if(email != null && pass != null)
            {
                pass = ValueConverters.CreateSHAHash(pass);
                User u = Cup.GetUser(email);
                if(u != null)
                {
                    if(u.Password == pass)
                    {
                        int banned = Cup.IsBanned(u.UID, this);
                        if(banned == -1)
                        {
                            LoggedIn = true;
                            Me = u;
                            WriteMessage(new SocketMessage("loginsuccess"));
                            Friends = Cup.GetFriendsList(Me.UID);
                            Parent.User_Login(this);
                            sendFriendsList();
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
                        LoggedIn = false;
                        WriteMessage(new SocketMessage("loginfailed"));
                        return;
                    }
                }
                else
                {
                    LoggedIn = false;
                    WriteMessage(new SocketMessage("loginfailed"));
                    return;
                }
            }
            WriteMessage(new SocketMessage("loginfailed"));
        }
    }
}