using System;
using System.Reflection;
using log4net;
using Octgn.Library.Localization;

namespace Octgn.Server
{
    public sealed class PlayerInfo
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Player Id
        /// </summary>
        internal byte Id;
        /// <summary>
        /// Player Public Key
        /// </summary>
        internal ulong Pkey;
        /// <summary>
        /// Software used
        /// </summary>
        internal string Software;
        /// <summary>
        /// Uses binary protocol
        /// </summary>
        internal bool Binary;
        /// <summary>
        /// Is Connected
        /// </summary>
        internal bool Connected;
        /// <summary>
        /// Time Disconnected
        /// </summary>
        internal DateTime TimeDisconnected = DateTime.Now;
        /// <summary>
        /// When using a two-sided table, indicates whether this player plays on the opposite side
        /// </summary>
        internal bool InvertedTable;
        /// <summary>
        /// Player Nickname
        /// </summary>
        internal string Nick;
        internal string UserId;
        /// <summary>
        /// Stubs to send messages to the player
        /// </summary>
        internal IClientCalls Rpc;
        /// <summary>
        /// Is player a spectator
        /// </summary>
        internal bool IsSpectator;
        /// <summary>
        /// Socket for the client
        /// </summary>
        internal ServerSocket Socket { get; private set; }
		/// <summary>
		/// Did the player say hello?
		/// </summary>
        internal bool SaidHello;

        private readonly State _state;

        internal PlayerInfo(State state, ServerSocket socket)
        {
            _state = state;
            Socket = socket;
            Connected = true;
        }

        internal void Setup(byte id, string nick, string userId, ulong pkey, IClientCalls rpc, string software, bool spectator)
        {
            Id = id;
            Nick = nick;
            UserId = userId;
            Rpc = rpc;
            Software = software;
            Pkey = pkey;
            IsSpectator = spectator;
        }

        internal void ResetSocket(ServerSocket socket)
        {
            Socket = socket;
        }

        internal void Disconnect(bool report)
        {
            Connected = false;
            Socket.Disconnect();
            Connected = true;
            OnDisconnect(report);
        }

        internal void OnDisconnect(bool report)
        {
            lock (this)
            {
                if (Connected == false)
                    return;
                this.Connected = false;
            }
            this.TimeDisconnected = DateTime.Now;
            if (this.SaidHello)
                new Broadcaster(_state).PlayerDisconnect(Id);
            if (report && _state.IsLocal == false && _state.Handler.GameStarted && this.IsSpectator == false)
            {
                _state.UpdateDcPlayer(this.Nick,true);
            }
        }

        internal void Kick(bool report, string message, params object[] args)
        {
            var mess = string.Format(message, args);
            this.Connected = false;
            this.TimeDisconnected = DateTime.Now;
            var rpc = new BinarySenderStub(this.Socket,_state.Handler);
            rpc.Kick(mess);
            //Socket.Disconnect();
            Disconnect(report);
            if (SaidHello)
            {
                new Broadcaster(_state)
                    .Error(string.Format(L.D.ServerMessage__PlayerKicked, Nick, mess));
            }
            SaidHello = false;
        }
    }
}