namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Linq;

    using Octgn.DataNew.Entities;

    public static class DeckExtensionMethods
    {
         public static void Save(this Deck deck, string path)
         {
             //TODO DO THIS!
             throw new NotImplementedException("OH GODS");
         }
        public static Deck Load(this Deck deck, string path)
        {
             //TODO DO THIS!
             throw new NotImplementedException("OH GODS");
            return new Deck();
        }
        public static int CardCount(this Deck deck)
        {
            var qs = deck.Sections.SelectMany(x => x.Cards).Select(x => x.Quantity);
            return qs.Sum(x => x);
        }
    }
}