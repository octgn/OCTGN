using System;

namespace Octgn
{
    public class IDHelper
    {
        public static Guid GlobalPlayerId { get; }

        public static Guid NewId() => Guid.NewGuid();

        static IDHelper() {
            GlobalPlayerId = new Guid((uint)0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
    }
}
