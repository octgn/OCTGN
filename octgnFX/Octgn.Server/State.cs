using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Octgn.Core.Networking;

namespace Octgn.Server
{
    using Octgn.Online.Library;

    public class State
    {
        #region Singleton

        internal static State SingletonContext { get; set; }

        private static readonly object GameStateEngineSingletonLocker = new object();

        public static State Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (GameStateEngineSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new State();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        public readonly DateTime StartTime;
        public bool HasSomeoneJoined;

        public State()
        {
            StartTime = DateTime.UtcNow;
        }

        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private static IGameStateEngine _engineContext;
        public IGameStateEngine Engine
        {
            get
            {
                try
                {
                    _locker.EnterReadLock();
                    return _engineContext;
                }
                finally
                {
                    _locker.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _locker.EnterWriteLock();
                    _engineContext = value;
                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }
        }

        private readonly List<PlayerInfo> _players = new List<PlayerInfo>();

        public PlayerInfo[] Clients
        {
            get
            {
                try
                {
                    _locker.EnterReadLock();
                    return _players.ToArray();
                }
                finally
                {
                    _locker.ExitReadLock();
                }
            }
        }

        public PlayerInfo[] Players
        {
            get
            {
                try
                {
                    _locker.EnterReadLock();
                    return _players.Where(x=>x.SaidHello).ToArray();
                }
                finally
                {
                    _locker.ExitReadLock();
                }
            }
        }

        public PlayerInfo[] DeadClients
        {
            get
            {
                try
                {
                    _locker.EnterReadLock();
                    return _players.Where(x => x.Socket.Status != SocketStatus.Connected || new TimeSpan(DateTime.Now.Ticks - x.Socket.LastPingTime.Ticks).TotalSeconds > 240).ToArray();
                }
                finally
                {
                    _locker.ExitReadLock();
                }
            }
        }

        public void AddClient(ServerSocket socket)
        {
            try
            {
                _locker.EnterWriteLock();
                _players.Add(new PlayerInfo(socket));
                HasSomeoneJoined = true;
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void RemoveClient(PlayerInfo player)
        {
            try
            {
                _locker.EnterWriteLock();
                var rem = _players.FirstOrDefault(x => x.Socket == player.Socket);
                _players.Remove(rem);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void RemoveAllClients()
        {
            try
            {
                _locker.EnterWriteLock();
                _players.Clear();
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public PlayerInfo GetPlayer(TcpClient client)
        {
            return Players.FirstOrDefault(x => x.Socket.Client == client);
        }

        public PlayerInfo GetPlayer(ServerSocket client)
        {
            return Players.FirstOrDefault(x => x.Socket == client);
        }

        public PlayerInfo GetPlayer(byte id)
        {
            return Players.FirstOrDefault(x => x.Id == id);
        }

        public PlayerInfo GetClient(TcpClient client)
        {
            return Clients.FirstOrDefault(x => x.Socket.Client == client);
        }

        public PlayerInfo GetClient(ServerSocket client)
        {
            return Clients.FirstOrDefault(x => x.Socket == client);
        }

        public bool SaidHello(ServerSocket socket)
        {
            return SaidHello(socket.Client);
        }

        public bool SaidHello(TcpClient client)
        {
            try
            {
                _locker.EnterReadLock();
                var p = _players.FirstOrDefault(x => x.Socket.Client == client);
                if (p == null) return false;
                return p.SaidHello;
            }
            finally
            {
                _locker.ExitReadLock();
            }
        }

        private Handler _handler;

        public Handler Handler
        {
            get
            {
                try
                {
                    _locker.EnterReadLock();
                    return _handler;
                }
                finally
                {
                    _locker.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    _locker.EnterWriteLock();
                    _handler = value;
                }
                finally
                {
                    _locker.ExitWriteLock();
                }
            }
        }
    }
}