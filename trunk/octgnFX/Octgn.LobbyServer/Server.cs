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
        /// <summary>
        /// Probubly doesn't need to exist. Should just be able to call the main Debug Tracer or something
        /// </summary>
        public TraceSource DebugTrace = new TraceSource("DebugTrace", SourceLevels.All);

        public IPAddress LocalIp { get; private set; }

        public int Port { get; private set; }

        public TcpListener ListenSocket { get; private set; }

        public List<Client> Clients 
        { 
            get
            {
                lock (_clients)
                    return _clients;
            } 
            set
            {
                lock (_clients)
                    _clients = value;
            } 
        }
        /// <summary>
        /// A list of all the current HostedGames. It should only contain those that are actually running.
        /// </summary>
        public List<HostedGame> Games { get; set; } 

        private int _nextId;

        private List<Client> _clients = new List<Client>();

        /// <summary>
        /// Current assembly version of the server.
        /// </summary>
        public Version Version
        {
            get
            {
                Assembly asm = Assembly.GetCallingAssembly();
                return asm.GetName().Version;
            }
        }

        /// <summary>
        /// This will get the next available port that can be hosted on.
        /// It checks all hosted games and also what ports are being used by the system.
        /// </summary>
        public int NextHostPort
        {
            get
            {
                lock(Games)
                {
                    while(Games.Exists(hg => hg.Port == _currentHostPort))
                    {
                        _currentHostPort++;
                        if(_currentHostPort >= 8000)
                            _currentHostPort = 5000;
                    }
                    while(!Networking.IsPortAvailable(_currentHostPort))
                    {
                        _currentHostPort++;
                        if (_currentHostPort >= 8000)
                            _currentHostPort = 5000;
                    }
                    return _currentHostPort;
                }
            }
        }
        private int _currentHostPort = 5000;
        /// <summary>
        /// Start the server
        /// </summary>
        /// <param name="ip">Just IpAddress.Any should work</param>
        /// <param name="port">The port to host on</param>
        public Server(IPAddress ip, int port)
        {
            _nextId = 0;
            LocalIp = ip;
            Port = port;
            ListenSocket = new TcpListener(LocalIp, Port);
            Games = new List<HostedGame>();
        }
        /// <summary>
        /// Start listening for connections
        /// </summary>
        public void Start()
        {
            ListenSocket.Start();
            AcceptClients();
        }
        /// <summary>
        /// Stop listening for connections and shut down ones that exist
        /// </summary>
        public void Stop()
        {
            ListenSocket.Stop();
            foreach(Client c in Clients)
            {
                c.Stop();
            }
        }
        /// <summary>
        /// Get an online user by there e-mail address
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>Current online client, or Null if ones not found(I think)</returns>
        public Client GetOnlineClientByEmail(string email)
        {
            return Clients.Where(c => c.LoggedIn).FirstOrDefault(c => c.Me.Email.ToLower().Equals(email.ToLower()));
        }
        /// <summary>
        /// Gets online user by there UID
        /// </summary>
        /// <param name="uid">user uid</param>
        /// <returns>Current online Client, or Null if ones not found.</returns>
        public Client GetOnlineClientByUid(int uid)
        {
            return Clients.Where(c => c.LoggedIn).FirstOrDefault(c => c.Me.Uid == uid);
        }
        /// <summary>
        /// If a user event happens, call this and it will broadcast it and update the necisary users.
        /// </summary>
        /// <param name="e">User status</param>
        /// <param name="client">The client that called</param>
        public void OnUserEvent(UserStatus e, Client client)
        {
            OnUserEvent(e, client, false);
        }
        /// <summary>
        /// If a user event happens, call this and it will broadcast it and update the necisary users.
        /// This gives you the option to supress a broadcast message.
        /// </summary>
        /// <param name="e">User status</param>
        /// <param name="client">The client that called</param>
        /// <param name="Supress">Should we supress a broadcast message</param>
        public void OnUserEvent(UserStatus e, Client client, bool Supress)
        {
            User me = (User)client.Me;
            if (e == UserStatus.Offline)
            {
                Clients.Remove(client);
                Chatting.UserOffline(me);
            }
            if (!Supress)
            {
                foreach (Client c in Clients)
                    c.OnUserEvent(e, me);
            }
        }

        private void AcceptClients()
        {
            ListenSocket.BeginAcceptTcpClient(AcceptReceiveDataCallback, ListenSocket);
        }
        /// <summary>
        /// Sends a socket message to all connected clients.
        /// </summary>
        /// <param name="sm">Message to send.</param>
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
            lock (Clients)
            {
                // Get the socket that handles the client request.
                TcpListener listener = (TcpListener)ar.AsyncState;
                try
                {
                    Clients.Add(new Client(listener.EndAcceptTcpClient(ar), _nextId, this));
                    _nextId++;
                    AcceptClients();
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("AcceptReceiveDataCallback: ObjectDisposedException");
                }
            }
        }
    }
}