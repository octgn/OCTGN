using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public class Server
    {
        public IPAddress LocalIP { get; private set; }

        public int Port { get; private set; }

        public TcpListener ListenSocket { get; private set; }

        public List<Client> Clients { get; set; }

        private int NextID;

        public Server(IPAddress ip, int port)
        {
            NextID = 0;
            LocalIP = ip;
            Port = port;
            ListenSocket = new TcpListener(LocalIP, Port);
            Clients = new List<Client>();
        }

        public void Start()
        {
            ListenSocket.Start();
            Accept_Clients();
        }

        public void Stop()
        {
            ListenSocket.Stop();
            foreach(Client c in Clients)
            {
                c.Stop();
            }
        }

        /// <summary>
        /// Happens when a client disconnects. This gets called automatically, don't call it.
        /// </summary>
        /// <param name="c">Client</param>
        public void Client_Disconnect(Client c)
        {
            if(c.LoggedIn)
            {
                SocketMessage sm = new SocketMessage("useroffline");
                sm.Add_Data(new NameValuePair("uid", c.Me.UID));
                foreach(Client cl in Clients)
                    if(cl.ID != c.ID)
                    {
                        if(cl.Connected)
                        {
                            cl.WriteMessage(sm);
                        }
                    }
            }
            Clients.Remove(c);
        }

        /// <summary>
        /// Happens when a user logs in. This gets called automatically, don't call it.
        /// </summary>
        /// <param name="c">Client</param>
        public void User_Login(Client c)
        {
            SocketMessage sm = new SocketMessage("useronline");
            sm.Add_Data(new NameValuePair(c.Me.Email, c.Me));
            foreach(Client cl in Clients)
                if(cl.Connected)
                {
                    cl.WriteMessage(sm);
                }
        }

        private void Accept_Clients()
        {
            ListenSocket.BeginAcceptTcpClient(AcceptReceiveDataCallback, ListenSocket);
        }

        private void AcceptReceiveDataCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;

            Clients.Add(new Client(listener.EndAcceptTcpClient(ar), NextID, this));
            NextID++;
            Accept_Clients();
        }
    }
}