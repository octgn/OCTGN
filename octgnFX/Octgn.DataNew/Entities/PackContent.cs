namespace Octgn.DataNew.Entities
{
    using System.Collections.Generic;

    public class PackContent
    {
        public PackContent()
        {
            LimitedCards = new List<Card>();
            UnlimitedCards = new List<Card>();
        }

        public List<Card> LimitedCards { get; private set; }
        public List<Card> UnlimitedCards { get; private set; }

        public void Merge(PackContent content)
        {
            LimitedCards.AddRange(content.LimitedCards);
            UnlimitedCards.AddRange(content.UnlimitedCards);
        }
    }
}