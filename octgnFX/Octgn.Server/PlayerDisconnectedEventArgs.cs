using System;

namespace Octgn.Server
{
    public class PlayerDisconnectedEventArgs : EventArgs
    {
        public const string TimeoutReason = "TimedOut";
        public const string DisconnectedReason = "Disconnected";
        public const string KickedReason = "Kicked";
        public const string ShutdownReason = "Shutdown";
        public const string ConnectionReplacedReason = "ConnectionReplaced";
        public const string LeaveReason = "Leave";

        public Player Player { get; set; }
        public string Reason { get; set; }
        public string Details { get; set; }

        public PlayerDisconnectedEventArgs(Player player, string reason, string details) {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Reason = reason ?? throw new ArgumentNullException(nameof(reason));
            Details = details;
        }
    }
}