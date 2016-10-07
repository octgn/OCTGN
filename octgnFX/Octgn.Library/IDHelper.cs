using System;

namespace Octgn
{
    public class IDHelper
    {
        public static Guid LocalHostedGameId { get; }
        public static Guid GlobalPlayerId { get; }
        public static Guid TableId { get; }

        public static Guid NewId() => Guid.NewGuid();

        static IDHelper() {
            // 0001 - Player
            // 0002 - Card
            // 0003 - Group
            // 0004 -
            // 0004 - Game ID
            GlobalPlayerId = Guid.Parse("0C76171D-0000-0000-0001-000000000000");
            GlobalPlayerId = Guid.Parse("0C76171D-0000-0000-0003-000000000000");
            LocalHostedGameId = Guid.Parse("0C76171D-0000-0000-0005-000000000000");
        }
    }
}
