using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Skylabs.Lobby;
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
            lock (GamingLocker)
            {
                int ret = Games.Count;
                return ret;
            }
        }

        public static void Stop()
        {
            lock (GamingLocker)
            {
                foreach (KeyValuePair<int, HostedGame> g in Games)
                {
                    g.Value.Stop();
                }
                Games.Clear();
            }
        }

        public static long TotalHostedGames()
        {
            lock (GamingLocker)
            {
                long ret = _totalHostedGames;
                return ret;
            }
        }

        public static int HostGame(Guid g, Version v, string name, string pass, User u)
        {
            lock (GamingLocker)
            {
                while (Games.ContainsKey(_currentHostPort) || !Networking.IsPortAvailable(_currentHostPort))
                {
                    _currentHostPort++;
                    if (_currentHostPort >= 20000)
                        _currentHostPort = 10000;
                }
                var hs = new HostedGame(_currentHostPort, g, v, name, pass, u);
                hs.HostedGameDone += HostedGameExited;
                if (hs.StartProcess())
                {
                    Games.Add(_currentHostPort, hs);
                    _totalHostedGames++;
                    return _currentHostPort;
                }
                hs.HostedGameDone -= HostedGameExited;
                return -1;
            }
        }

        public static void StartGame(int port)
        {
            lock (GamingLocker)
            {
                try
                {
                    Games[port].Status = Lobby.HostedGameData.EHostedGame.GameInProgress;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
            }
        }

        public static List<Lobby.HostedGameData> GetLobbyList()
        {
            lock (GamingLocker)
            {
                List<Lobby.HostedGameData> sendgames =
                    Games.Select(
                        g =>
                        new Lobby.HostedGameData(g.Value.GameGuid, (Version) g.Value.GameVersion.Clone(), g.Value.Port,
                                             (string) g.Value.Name.Clone(), !String.IsNullOrWhiteSpace(g.Value.Password),
                                             (User) g.Value.Hoster.Clone(), g.Value.TimeStarted)
                            {GameStatus = g.Value.Status}).ToList();
                return sendgames;
            }
        }

        private static void HostedGameExited(object sender, EventArgs e)
        {
            lock (GamingLocker)
            {
                var s = sender as HostedGame;
                if (s == null) return;
                s.Status = Lobby.HostedGameData.EHostedGame.StoppedHosting;
                var sm = new SocketMessage("gameend");
                sm.AddData("port", s.Port);
                Action t = () => Server.AllUserMessage(sm);
                t.BeginInvoke(null, null);
                Games.Remove(s.Port);
            }
        }
    }
}