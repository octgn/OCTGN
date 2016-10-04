using System;

namespace Octgn.Library
{
    public class IDHelper
    {
        public static Guid GlobalPlayerId { get; }

        static IDHelper() {
            GlobalPlayerId = new Guid((uint)0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }
    }
}
