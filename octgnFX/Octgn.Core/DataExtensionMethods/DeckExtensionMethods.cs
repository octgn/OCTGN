namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Linq;

    using Octgn.DataNew.Entities;

    public static class DeckExtensionMethods
    {
         public static void Save(this IDeck deck, string path)
         {
             //TODO [DB MIGRATION] DO THIS!
             throw new NotImplementedException("OH GODS");
         }
        public static IDeck Load(this IDeck deck, string path)
        {
            //TODO [DB MIGRATION]  DO THIS!
             throw new NotImplementedException("OH GODS");
            return new Deck();
        }
        public static int CardCount(this IDeck deck)
        {
            var qs = deck.Sections.SelectMany(x => x.Cards).Select(x => x.Quantity);
            return qs.Sum(x => x);
        }
        public static ObservableSection AsObservable(this ISection section)
        {
            if (section == null) return null;
            var ret = new ObservableSection();
            ret.Name = section.Name;
            ret.Cards = section.Cards;
            return ret;
        }
        public static ObservableMultiCard AsObservable(this IMultiCard card)
        {
            if (card == null) return null;
            var ret = new ObservableMultiCard
                          {
                              Id = card.Id,
                              Name = card.Name,
                              Properties = card.Properties.ToDictionary(x => x.Key, y => y.Value),
                              ImageUri = card.ImageUri,
                              IsMutable = card.IsMutable,
                              Alternate = card.Alternate,
                              SetId = card.SetId,
                              Dependent = card.Dependent,
                              Quantity = card.Quantity
                          };
            return ret;
        }
        public static ObservableDeck AsObservable(this IDeck deck)
        {
            if (deck == null) return null;
            var ret = new ObservableDeck { GameId = deck.GameId, IsShared = deck.IsShared, Sections = deck.Sections };
            return ret;
        }
    }
}