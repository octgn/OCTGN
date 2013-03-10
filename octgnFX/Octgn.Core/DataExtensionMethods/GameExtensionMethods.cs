namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO.Abstractions;
    using System.Linq;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;
    using Octgn.Library;

    public static class GameExtensionMethods
    {
        internal static IFileSystem IO {
            get
            {
                return io ?? (io = new FileSystem());
            }
            set
            {
                io = value;
            }
        }
        private static IFileSystem io;

        public static string GetFullPath(this Game game)
        {
            var ret = "";
            ret = IO.Path.Combine(Paths.DataDirectory, "Games");
            ret = IO.Path.Combine(ret, game.Id.ToString());
            ret = IO.Path.Combine(ret, "Defs");
            ret = IO.Path.Combine(ret, game.Filename);
            return ret;
        }

        public static Deck CreateDeck(this Game game)
        {
            var deck = new Deck { GameId = game.Id };
            return deck;
        }

        public static string GetInstallPath(this Game game)
        {
            return IO.Path.Combine(IO.Path.Combine(Paths.DataDirectory, "Games"), game.Id.ToString());
        }

        public static Uri GetCardBackUri(this Game game)
        {
            var path = IO.Path.Combine(game.GetInstallPath(), game.CardBack);
            var ret = new Uri(path);
            return ret;
        }

        public static string GetDefaultDeckPath(this Game game)
        {
            return IO.Path.Combine(Paths.DataDirectory, "Decks");
        }

        public static Card GetCardByName(this Game game, string name)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return null;
            return g.Sets.SelectMany(x => x.Cards).FirstOrDefault(x => x.Name == name);
        }

        public static Card GetCardById(this Game game, Guid id)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return null;
            return g.Sets.SelectMany(x => x.Cards).FirstOrDefault(x => x.Id == id);
        }

        public static Set GetSetById(this Game game, Guid id)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return null;
            return g.Sets.FirstOrDefault(x => x.Id == id);
        }

        public static IEnumerable<Marker> GetAllMarkers( this Game game)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return new List<Marker>();
            return g.Sets.SelectMany(x => x.Markers);
        }

        public static Pack GetPackById(this Game game, Guid id)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return null;
            return g.Sets.SelectMany(x => x.Packs).FirstOrDefault(x => x.Id == id);
        }

        public static IEnumerable<PropertyDef> AllProperties(this Game game)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return new List<PropertyDef>();
            return Enumerable.Repeat(new PropertyDef{Name="Name", Type = PropertyType.String}, 1).Union(game.CustomProperties);
        }

        public static IEnumerable<Card> AllCards(this Game game)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return new List<Card>();
            return g.Sets.SelectMany(x => x.Cards);
        }

        public static DataTable ToDataTable(this IEnumerable<Card> cards)
        {
            var ret = new DataTable();
            foreach (var p in typeof(Card).GetProperties())
            {
                ret.Columns.Add(p.Name, p.PropertyType);
            }
            foreach (var list in cards.Select(c => typeof(Card).GetProperties().Select(p => p.GetValue(c, new object[] { })).ToList()))
            {
                ret.Rows.Add(list);
            }
            return ret;
        }
    }
}