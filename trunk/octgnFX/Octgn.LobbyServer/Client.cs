using System;
using System.Net.Sockets;
using MySql.Data.MySqlClient;
using Skylabs.Net;
using Skylabs.Net.Sockets;

namespace Skylabs.LobbyServer
{
    public class Client : SkySocket
    {
        public int ID { get; private set; }

        public bool LoggedIn { get; private set; }

        private Server Parent;

        public Client(TcpClient client, int id, Server server)
            : base(client)
        {
            ID = id;
            Parent = server;
            LoggedIn = false;
        }

        public void Stop()
        {
            SocketMessage sm = new SocketMessage("END");
            WriteMessage(sm);
            Close(DisconnectReason.CleanDisconnect);
        }

        public override void OnMessageReceived(SocketMessage sm)
        {
            if(sm.Header == "login")
            {
                Login(sm);
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
                int len = pass.Length;
                try
                {
                    using(MySqlConnection con = new MySqlConnection(Program.MySqlConnectionString))
                    {
                        con.Open();
                        using(MySqlDataAdapter da = new MySqlDataAdapter("SELECT * FROM users WHERE email='" + email + "' AND password='" + pass + "';", con))
                        {
                            using(MySqlDataReader dr = da.SelectCommand.ExecuteReader())
                            {
                                if(dr.HasRows)
                                {
                                    LoggedIn = true;
                                    WriteMessage(new SocketMessage("loginsuccess"));
                                    con.Close();
                                    return;
                                }
                                else
                                {
                                    LoggedIn = false;
                                    WriteMessage(new SocketMessage("loginfailed"));
                                    con.Close();
                                    return;
                                }
                            }
                        }
                    }
                }
                catch(MySqlException ex)
                {
#if(DEBUG)
                    if(System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();
#endif
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