using System;
using System.Collections.Concurrent;
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
    public static class Server
    {
        public static IPAddress LocalIp { get; private set; }

        public static int Port { get; private set; }

        public static TcpListener ListenSocket { get; private set; }

        private static List<Client> Clients { get; set; }

        private static int _nextId = 0;

        private static readonly object ClientLocker = new object();

        /// <summary>
        /// Current assembly version of the server.
        /// </summary>
        public static Version Version
        {
            get
            {
                Assembly asm = Assembly.GetCallingAssembly();
                return asm.GetName().Version;
            }
        }

        /// <summary>
        /// Start the server
        /// </summary>
        static Server()
        { 
            Clients = new List<Client>();
        }
        /// <summary>
        /// Start listening for connections
        /// </summary>
        /// <param name="ip">Just IpAddress.Any should work</param>
        /// <param name="port">The port to host on</param>
        public static void Start(IPAddress ip, int port)
        {
            LocalIp = ip;
            Port = port;
            ListenSocket = new TcpListener(LocalIp, Port);
            ListenSocket.Start();
            AcceptClients();
        }
        /// <summary>
        /// Stop listening for connections and shut down ones that exist
        /// </summary>
        public static void Stop()
        {
            ListenSocket.Stop();
            lock (ClientLocker)
            {
                foreach (Client c in Clients)
                {
                    c.Stop();
                }
            }
        }
        /// <summary>
        /// Get an online user by there e-mail address
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>Current online client, or Null if ones not found(I think)</returns>
        public static Client GetOnlineClientByEmail(string email)
        {
            lock(ClientLocker)
                return Clients.Where(c => c.LoggedIn).FirstOrDefault(c => c.Me.Email.ToLower().Equals(email.ToLower()));
        }
        /// <summary>
        /// Gets online user by there UID
        /// </summary>
        /// <param name="uid">user uid</param>
        /// <returns>Current online Client, or Null if ones not found.</returns>
        public static Client GetOnlineClientByUid(int uid)
        {
            lock (ClientLocker)
                return Clients.Where(c => c.LoggedIn).FirstOrDefault(c => c.Me.Uid == uid);
        }
        public static UserStatus GetOnlineUserStatus(int uid)
        {
            lock(ClientLocker)
            {
                foreach(Client c in Clients)
                {
                    if (c.Me.Uid == uid)
                        return c.Me.Status;
                }
                return UserStatus.Offline;
            }
        }
        /// <summary>
        /// If a user event happens, call this and it will broadcast it and update the necisary users.
        /// </summary>
        /// <param name="e">User status</param>
        /// <param name="client">The client that called</param>
        public static void OnUserEvent(UserStatus e, Client client)
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
        public static void OnUserEvent(UserStatus e, Client client, bool Supress)
        {
            lock (ClientLocker)
            {
                User me = (User) client.Me;
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
        }

        private static void AcceptClients()
        {
            ListenSocket.BeginAcceptTcpClient(AcceptReceiveDataCallback, ListenSocket);
        }
        /// <summary>
        /// Sends a socket message to all connected clients.
        /// </summary>
        /// <param name="sm">Message to send.</param>
        public static void AllUserMessage(SocketMessage sm)
        {
            lock (ClientLocker)
            {
                foreach (Client c in Clients)
                {
                    if (c.LoggedIn)
                        c.WriteMessage(sm);
                }
            }
        }
        /// <summary>
        /// Stops and removes all clients based on a uid.
        /// </summary>
        /// <param name="uid">UID</param>
        /// <returns>Tupple, where value1=number of users with UID who are logged in, and value2=Number of clients removed.</returns>
        public static Tuple<int,int> StopAndRemoveAllByUID(int uid)
        {
            lock(ClientLocker)
            {
                int loggedInCount = 0;
                int removedCount = 0;
                foreach(Client c in Clients)
                {
                    if (c == null) continue;
                    if (c.Me.Uid == uid)
                    {
                        if (c.LoggedIn)
                            loggedInCount++;
                        c.Stop(true);
                    }
                }
                try
                {
                    removedCount = Server.Clients.RemoveAll(c => c.Me.Uid == uid);
                }
                catch (ArgumentNullException)
                {

                }
                return new Tuple<int, int>(loggedInCount,removedCount);
            }
        }
        private static void AcceptReceiveDataCallback(IAsyncResult ar)
        {
            lock (ClientLocker)
            {
                // Get the socket that handles the client request.
                TcpListener listener = (TcpListener) ar.AsyncState;
                try
                {
                    Clients.Add(new Client(listener.EndAcceptTcpClient(ar), _nextId));
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