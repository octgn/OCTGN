using System;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using log4net;
using Octgn.Library.Networking;
using Octgn.Site.Api;
using Octgn.Site.Api.Models;

namespace Octgn.Server
{
    public sealed class Player
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const byte HOSTPLAYERID = 1;

        /// <summary>
        /// Player Id
        /// </summary>
        public byte Id { get; private set; }

        /// <summary>
        /// Player Public Key
        /// </summary>
        public ulong Pkey { get; private set; }
        /// <summary>
        /// Software used
        /// </summary>
        public string Software { get; private set; }
        /// <summary>
        /// Is Connected
        /// </summary>
        public bool Connected { get; private set; }
        public PlayerSettings Settings {
            get => _settings;
            set {
                _settings = value;

                Rpc.PlayerSettings(Id, _settings.InvertedTable, _settings.IsSpectator);
                _context.Broadcaster.PlayerSettings(this.Id, _settings.InvertedTable, _settings.IsSpectator);
            }
        }
        private PlayerSettings _settings;

        /// <summary>
        /// Time Disconnected
        /// </summary>
        public DateTime TimeDisconnected { get; private set; } = DateTime.Now;
        /// <summary>
        /// Player Nickname
        /// </summary>
        public string Nick { get; private set; }
        public string UserId { get; private set; }
        /// <summary>
        /// Did the player say hello?
        /// </summary>
        public bool SaidHello { get; set; }

        public IClientCalls Rpc => _socket.Rpc;

        public bool IsTimedOut
            => TimeSinceLastPing.TotalSeconds >= _context.Config.PlayerTimeoutSeconds;

        public TimeSpan TimeSinceLastPing => new TimeSpan(DateTime.Now.Ticks - _socket.LastPingTime.Ticks);

        public int TotalPingsReceived => _socket.PingsReceived;

        public bool IsSubbed { get; private set; }

        /// <summary>
        /// Is this player part of a local game or a Octgn.Net hosted game.
        /// </summary>
        private readonly GameContext _context;

        public event EventHandler<PlayerDisconnectedEventArgs> Disconnected;

        internal Player(ServerSocket socket, GameContext context) {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _settings = new PlayerSettings(false, false);
            ResetSocket(socket);
        }

        internal void Setup(byte id, string nick, string userId, ulong pkey, string software, bool invertedTable, bool spectator, PlayerCollection collection) {
            Id = id;
            Nick = nick;
            UserId = userId;
            Software = software;
            Pkey = pkey;
            Settings = new PlayerSettings(invertedTable, spectator);
            _collection = collection ?? throw new ArgumentNullException(nameof(collection));

            if (!_context.Config.IsLocal) {
                var client = new ApiClient();
                var result = client.IsSubbed(nick, _context.Config.ApiKey);

                IsSubbed = result == IsSubbedResult.Ok;
            }
        }

        private ServerSocket _socket;
        private readonly object SOCKETLOCKER = new object();

        internal void ResetSocket(Player player) {
            lock (SOCKETLOCKER) {
                ResetSocket(player._socket);
                player._socket.ConnectionChanged -= player.Socket_OnConnectionChanged;
                player._socket.DataReceived -= player.Socket_DataReceived;
                player._socket = null;
                player.Connected = false;
                player.TimeDisconnected = DateTime.Now;
                Disconnected?.Invoke(player, new PlayerDisconnectedEventArgs(player, PlayerDisconnectedEventArgs.ConnectionReplacedReason, string.Empty));
            }
        }

        internal void ResetSocket(ServerSocket socket) {
            lock (SOCKETLOCKER) {
                if (_socket != null) {
                    _socket.ConnectionChanged -= Socket_OnConnectionChanged;
                    _socket.DataReceived -= Socket_DataReceived;
                }

                _socket = socket;

                this.Connected = true;
                _socket.Handler.SetPlayer(this);
                _socket.ConnectionChanged += Socket_OnConnectionChanged;
                _socket.DataReceived += Socket_DataReceived;
            }
        }

        internal void DisconnectSocket(string reason, string details) {
            lock (SOCKETLOCKER) {
                var socket = _socket;
                _socket = null;
                if (socket == null) return;

                this.Connected = false;
                TimeDisconnected = DateTime.Now;

                socket.Disconnect();
            }
            Disconnected?.Invoke(this, new PlayerDisconnectedEventArgs(this, reason, details));
        }

        private void Socket_DataReceived(object sender, DataReceivedEventArgs e) {
            _context.Run(() => {
                _socket.OnPingReceived();

                if (!SaidHello) {
                    if (!BinaryParser.AnonymousCalls.Contains(e.Data[4])) {
                        Log.Warn($"{this}: Packet Dropped(Hello Not Received Yet): {e.Data[4]}");

                        return;
                    }
                }

                _socket.Serializer.Parse(e.Data.Skip(4).ToArray());
            });
        }

        private void Socket_OnConnectionChanged(object sender, ConnectionChangedEventArgs e) {
            if (e.Event == SocketConnectionEvent.Disconnected) {
                Log.Info($"{this} - Disconnected");
                DisconnectSocket(PlayerDisconnectedEventArgs.DisconnectedReason, "Socket Disconnected");
            }
        }

        private PlayerCollection _collection;

        /// <summary>
        /// Disconnect the player.
        /// </summary>
        /// <param name="reason">Reason for disconnection. You can find reasons in <see cref="PlayerDisconnectedEventArgs"/></param>
        internal void Disconnect(string reason, string details) {
            Log.Info($"{this} - Called Disconnect because {reason} - {details}");
            DisconnectSocket(reason, details);
        }

        public void Kick(string message, params object[] args) {
            _collection.AddKickedPlayer(this);

            var mess = string.Format(message, args);
            Rpc.Kick(mess);
            DisconnectSocket(PlayerDisconnectedEventArgs.KickedReason, mess);
            SaidHello = false;
        }

        public void Leave() {
            DisconnectSocket(PlayerDisconnectedEventArgs.LeaveReason, string.Empty);
            if (_context.Config.IsLocal == false) {
                var mess = new GameMessage();
                // don't send if we join our own room...that'd be annoying
                if (UserId.Equals(_context.Game.HostUser.Id, StringComparison.InvariantCultureIgnoreCase)) return;
                mess.Message = string.Format("{0} has left your game", Nick);
                mess.Sent = DateTime.Now;
                mess.SessionId = _context.Game.Id;
                mess.Type = GameMessageType.Event;
                new Octgn.Site.Api.ApiClient().GameMessage(_context.Config.ApiKey, mess);
            }
        }

        public void Ping() {
            Rpc.Ping();
        }

        public override string ToString() {
            return $"Player {Id}:{Nick}:{UserId}";
        }

        public bool Equals(TcpClient client) {
            return _socket.Client == client;
        }
    }
}