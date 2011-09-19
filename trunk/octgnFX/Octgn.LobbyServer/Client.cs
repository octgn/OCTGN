using System;
using System.Net.Sockets;
using Octgn.LobbyServer;
using Skylabs.Lobby;
using Skylabs.Net;
using Skylabs.Net.Sockets;

namespace Skylabs.LobbyServer
{
    public class Client : SkySocket
    {
        public int ID { get; private set; }

        public bool LoggedIn { get; private set; }

        public MySqlCup Cup { get; private set; }

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
                case "end":
                    GotENDMessage = true;
                    Stop();
                    break;
            }
        }

        public override void OnDisconnect(DisconnectReason reason)
        {
        }

        private void Login(SocketMessage sm)
        {
            string email = sm["email"];
            string pass = sm["password"];
            if(email != null && pass != null)
            {
                pass = CreateSHAHash(pass);
                User u = Cup.GetUser(email);
                if(u != null)
                {
                    if(u.Password == pass)
                    {
                        int banned = Cup.IsBanned(u, this);
                        if(banned == -1)
                        {
                            LoggedIn = true;
                            WriteMessage(new SocketMessage("loginsuccess"));
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

        private static string CreateSHAHash(string Password)
        {
            using(System.Security.Cryptography.SHA512Managed HashTool = new System.Security.Cryptography.SHA512Managed())
            {
                Byte[] PasswordAsByte = System.Text.Encoding.ASCII.GetBytes(Password);
                Byte[] EncryptedBytes = HashTool.ComputeHash(PasswordAsByte);
                HashTool.Clear();
                return BitConverter.ToString(EncryptedBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }
}