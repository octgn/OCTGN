using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Skylabs.Lobby;

namespace Skylabs.LobbyServer
{
    using System.IO;
    using System.Threading.Tasks;

    public static class Gaming
    {
        //private static readonly object GamingLocker = new object();
        private static int _currentHostPort = 10000;
        private static readonly ReaderWriterLockSlim Locker;

        static Gaming()
        {
            Locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            Games = new Dictionary<int, HostedGame>();
        }

        private static Dictionary<int, HostedGame> Games { get; set; }

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

        public static int HostGame(Guid g, Version v, string name, string pass, User u)
        {
            Locker.EnterWriteLock();//Enter Lock
            while (Games.ContainsKey(_currentHostPort) || !Networking.IsPortAvailable(_currentHostPort))
            {
                _currentHostPort++;
                if (_currentHostPort >= 20000)
                    _currentHostPort = 10000;
            }
            var hs = new HostedGame(_currentHostPort, g, v, "unknown", name, pass, u, true, true);
            hs.HostedGameDone += HostedGameExitedEventLauncher;
            if (hs.StartProcess())
            {
                Games.Add(_currentHostPort, hs);
                Locker.ExitWriteLock();//Exit Lock
                return _currentHostPort;
            }
            hs.HostedGameDone -= HostedGameExitedEventLauncher;
            Locker.ExitWriteLock();//Exit Lock
            return -1;

        }

        private static void HostedGameExitedEventLauncher(object sender, EventArgs e)
        {
            Task.Factory.StartNew(
                () => HostedGameExited(sender, e));
        }

        public static void StartGame(int port)
        {
            Locker.EnterWriteLock();
            try
            {
                Games[port].Status = Lobby.EHostedGame.GameInProgress;
            }
            catch (Exception e)
            {
                Logger.Er(e);
            }
            Locker.ExitWriteLock();
        }

        public static List<Lobby.HostedGameData> GetLobbyList()
        {
            Locker.EnterReadLock();
            List<Lobby.HostedGameData> sendgames =
                Games.Select(
                    g =>
                    new Lobby.HostedGameData(g.Value.GameGuid, (Version)g.Value.GameVersion.Clone(), g.Value.Port,
                                            (string)g.Value.Name.Clone(), (User)g.Value.Hoster, g.Value.TimeStarted,g.Value.GameName, !String.IsNullOrWhiteSpace(g.Value.Password)) { GameStatus = g.Value.Status }).ToList();
            Locker.ExitReadLock();
            return sendgames;
        }

        private static void HostedGameExited(object sender, EventArgs e)
        {
            Locker.EnterWriteLock();
            var s = sender as HostedGame;
            if (s == null)
            {
                Locker.ExitWriteLock();
                return;
            }
            try
            {
                s.HostedGameDone -= HostedGameExitedEventLauncher;
                var dir = new FileInfo(s.StandAloneApp.StartInfo.FileName).Directory;
                dir.Delete(true);
            }
            catch (Exception ex)
            {
                Logger.Er(ex);
            }
            s.Status = Lobby.EHostedGame.StoppedHosting;
            Games.Remove(s.Port);
            Locker.ExitWriteLock();
        }
    }
}