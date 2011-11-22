using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Skylabs.Lobby;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public class Server
    {
        public TraceSource DebugTrace = new TraceSource("DebugTrace", SourceLevels.All);

        public IPAddress LocalIp { get; private set; }

        public int Port { get; private set; }

        public TcpListener ListenSocket { get; private set; }

        public List<Client> Clients { get; set; }

        public List<HostedGame> Games { get; set; } 

        private int _nextId;

        public Version Version
        {
            get
            {
                Assembly asm = Assembly.GetCallingAssembly();
                return asm.GetName().Version;
            }
        }

        public Server(IPAddress ip, int port)
        {
            _nextId = 0;
            LocalIp = ip;
            Port = port;
            ListenSocket = new TcpListener(LocalIp, Port);
            Clients = new List<Client>();
            Games = new List<HostedGame>();
        }

        public void Start()
        {
            ListenSocket.Start();
            AcceptClients();
        }

        public void Stop()
        {
            ListenSocket.Stop();
            foreach(Client c in Clients)
            {
                c.Stop();
            }
        }

        public Client GetOnlineClientByEmail(string email)
        {
            return Clients.Where(c => c.LoggedIn).FirstOrDefault(c => c.Me.Email.ToLower().Equals(email.ToLower()));
        }

        public Client GetOnlineClientByUid(int uid)
        {
            return Clients.Where(c => c.LoggedIn).FirstOrDefault(c => c.Me.Uid == uid);
        }

        public void OnUserEvent(UserStatus e, Client client)
        {
            User me = (User)client.Me.Clone();
            if(e == UserStatus.Offline)
                Clients.Remove(client);

            foreach(Client c in Clients)
                c.OnUserEvent(e, me);
        }

        private void AcceptClients()
        {
            ListenSocket.BeginAcceptTcpClient(AcceptReceiveDataCallback, ListenSocket);
        }

        public void AllUserMessage(SocketMessage sm)
        {
            foreach(Client c in Clients)
            {
                if(c.LoggedIn)
                    c.WriteMessage(sm);
            }
        }

        private void AcceptReceiveDataCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.
            TcpListener listener = (TcpListener)ar.AsyncState;
            try
            {
                Clients.Add(new Client(listener.EndAcceptTcpClient(ar), _nextId, this));
                _nextId++;
                AcceptClients();
            }
            catch(ObjectDisposedException de)
            {
                //Todo Probubly should have some form of logging for this error.
            }
        }
    }
}