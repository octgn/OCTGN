using System;
using System.Reflection;
using log4net;
using Octgn.Site.Api;
using Octgn.Site.Api.Models;

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

        internal bool ReportDisconnect;

        internal PlayerInfo(ServerSocket socket)
        {
            Socket = socket;
            Connected = true;
        }

        internal void Setup(byte id, string nick, ulong pkey, IClientCalls rpc, string software, bool spectator)
        {
            Id = id;
            Nick = nick;
            Rpc = rpc;
            Software = software;
            Pkey = pkey;
            IsSpectator = spectator;
            ReportDisconnect = true;
        }

        internal void ResetSocket(ServerSocket socket)
        {
            Socket = socket;
        }

        internal void Disconnect()
        {
            this.Connected = false;
            this.TimeDisconnected = DateTime.Now;
            Socket.Disconnect();
            if (ReportDisconnect && State.Instance.Engine.IsLocal == false && State.Instance.Handler.GameStarted)
            {
                // TODO Report this player disconnecting in game.
                try
                {
                    var c = new ApiClient();
                    var req = new PutDisconnectReq(State.Instance.Engine.ApiKey, State.Instance.Engine.Game.Id.ToString(), this.Nick);
                    c.SetUserDisconnectedGameHistory(req);
                }
                catch (Exception e)
                {
                    Log.Error("Disconnect Error reporting disconnect",e);
                }
            }
            if(this.SaidHello)
                new Broadcaster(State.Instance.Handler).PlayerDisconnect(Id);
        }

        internal void Kick(string message, params object[] args)
        {
            ReportDisconnect = false;
            var mess = string.Format(message, args);
            this.Connected = false;
            this.TimeDisconnected = DateTime.Now;
            var rpc = new BinarySenderStub(this.Socket,State.Instance.Handler);
            rpc.Kick(mess);
            Socket.Disconnect();
            if (SaidHello)
            {
                new Broadcaster(State.Instance.Handler)
                    .Error(string.Format("Player {0} was kicked: {1}", Nick, mess));
            }
            SaidHello = false;
        }
    }
}