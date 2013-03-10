namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew;
    using Octgn.DataNew.Entities;

    public static class SetExtensionMethods
    {
        public static IEnumerable<Card> Cards(this Set set)
        {
            return DbContext.Get().Cards.Where(x => x.SetId == set.Id);
        }
        public static string GetPackUri(this Set set)
        {
            return "pack://file:,,," + set.PackageName.Replace('\\', ',');
        }

        public static Uri GetPictureUri(this Set set, string path)
        {
            return new Uri(set.GetPackUri() + path);
        }

        public static Game GetGame(this Set set)
        {
            return GameManager.Get().Games.FirstOrDefault(x=>x.Id == set.GameId);
        }

        public static Set AddCard(this Set set, params Card[] cards)
        {
            foreach (var c in cards)
            {
                c.SetId = set.Id;
                DbContext.Get().Save(c);
            }
            return set;
        }
    }
}