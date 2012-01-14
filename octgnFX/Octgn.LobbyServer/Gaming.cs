using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Skylabs.Lobby;
using Skylabs.Net;
using System.Threading;

namespace Skylabs.LobbyServer
{
    public static class Gaming
    {
        private static Dictionary<int, HostedGame> Games { get; set; }

        private static readonly object GamingLocker = new object();

        private static int _currentHostPort = 5000;

        private static long _totalHostedGames = 0;

        static Gaming()
        {
            Games = new Dictionary<int, HostedGame>();
        }
        public static int GameCount()
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
            lock (GamingLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
                int ret = Games.Count;
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
                return ret;
            }
        }
        public static long TotalHostedGames()
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
            lock (GamingLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
                long ret = _totalHostedGames;
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
                return ret;
            }
        }
        public static int HostGame(Guid g,Version v, string name,string pass,User u)
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
            lock(GamingLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
                while (Games.ContainsKey(_currentHostPort) || !Networking.IsPortAvailable(_currentHostPort))
                {
                    _currentHostPort++;
                    if (_currentHostPort >= 8000)
                        _currentHostPort = 5000;
                    
                }
                HostedGame hs = new HostedGame(_currentHostPort,g,v,name,pass,u);
                hs.HostedGameDone += HostedGameExited;
                if (hs.StartProcess())
                {
                    Games.Add(_currentHostPort, hs);
                    _totalHostedGames++;
                    Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
                    return _currentHostPort;
                }
                else
                {
                    Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
                    hs.HostedGameDone -= HostedGameExited;
                    return -1;
                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
            }
        }
        public static void StartGame(int port)
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
            lock(GamingLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
                try
                {
                    Games[port].Status = Lobby.HostedGame.eHostedGame.GameInProgress;
                }
                catch (Exception)
                {

                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
            }
        }
        public static List<Skylabs.Lobby.HostedGame> GetLobbyList()
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
            lock(GamingLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
                List<Lobby.HostedGame> sendgames = new List<Lobby.HostedGame>();
                foreach(KeyValuePair<int,HostedGame> g in Games)
                {
                    Lobby.HostedGame newhg =
                        new Lobby.HostedGame(g.Value.GameGuid, (Version)g.Value.GameVersion.Clone(),
                            g.Value.Port, (string)g.Value.Name.Clone(),
                            !String.IsNullOrWhiteSpace(g.Value.Password), (User)g.Value.Hoster.Clone());
                    newhg.GameStatus = g.Value.Status;
                    sendgames.Add(newhg);
                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
                return sendgames;
            }
        }
        private static void HostedGameExited(object sender,EventArgs e)
        {
            Logger.TL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
            lock (GamingLocker)
            {
                Logger.L(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
                HostedGame s = sender as HostedGame;
                if (s != null)
                {
                    s.Status = Lobby.HostedGame.eHostedGame.StoppedHosting;
                    SocketMessage sm = new SocketMessage("gameend");
                    sm.AddData("port", s.Port);
                    Thread t = new Thread(()=>Server.AllUserMessage(sm));
                    t.Start();
                    Games.Remove(s.Port);
                }
                Logger.UL(System.Reflection.MethodInfo.GetCurrentMethod().Name, "GamingLocker");
            }
        }
    }
}
