using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Skylabs.Lobby;
using Skylabs.Lobby.Sockets;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public static class Gaming
    {
        private static readonly object GamingLocker = new object();
        private static int _currentHostPort = 5000;
        private static long _totalHostedGames;

        static Gaming()
        {
            Games = new Dictionary<int, HostedGame>();
        }

        private static Dictionary<int, HostedGame> Games { get; set; }

        public static int GameCount()
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
            lock (GamingLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                var ret = Games.Count;
                Logger.UL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                return ret;
            }
        }

        public static void Stop()
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
            lock (GamingLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                foreach (var g in Games)
                {
                    g.Value.Stop();
                }
                Games.Clear();
                Logger.UL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
            }
        }

        public static long TotalHostedGames()
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
            lock (GamingLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                var ret = _totalHostedGames;
                Logger.UL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                return ret;
            }
        }

        public static int HostGame(Guid g, Version v, string name, string pass, User u)
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
            lock (GamingLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                while (Games.ContainsKey(_currentHostPort) || !Networking.IsPortAvailable(_currentHostPort))
                {
                    _currentHostPort++;
                    if (_currentHostPort >= 8000)
                        _currentHostPort = 5000;
                }
                var hs = new HostedGame(_currentHostPort, g, v, name, pass, u);
                hs.HostedGameDone += HostedGameExited;
                if (hs.StartProcess())
                {
                    Games.Add(_currentHostPort, hs);
                    _totalHostedGames++;
                    Logger.UL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                    return _currentHostPort;
                }
                Logger.UL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                hs.HostedGameDone -= HostedGameExited;
                return -1;
            }
        }

        public static void StartGame(int port)
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
            lock (GamingLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                try
                {
                    Games[port].Status = Lobby.HostedGame.EHostedGame.GameInProgress;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
                Logger.UL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
            }
        }

        public static List<Lobby.HostedGame> GetLobbyList()
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
            lock (GamingLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                var sendgames = Games.Select(g => new Lobby.HostedGame(g.Value.GameGuid, (Version) g.Value.GameVersion.Clone(), g.Value.Port, (string) g.Value.Name.Clone(), !String.IsNullOrWhiteSpace(g.Value.Password), (User) g.Value.Hoster.Clone(), g.Value.TimeStarted) {GameStatus = g.Value.Status}).ToList();
                Logger.UL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                return sendgames;
            }
        }

        private static void HostedGameExited(object sender, EventArgs e)
        {
            Logger.TL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
            lock (GamingLocker)
            {
                Logger.L(MethodBase.GetCurrentMethod().Name, "GamingLocker");
                var s = sender as HostedGame;
                if (s != null)
                {
                    s.Status = Lobby.HostedGame.EHostedGame.StoppedHosting;
                    var sm = new SocketMessage("gameend");
                    sm.AddData("port", s.Port);
                    Action t = () => Server.AllUserMessage(sm);
                    t.BeginInvoke(null, null);
                    Games.Remove(s.Port);
                }
                Logger.UL(MethodBase.GetCurrentMethod().Name, "GamingLocker");
            }
        }
    }
}