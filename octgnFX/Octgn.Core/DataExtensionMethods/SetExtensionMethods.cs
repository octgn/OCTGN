namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.IO;
    using System.Linq;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;

    public static class SetExtensionMethods
    {
        public static Uri GetPictureUri(this Set set, string path)
        {
            if (!Directory.Exists(set.ImagePackUri)) Directory.CreateDirectory(set.ImagePackUri);
            var files = Directory.GetFiles(set.ImagePackUri, path + ".*");
            if (files.Length == 0)
            {
                if (!Directory.Exists(set.ProxyPackUri)) Directory.CreateDirectory(set.ProxyPackUri);
                files = Directory.GetFiles(set.ProxyPackUri, path + ".png");
                if (files.Length == 0) return null;
                return new Uri(files.First());
            }
            return new Uri(files.First());
        }

        public static Game GetGame(this Set set)
        {
            return GameManager.Get().GetById(set.GameId);
        }

        public static Set AddCard(this Set set, params Card[] cards)
        {
            var temp = set.Cards.ToList();
            foreach (var c in cards)
            {
                if (temp.Any(x => x.Id == c.Id)) continue;
                temp.Add(c);
            }
            set.Cards = temp;
            return set;
        }
    }
}