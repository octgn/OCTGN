using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Skylabs.Lobby;
using Skylabs.Lobby.Threading;
using Skylabs.Net;

namespace Skylabs.LobbyServer
{
    public static class Gaming
    {
        //private static readonly object GamingLocker = new object();
        private static int _currentHostPort = 5000;
        private static long _totalHostedGames;
        private static readonly ReaderWriterLockSlim Locker; 

        static Gaming()
        {
            Locker = new ReaderWriterLockSlim();
            Games = new Dictionary<int, HostedGame>();
        }

        private static Dictionary<int, HostedGame> Games { get; set; }

        public static int GameCount()
        {
            Locker.EnterReadLock();
                int ret = Games.Count;
            Locker.ExitReadLock();
            return ret;
        }

        public static HostedGame GetGame(int port)
        {
            HostedGame ret;
            Locker.EnterReadLock();
                Games.TryGetValue(port, out ret);
            Locker.ExitReadLock();
            return ret;
        }

        public static void Stop()
        {
            Locker.EnterWriteLock();
                foreach (var g in Games)
                {
                    g.Value.Stop();
                }
                Games.Clear();
            Locker.ExitWriteLock();
        }

        public static long TotalHostedGames()
        {
            Locker.EnterReadLock();
                long ret = _totalHostedGames;
            Locker.ExitReadLock();
            return ret;
            
        }

        public static int HostGame(Guid g, Version v, string name, string pass, User u)
        {
            Locker.EnterWriteLock();//Enter Lock
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
                Locker.ExitWriteLock();//Exit Lock
                return _currentHostPort;
            }
            hs.HostedGameDone -= HostedGameExited;
            Locker.ExitWriteLock();//Exit Lock
            return -1;

        }

        public static void StartGame(int port)
        {
            Locker.EnterWriteLock();
                try
                {
                    Games[port].Status = Lobby.HostedGameData.EHostedGame.GameInProgress;
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e);
                    if (Debugger.IsAttached) Debugger.Break();
                }
            Locker.ExitWriteLock();
        }

        public static List<Lobby.HostedGameData> GetLobbyList()
        {
            Locker.EnterReadLock();
            List<Lobby.HostedGameData> sendgames =
                Games.Select(
                    g =>
                    new Lobby.HostedGameData(g.Value.GameGuid, (Version) g.Value.GameVersion.Clone(), g.Value.Port,
                                            (string) g.Value.Name.Clone(), !String.IsNullOrWhiteSpace(g.Value.Password),
                                            (User) g.Value.Hoster.Clone(), g.Value.TimeStarted)
                        {GameStatus = g.Value.Status}).ToList();
            Locker.ExitReadLock();
            return sendgames;
        }

        private static void HostedGameExited(object sender, EventArgs e)
        {
            Locker.EnterWriteLock();
                var s = sender as HostedGame;
                if (s == null)
                    {Locker.ExitWriteLock();return;}
                s.Status = Lobby.HostedGameData.EHostedGame.StoppedHosting;
                var sm = new SocketMessage("gameend");
                sm.AddData("port", s.Port);
                LazyAsync.Invoke(() => Server.AllUserMessage(sm));
                Games.Remove(s.Port);
            Locker.ExitWriteLock();
        }
    }
}