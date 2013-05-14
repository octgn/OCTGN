namespace Octgn.Play
{
    public class Table : Group,IPlayTable
    {
        public Table(DataNew.Entities.Group def)
            : base(null, def)
        {
        }

        public void BringToFront(IPlayCard card)
        {
            if (card.Group != this) return;
            card.MoveToTable((int) card.X, (int) card.Y, card.FaceUp, Cards.Count);
        }

        public void SendToBack(IPlayCard card)
        {
            if (card.Group != this) return;
            card.MoveToTable((int) card.X, (int) card.Y, card.FaceUp, 0);
        }
    }
}