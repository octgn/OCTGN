using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Skylabs.Lobby;
using Skylabs.Lobby.Sockets;
using Skylabs.Lobby.Threading;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public static class Server
    {
        private static int _nextId;

        private static readonly object ClientLocker = new object();

        private static readonly DateTime ServerStartTime = DateTime.Now;

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
        }

        public static IPAddress LocalIp { get; private set; }

        public static int Port { get; private set; }

        private static TcpListener ListenSocket { get; set; }

        private static List<Client> Clients { get; set; }

        public static TimeSpan ServerRunTime
        {
            get { return new TimeSpan(DateTime.Now.Ticks - ServerStartTime.Ticks); }
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
        }

        /// <summary>
        /// Get an online user by there e-mail address
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns>Current online client, or Null if ones not found(I think)</returns>
        public static Client GetOnlineClientByEmail(string email)
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "ClientLocker");
                Client ret = Clients.FirstOrDefault(c => c.LoggedIn && c.Me.Email.ToLower().Equals(email.ToLower()));
                Logger.UL(MethodBase.GetCurrentMethod().Name, "ClientLocker");
                return ret;
            }
        }

        public static void WriteMessageToClient(SocketMessage sm, string email)
        {
            lock (ClientLocker)
            {
                Client cl = Clients.FirstOrDefault(c => c.LoggedIn && c.Me.Email.ToLower() == email.ToLower());
                if (cl != null)
                    LazyAsync.Invoke(() => cl.WriteMessage(sm));
            }
        }

        public static void WriteMessageToClient(SocketMessage sm, int uid)
        {
            lock (ClientLocker)
            {
                Client cl = Clients.FirstOrDefault(c => c.LoggedIn && c.Me.Uid == uid);
                if (cl != null)
                {
                    LazyAsync.Invoke(() => cl.WriteMessage(sm));
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
            Logger.TL(MethodBase.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "ClientLocker");
                Client ret = Clients.FirstOrDefault(c => c.LoggedIn && c.Me.Uid == uid);
                Logger.UL(MethodBase.GetCurrentMethod().Name, "ClientLocker");
                return ret;
            }
        }

        public static int OnlineCount()
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "ClientLocker");
                int ret = Clients.Count(c => c.LoggedIn);
                Logger.UL(MethodBase.GetCurrentMethod().Name, "ClientLocker");
                return ret;
            }
        }

        public static UserStatus GetOnlineUserStatus(int uid)
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "ClientLocker");
                foreach (Client c in Clients)
                {
                    if (c.LoggedIn && c.Me.Uid == uid)
                    {
                        Logger.UL(MethodBase.GetCurrentMethod().Name, "ClientLocker");
                        return c.Me.Status;
                    }
                }
                Logger.UL(MethodBase.GetCurrentMethod().Name, "ClientLocker");
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
            Logger.TL(MethodBase.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "ClientLocker");
                User me = client.Me;
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
                        LazyAsync.Invoke(() => Chatting.UserOffline((User) me.Clone()));
                    }
                }
                if (!Supress)
                {
                    foreach (Client c in Clients)
                    {
                        Client cl2 = c;
                        LazyAsync.Invoke(() => cl2.OnUserEvent(e, me.Clone() as User));
                    }
                }
                Logger.UL(MethodBase.GetCurrentMethod().Name, "ClientLocker");
            }
        }

        private static void AcceptClients()
        {
            try
            {
                ListenSocket.BeginAcceptTcpClient(AcceptReceiveDataCallback, ListenSocket);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Sends a socket message to all connected clients.
        /// </summary>
        /// <param name="sm">Message to send.</param>
        public static void AllUserMessage(SocketMessage sm)
        {
            lock (ClientLocker)
            {
#if(TestServer)
                    Trace.WriteLine("#WriteAll: " + sm.Header);
#endif
                foreach (Client c in Clients)
                {
                    Client cl2 = c;
                    if (cl2.LoggedIn)
                    {
#if(TestServer)
                            Trace.WriteLine("#TryWriteTo[" + cl2.Id + "](" + sm.Header + ")");
#endif
                        LazyAsync.Invoke(() => cl2.WriteMessage(sm));
                    }
                }
            }
        }

        public static void AllUserMessageUidList(int[] uids, SocketMessage sm)
        {
            lock (ClientLocker)
            {
                foreach (Client c in Clients)
                {
                    Client cl2 = c;
                    if (cl2.LoggedIn && uids.Contains(cl2.Me.Uid))
                    {
                        LazyAsync.Invoke(() => cl2.WriteMessage(sm));
                    }
                }
            }
        }

        // unused
        /*
        private static void StopClient(int clientID)
        {
            lock (ClientLocker)
            {
                Trace.WriteLine(String.Format("#Stopping client[{0}]", clientID));
                Client c = Clients.FirstOrDefault(cl => cl.Id == clientID);
                if (c != null)
                {
                    Trace.WriteLine("Stopping client[" + c.Id.ToString() + "]");
                    LazyAsync.Invoke(() => c.Stop());
                    Clients.Remove(c);
                }

            }
        }
        */

        /// <summary>
        /// Stops and removes all clients based on a uid.
        /// </summary>
        /// <param name="caller">The Caller</param>
        /// <param name="uid">UID</param>
        /// <returns>Tupple, where value1=number of users with UID who are logged in, and value2=Number of clients removed.</returns>
        public static Tuple<int, int> StopAndRemoveAllByUID(Client caller, int uid)
        {
            //Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                //Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                int loggedInCount = 0;
                int removedCount = 0;
                // int StartCount = Clients.Count; // unused
                foreach (Client cl in Clients)
                {
                    Client cl2 = cl;
                    if (cl2 == null) continue;
                    if (cl2.Id == caller.Id) continue;
                    if (cl2.Me.Uid == uid)
                    {
                        removedCount++;
                        if (cl2.LoggedIn)
                            loggedInCount++;
                        Trace.WriteLine(String.Format("#Try stop client[{0}]", cl2.Id));
                        LazyAsync.Invoke(() => cl2.Stop());
                    }
                }
                //Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                return new Tuple<int, int>(loggedInCount, removedCount);
            }
        }

        private static void AcceptReceiveDataCallback(IAsyncResult ar)
        {
            // Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            lock (ClientLocker)
            {
                //Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
                // Get the socket that handles the client request.

                try
                {
                    var listener = (TcpListener) ar.AsyncState;
                    var ss = new SkySocket(listener.EndAcceptTcpClient(ar));
                    var c = new Client(ss, _nextId);
                    c.OnDisconnect += c_OnDisconnect;
                    Clients.Add(c);
                    //Logger.log(MethodInfo.GetCurrentMethod().Name, "Client " + _nextId.ToString() + " connected.");
                    _nextId++;
                }
                catch (ObjectDisposedException)
                {
                    Console.WriteLine("AcceptReceiveDataCallback: ObjectDisposedException");
                }
                catch (SocketException)
                {
                }
                AcceptClients();
                //Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "ClientLocker");
            }
        }

        private static void c_OnDisconnect(object sender, EventArgs e)
        {
            lock (ClientLocker)
            {
                var c = sender as Client;
                if (c == null)
                    return;
                c.OnDisconnect -= c_OnDisconnect;
                if (Clients.Exists(cl => cl.Id != c.Id && cl.LoggedIn && c.Me.Equals(cl.Me)))
                {
                }
                else
                {
                    LazyAsync.Invoke(() => OnUserEvent(UserStatus.Offline, c));
                }
            }
        }
    }
}