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
        private static int _currentHostPort = 10000;
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
            Logger.PreLock();
            Locker.EnterReadLock();
            Logger.InLock();
                int ret = Games.Count;
            Locker.ExitReadLock();
            Logger.EndLock();
            return ret;
        }

        public static HostedGame GetGame(int port)
        {
            HostedGame ret;
            Logger.PreLock();
            Locker.EnterReadLock();
            Logger.InLock();
                Games.TryGetValue(port, out ret);
            Locker.ExitReadLock();
            Logger.EndLock();
            return ret;
        }

        public static void Stop()
        {
            Logger.PreLock();
            Locker.EnterWriteLock();
            Logger.InLock();
                foreach (var g in Games)
                {
                    g.Value.Stop();
                }
                Games.Clear();
            Locker.ExitWriteLock();
            Logger.EndLock();
        }

        public static long TotalHostedGames()
        {
            Logger.PreLock();
            Locker.EnterReadLock();
            Logger.InLock();
                long ret = _totalHostedGames;
            Locker.ExitReadLock();
            Logger.EndLock();
            return ret;
            
        }

        public static int HostGame(Guid g, Version v, string name, string pass, User u)
        {
            Logger.PreLock();
            Locker.EnterWriteLock();//Enter Lock
            Logger.InLock();
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
                Logger.EndLock();
                return _currentHostPort;
            }
            hs.HostedGameDone -= HostedGameExited;
            Locker.ExitWriteLock();//Exit Lock
            Logger.EndLock();
            return -1;

        }

        public static void StartGame(int port)
        {
            Logger.PreLock();
            Locker.EnterWriteLock();
            Logger.InLock();
                try
                {
                    Games[port].Status = Lobby.EHostedGame.GameInProgress;
                }
                catch (Exception e)
                {
                    Logger.Er(e);
                }
            Locker.ExitWriteLock();
            Logger.EndLock();
        }

        public static List<Lobby.HostedGameData> GetLobbyList()
        {
            Logger.PreLock();
            Locker.EnterReadLock();
            Logger.InLock();
            List<Lobby.HostedGameData> sendgames =
                Games.Select(
                    g =>
                    new Lobby.HostedGameData(g.Value.GameGuid, (Version) g.Value.GameVersion.Clone(), g.Value.Port,
                                            (string) g.Value.Name.Clone(), (User) g.Value.Hoster, g.Value.TimeStarted)
                        {GameStatus = g.Value.Status}).ToList();
            Locker.ExitReadLock();
            Logger.EndLock();
            return sendgames;
        }

        private static void HostedGameExited(object sender, EventArgs e)
        {
            Logger.PreLock();
            Locker.EnterWriteLock();
            Logger.InLock();
                var s = sender as HostedGame;
                if (s == null)
                {
                    Locker.ExitWriteLock();
                    Logger.EndLock();
                    return;
                }
                s.Status = Lobby.EHostedGame.StoppedHosting;
                Games.Remove(s.Port);
            Locker.ExitWriteLock();
            Logger.EndLock();
            GameBot.RefreshLists();
        }
    }
}