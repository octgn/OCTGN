//Copyright 2012 Skylabs
//In order to use this software, in any manor, you must first contact Skylabs.
//Website: http://www.skylabsonline.com
//Email:   skylabsonline@gmail.com
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
using Skylabs.Lobby.Sockets;
using Skylabs.Lobby.Threading;

namespace Skylabs.LobbyServer
{
    public static class Server
    {
        public static IPAddress LocalIp { get; private set; }

        public static int Port { get; private set; }

        private static TcpListener ListenSocket { get; set; }

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

        private static Conductor Conductor;

        /// <summary>
        /// Current assembly version of the server.
        /// </summary>
        public static Version Version = Assembly.GetExecutingAssembly().GetName().Version;

        /// <summary>
        /// Start the server
        /// </summary>
        static Server()
        {
            Clients = new List<Client>();
            Conductor = new Conductor();
        }
        /// <summary>
        /// Start listening for connections
        /// </summary>
        /// <param name="ip">Just IpAddress.Any should work</param>
        /// <param name="port">The port to host on</param>
        public static void Start(IPAddress ip, int port)
        {
            Logger.log("Start", "Start listening");
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
            Conductor.Dispose();
        }
        /// <summary>
        /// Get an online user by there e-mail address
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>Current online client, or Null if ones not found(I think)</returns>
        public static Client GetOnlineClientByEmail(string email)
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name,"ClientLocker");
                Client ret = Clients.FirstOrDefault(c => c.LoggedIn == true && c.Me.Email.ToLower().Equals(email.ToLower()));
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                return ret;
            }
            
        }
        public static void WriteMessageToClient(SocketMessage sm,string email)
        {
            lock (ClientLocker)
            {
                Client cl = Clients.FirstOrDefault(c => c.LoggedIn && c.Me.Email.ToLower() == email.ToLower());
                if (cl != null)
                    Conductor.Add(()=>cl.WriteMessage(sm));
            }
        }
        public static void WriteMessageToClient(SocketMessage sm,int uid)
        {
            lock (ClientLocker)
            {
                Client cl = Clients.FirstOrDefault(c => c.LoggedIn && c.Me.Uid == uid);
                if (cl != null)
                {
                    Conductor.Add(()=>cl.WriteMessage(sm));
                }
            }
        }
        /// <summary>
        /// Gets online user by there UID
        /// </summary>
        /// <param name="uid">user uid</param>
        /// <returns>Current online Client, or Null if ones not found.</returns>
        public static Client GetOnlineClientByUid(int uid)
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                Client ret = Clients.FirstOrDefault(c => c.LoggedIn == true && c.Me.Uid == uid);
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                return ret;
            }
            
        }
        public static int OnlineCount()
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                int ret = Clients.Count(c => c.LoggedIn == true);
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                return ret;
            }
        }
        public static UserStatus GetOnlineUserStatus(int uid)
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock(ClientLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                foreach(Client c in Clients)
                {
                    if (c.LoggedIn == true && c.Me.Uid == uid)
                    {
                        Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                        return c.Me.Status;
                    }
                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
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
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
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
                        Conductor.Add(()=>Chatting.UserOffline((User)me.Clone()));
                    }
                }
                if (!Supress)
                {
                    foreach (Client c in Clients)
                    { 
                        Conductor.Add(()=>c.OnUserEvent(e, me.Clone() as User));
                    }
                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            }
        }

        private static void AcceptClients()
        {
            try
            {
                ListenSocket.BeginAcceptTcpClient(AcceptReceiveDataCallback, ListenSocket);
            }
            catch (Exception)
            {

            }
        }
        /// <summary>
        /// Sends a socket message to all connected clients.
        /// </summary>
        /// <param name="sm">Message to send.</param>
        public static void AllUserMessage(SocketMessage sm)
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            List<Client> templist = new List<Client>();
            lock (ClientLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                foreach (Client c in Clients)
                {
                    if (c.LoggedIn)
                        templist.Add(c);
                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            }
            foreach(Client c in templist)
                Conductor.Add(()=>c.WriteMessage(sm));
        }
        /// <summary>
        /// Stops and removes all clients based on a uid.
        /// </summary>
        /// <param name="uid">UID</param>
        /// <returns>Tupple, where value1=number of users with UID who are logged in, and value2=Number of clients removed.</returns>
        public static Tuple<int,int> StopAndRemoveAllByUID(int uid)
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock(ClientLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                int loggedInCount = 0;
                int removedCount = 0;
                List<int> rlist = new List<int>();
                for (int i = 0; i < Clients.Count;i++ )
                {
                    if (Clients[i] == null) continue;
                    if (Clients[i].Me.Uid == uid)
                    {
                        rlist.Add(Clients[i].Id);
                        removedCount++;
                        if (Clients[i].LoggedIn)
                            loggedInCount++;
                        Client sClient = Clients[i];
                        Conductor.Add(() => sClient.Stop());
                        Logger.log(MethodInfo.GetCurrentMethod().Name, "Stoping client " + Clients[i].Id.ToString());
                    }
                }
                try
                {
                    foreach (int r in rlist)
                    {
                        Clients.RemoveAll(c => c.Id == r);
                    }
                }
                catch (ArgumentNullException)
                {

                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                return new Tuple<int, int>(loggedInCount,removedCount);
            }
        }
        private static void AcceptReceiveDataCallback(IAsyncResult ar)
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                // Get the socket that handles the client request.
                
                try
                {
                    TcpListener listener = (TcpListener)ar.AsyncState;
                    SkySocket ss = new SkySocket(listener.EndAcceptTcpClient(ar));
                    Clients.Add(new Client(ss, _nextId));
                    Logger.log(MethodInfo.GetCurrentMethod().Name, "Client " + _nextId.ToString() + " connected.");
                    _nextId++;
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("AcceptReceiveDataCallback: ObjectDisposedException");
                }
                AcceptClients();
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            }
        }
    }
}