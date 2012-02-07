using Octgn.Definitions;
using Octgn.Play.Actions;

namespace Octgn.Play
{
    public class Table : Group
    {
        public Table(GroupDef def)
            : base(null, def)
        { }

        public void BringToFront(Card card)
        {
            if (card.Group != this) return;
            card.MoveToTable((int)card.X, (int)card.Y, card.FaceUp, Cards.Count);
        }

        public void SendToBack(Card card)
        {
            if (card.Group != this) return;
            card.MoveToTable((int)card.X, (int)card.Y, card.FaceUp, 0);
        }
    }
}