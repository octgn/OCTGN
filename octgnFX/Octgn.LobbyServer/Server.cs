using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
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

        private static readonly DateTime ServerStartTime = DateTime.Now;

        public static TimeSpan ServerRunTime
        {
            get
            {
                return new TimeSpan(DateTime.Now.Ticks - ServerStartTime.Ticks);
            }
        }

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
            LockLogger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                LockLogger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name,"ClientLocker");
                Client ret =  Clients.Where(c => c.LoggedIn).FirstOrDefault(c => c.Me.Email.ToLower().Equals(email.ToLower()));
                LockLogger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                return ret;
            }
            
        }
        /// <summary>
        /// Gets online user by there UID
        /// </summary>
        /// <param name="uid">user uid</param>
        /// <returns>Current online Client, or Null if ones not found.</returns>
        public static Client GetOnlineClientByUid(int uid)
        {
            LockLogger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                LockLogger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                Client ret = Clients.Where(c => c.LoggedIn).FirstOrDefault(c => c.Me.Uid == uid);
                LockLogger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                return ret;
            }
            
        }
        public static int OnlineCount()
        {
            LockLogger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                LockLogger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                int ret = Clients.Count(c => c.LoggedIn == true);
                LockLogger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                return ret;
            }
        }
        public static UserStatus GetOnlineUserStatus(int uid)
        {
            LockLogger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock(ClientLocker)
            {
                LockLogger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                foreach(Client c in Clients)
                {
                    if (c.LoggedIn == true && c.Me.Uid == uid)
                    {
                        LockLogger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                        return c.Me.Status;
                    }
                }
                LockLogger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
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
            LockLogger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                LockLogger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                User me = (User) client.Me;
                if (e == UserStatus.Offline)
                {
                    Clients.Remove(client);
                    bool foundOne = false;
                    foreach (Client c in Clients)
                    {
                        if (c.Me.Uid == me.Uid)
                        {
                            foundOne = true;
                            break;
                        }
                    }
                    if (!foundOne)
                    {
                        Thread t = new Thread(() => { Chatting.UserOffline((User)me.Clone()); });
                        t.Start();
                    }
                }
                if (!Supress)
                {
                    foreach (Client c in Clients)
                        c.OnUserEvent(e, me);
                }
                LockLogger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
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
            LockLogger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            List<Client> templist = new List<Client>();
            lock (ClientLocker)
            {
                LockLogger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                foreach (Client c in Clients)
                {
                    if (c.LoggedIn)
                        templist.Add(c);
                }
                LockLogger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            }
            foreach(Client c in templist)
                c.WriteMessage(sm);
        }
        /// <summary>
        /// Stops and removes all clients based on a uid.
        /// </summary>
        /// <param name="uid">UID</param>
        /// <returns>Tupple, where value1=number of users with UID who are logged in, and value2=Number of clients removed.</returns>
        public static Tuple<int,int> StopAndRemoveAllByUID(int uid)
        {
            LockLogger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock(ClientLocker)
            {
                LockLogger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                int loggedInCount = 0;
                int removedCount = 0;
                List<Client> rlist = new List<Client>();
                foreach(Client c in Clients)
                {
                    if (c == null) continue;
                    if (c.Me.Uid == uid)
                    {
                        rlist.Add(c);
                        removedCount++;
                        if (c.LoggedIn)
                            loggedInCount++;
                        c.Stop(true);
                    }
                }
                try
                {
                    foreach (Client r in rlist)
                    {
                        Clients.Remove(r);
                    }
                }
                catch (ArgumentNullException)
                {

                }
                LockLogger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                return new Tuple<int, int>(loggedInCount,removedCount);
            }
        }
        private static void AcceptReceiveDataCallback(IAsyncResult ar)
        {
            LockLogger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                LockLogger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
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
                LockLogger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            }
        }
    }
}