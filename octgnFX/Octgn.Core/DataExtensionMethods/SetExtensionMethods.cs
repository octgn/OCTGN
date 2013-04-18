namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew;
    using Octgn.DataNew.Entities;

    public static class SetExtensionMethods
    {
        public static string GetInstallPath(this Set set)
        {
            return Path.Combine(set.GetGame().GetInstallPath(), "Sets", set.Id.ToString());
        }
        public static string GetDeckUri(this Set set)
        {
            return Path.Combine(set.GetInstallPath(), "Decks");
        }
        public static string GetPackUri(this Set set)
        {
            return Path.Combine(set.GetInstallPath(), "Cards");
            //return "pack://file:,,," + set.PackageName.Replace('\\', ',');
        }

        public static string GetPackProxyUri(this Set set)
        {
            return Path.Combine(set.GetPackUri(), "Proxies");
        }

        public static Uri GetPictureUri(this Set set, string path)
        {
            if (!Directory.Exists(set.GetPackUri())) Directory.CreateDirectory(set.GetPackUri());
            var files = Directory.GetFiles(set.GetPackUri(), path + ".*");
            if (files.Length == 0)
            {
                if (!Directory.Exists(set.GetPackProxyUri())) Directory.CreateDirectory(set.GetPackProxyUri());
                files = Directory.GetFiles(set.GetPackProxyUri(), path + ".png");
                if (files.Length == 0) return null;
                return new Uri(files.First());
            }
            return new Uri(files.First());
        }

        public static Game GetGame(this Set set)
        {
            return GameManager.Get().Games.FirstOrDefault(x=>x.Id == set.GameId);
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