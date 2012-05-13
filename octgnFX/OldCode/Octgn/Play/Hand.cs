using Octgn.Definitions;

namespace Octgn.Play
{
    public sealed class Hand : Group
    {
        public Hand(Player owner, GroupDef def)
            : base(owner, def)
        {
        }
    }
}