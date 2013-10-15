using System;

namespace Octgn.Server
{
    public sealed class PlayerInfo
    {
        /// <summary>
        /// Player Id
        /// </summary>
        internal readonly byte Id;
        /// <summary>
        /// Player Public Key
        /// </summary>
        internal readonly ulong Pkey;
        /// <summary>
        /// Software used
        /// </summary>
        internal readonly string Software;
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
        internal ServerSocket Socket;

        internal PlayerInfo(byte id, ServerSocket socket, string nick, ulong pkey, IClientCalls rpc, string software, bool spectator)
        {
            // Init fields
            Socket = socket;
            Id = id;
            Nick = nick;
            Rpc = rpc;
            Software = software;
            Pkey = pkey;
            IsSpectator = spectator;
            Connected = true;
        }
    }
}