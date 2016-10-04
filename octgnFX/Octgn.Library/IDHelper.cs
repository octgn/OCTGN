using System;

namespace Octgn
{
    public class IDHelper
    {
        public static Guid GlobalPlayerId { get; }
        public static Guid TableId { get; }

        public static Guid NewId() => Guid.NewGuid();

        static IDHelper() {
            GlobalPlayerId = new Guid((uint)0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            TableId = Guid.Parse("61d1644d-224d-453b-967e-9497445c661e");
        }
    }
}
