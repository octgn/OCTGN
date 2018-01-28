using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Octgn.Library.Networking;
using Octgn.Online.Hosting;

namespace Octgn.Server
{
    public class State
    {
        public bool HasSomeoneJoined;
        public HostedGame Game { get; set; }
        public bool IsLocal { get; set; }
        public bool IsDebug { get; set; }
        public string ApiKey { get; set; }
        public Handler Handler { get; }
        internal Broadcaster Broadcaster { get; }

        public State(HostedGame game, bool isLocal, bool isDebug)
        {
            Game = game;
            IsLocal = isLocal;
            IsDebug = isDebug;
            Handler = new Handler(this);
            Broadcaster = new Broadcaster(this);
        }

        private readonly ReaderWriterLockSlim _locker = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly List<PlayerInfo> _players = new List<PlayerInfo>();

		private readonly List<ulong> _kickedPlayers = new List<ulong>();

        private readonly List<string> _dcPlayers = new List<string>();

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

        public ulong[] KickedPlayers
        {
            get
            {
                try
                {
					_locker.EnterReadLock();
                    return _kickedPlayers.ToArray();
                }
                finally
                {
                    _locker.ExitReadLock();
                }
            }
        }

        public string[] DcUsers
        {
            get
            {
                lock (_dcPlayers)
                {
                    return _dcPlayers.ToArray();
                }
            }
        }

        public void AddClient(ServerSocket socket)
        {
            try
            {
                _locker.EnterWriteLock();
                _players.Add(new PlayerInfo(this, socket));
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

		public void AddKickedPlayer(PlayerInfo pinfo)
		{
		    try
		    {
				_locker.EnterWriteLock();
				_kickedPlayers.Add(pinfo.Pkey);
		    }
		    finally
		    {
		        _locker.ExitWriteLock();
		    }
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

        public void UpdateDcPlayer(string name, bool dc)
        {
            lock (_dcPlayers)
            {
                var n = name.ToLowerInvariant();
                if (dc)
                {
                    if (_dcPlayers.Contains(n))
                        return;
                    _dcPlayers.Add(name);
                }
                else
                {
                    if (_dcPlayers.Contains(n) == false)
                        return;
                    _dcPlayers.Remove(n);
                }
            }
        }
    }
}