using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Skylabs.Lobby;

namespace Skylabs.LobbyServer
{
    public static class Gaming
    {
        //private static readonly object GamingLocker = new object();
        private static int _currentHostPort = 10000;
        private static readonly ReaderWriterLockSlim Locker; 

        static Gaming()
        {
            Locker = new ReaderWriterLockSlim();
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
            var hs = new HostedGame(_currentHostPort, g, v,"unknown", name, pass, u);
            hs.HostedGameDone += HostedGameExited;
            if (hs.StartProcess())
            {
                Games.Add(_currentHostPort, hs);
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
                    new Lobby.HostedGameData(g.Value.GameGuid, (Version) g.Value.GameVersion.Clone(), g.Value.Port,
                                            (string) g.Value.Name.Clone(), (User) g.Value.Hoster, g.Value.TimeStarted)
                        {GameStatus = g.Value.Status}).ToList();
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
                s.Status = Lobby.EHostedGame.StoppedHosting;
                Games.Remove(s.Port);
            Locker.ExitWriteLock();
        }
    }
}