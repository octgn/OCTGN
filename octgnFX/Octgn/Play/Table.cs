namespace Octgn.Play
{
    public class Table : Group
    {
        public Table(DataNew.Entities.Group def)
            : base(null, def)
        {
        }

        public void BringToFront(Card card)
        {
            if (card.Group != this) return;
			card.MoveToTable((int) card.X, (int) card.Y, card.FaceUp, Count,false);
        }

        public void SendToBack(Card card)
        {
            if (card.Group != this) return;
            card.MoveToTable((int) card.X, (int) card.Y, card.FaceUp, 0,false);
        }
    }
}