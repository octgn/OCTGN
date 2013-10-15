using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        #region Clients

        private readonly List<ServerSocket> _clients = new List<ServerSocket>();
        private readonly List<PlayerInfo> _players = new List<PlayerInfo>(); 

        public ServerSocket[] Clients
        {
            get
            {
                try
                {
                    _locker.EnterReadLock();
                    return _clients.ToArray();
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
                    return _players.ToArray();
                }
                finally
                {
                    _locker.ExitReadLock();
                }
            }
        }

        public PlayerInfo this[ServerSocket index]
        {
            get
            {
                try
                {
                    _locker.EnterReadLock();
                    return _players.FirstOrDefault(x => x.Socket == index);
                }
                finally
                {
                    _locker.ExitReadLock();
                }
            }
            //set
            //{
            //    try
            //    {
            //        _locker.EnterWriteLock();

            //    }
            //    finally
            //    {
            //        _locker.ExitWriteLock();
            //    }
            //}
        }

        public ServerSocket[] DeadClients
        {
            get
            {
                try
                {
                    _locker.EnterReadLock();
                    return _clients.Where(x => !x.Client.Connected || new TimeSpan(DateTime.Now.Ticks - x.LastPingTime.Ticks).TotalSeconds > 240).ToArray();
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
                _clients.Add(socket);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public void RemoveClient(ServerSocket socket)
        {
            try
            {
                _locker.EnterWriteLock();
                _clients.Remove(socket);
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
                _clients.Clear();
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        #endregion Clients

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