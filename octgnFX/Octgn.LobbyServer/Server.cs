using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

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

        private void Accept_Clients()
        {
            ListenSocket.BeginAcceptTcpClient(AcceptReceiveDataCallback, ListenSocket);
        }

        private void AcceptReceiveDataCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;

            Clients.Add(new Client(listener.EndAcceptTcpClient(ar), NextID));
            NextID++;
            Accept_Clients();
        }
    }
}