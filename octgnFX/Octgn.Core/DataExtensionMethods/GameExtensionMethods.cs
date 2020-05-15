/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

using Octgn.Core.DataManagers;
using Octgn.DataNew;
using Octgn.DataNew.Entities;
using Octgn.Library;
using Octgn.Library.Exceptions;
using Octgn.Library.Localization;
using Octgn.ProxyGenerator;
namespace Octgn.Core.DataExtensionMethods
{
    public static class GameExtensionMethods
    {
        internal static IFileSystem IO
        {
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

        private static PropertyDef _nameProperty;
        
        public static PropertyDef NameProperty
        {
            get
            {
                if (_nameProperty == null)
                    _nameProperty = new PropertyDef()
                    {
                        Name = "Name",
                        Type = PropertyType.String
                    };
                return _nameProperty;
            }
        }

        public static IEnumerable<Set> Sets(this Game game)
        {
            var ret = SetManager.Get().GetByGameId(game.Id);
            return ret;
        }

        /// <summary>
        /// Not Implemented
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static Game Install(this Game game)
        {
            throw new NotImplementedException();
        }

        public static Game UpdateGameHash(this Game game, string hash)
        {
            game = GameManager.Get().GetById(game.Id);
            game.FileHash = hash;
            DbContext.Get().Save(game);
            return game;
        }

        public static Deck CreateDeck(this Game game)
        {
            var deck = new Deck { GameId = game.Id };
            deck.Sections = game.DeckSections.Select(x => new Section { Name = x.Value.Name.Clone() as string, Cards = new List<IMultiCard>(), Shared = x.Value.Shared }).ToList();
            deck.Sections = deck.Sections.Concat(game.SharedDeckSections.Select(x => new Section { Name = x.Value.Name.Clone() as string, Cards = new List<IMultiCard>(), Shared = x.Value.Shared })).ToList();
            return deck;
        }

        public static IEnumerable<GameScript> GetScripts(this Game game)
        {
            return DbContext.Get().Scripts.Where(x => x.GameId == game.Id);
        }

        public static string GetDefaultDeckPath(this Game game)
        {
            var path = Config.Instance.Paths.DeckPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public static Card GetCardByName(this Game game, string name)
        {
            return game.Sets().SelectMany(x => x.Cards).FirstOrDefault(y => y.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        public static Card GetCardById(this Game game, Guid id)
        {
            return game.Sets().SelectMany(x => x.Cards).FirstOrDefault(y => y.Id == id);
        }

        public static Set GetSetById(this Game game, Guid id)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return null;
            return g.Sets().FirstOrDefault(x => x.Id == id);
        }
        public static Pack GetPackById(this Game game, Guid id)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return null;
            return g.Sets().SelectMany(x => x.Packs).FirstOrDefault(x => x.Id == id);
        }

        public static IEnumerable<PropertyDef> AllProperties(this Game game)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return new List<PropertyDef>();
            return Enumerable.Repeat(NameProperty, 1).Union(game.CustomProperties);
        }

        public static IEnumerable<Card> AllCards(this Game game)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return new List<Card>();
            return g.Sets().SelectMany(x => x.Cards);
        }

        public static IEnumerable<Card> AllCards(this Game game, bool hidden)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return new List<Card>();
            return g.Sets().Where(x => x.Hidden == false).SelectMany(x => x.Cards);
        }

        public static DataTable ToDataTable(this IEnumerable<Card> cards, Game game)
        {
            DataTable table = new DataTable();

            var values = new object[game.CustomProperties.Count + 6];
            var defaultValues = new object[game.CustomProperties.Count + 6];
            var indexes = new Dictionary<int, string>();
            var setCache = new Dictionary<Guid, string>();
            var i = 6;
            table.Columns.Add("Name", typeof(string));
            defaultValues[0] = "";
            table.Columns.Add("SetName", typeof(string));
            defaultValues[1] = "";
            table.Columns.Add("set_id", typeof(string));
            defaultValues[2] = "";
            table.Columns.Add("img_uri", typeof(string));
            defaultValues[3] = "";
            table.Columns.Add("id", typeof(string));
            defaultValues[4] = "";
            table.Columns.Add("Alternates", typeof(string));
            defaultValues[5] = "";
            foreach (var prop in game.CustomProperties)
            {
                switch (prop.Type)
                {
                    case PropertyType.String:
                        table.Columns.Add(prop.Name, typeof(string));
                        defaultValues[i] = "";
                        break;
                    case PropertyType.RichText:
                        table.Columns.Add(prop.Name, typeof(RichTextPropertyValue));
                        defaultValues[i] = new RichTextPropertyValue() { Value = new RichSpan() };
                        break;
                    case PropertyType.Integer:
                        table.Columns.Add(prop.Name, typeof(double));
                        defaultValues[i] = null;
                        break;
                    case PropertyType.GUID:
                        table.Columns.Add(prop.Name, typeof(Guid));
                        defaultValues[i] = Guid.Empty;
                        break;
                    case PropertyType.Char:
                        table.Columns.Add(prop.Name, typeof(char));
                        defaultValues[i] = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                indexes.Add(i, prop.Name);
                i++;
            }

            foreach (Card item in cards)
            {
                for (i = 6; i < values.Length; i++)
                {
                    values[i] = defaultValues[i];
                }
                if (!setCache.ContainsKey(item.SetId))
                    setCache.Add(item.SetId, item.GetSet().Name);
                values[1] = setCache[item.SetId];
                values[2] = item.SetId;
                values[3] = item.GetImageUri();
                values[4] = item.Id;
                foreach (CardPropertySet alt in item.PropertySets.Values)
                {
                    values[5] = alt.Type;
                    values[0] = alt.Name;
                    foreach (var prop in alt.Properties)
                    {
                        var ix = indexes.Where(x => x.Value == prop.Key.Name).Select(x => new { x.Key, x.Value }).FirstOrDefault();
                        if (ix == null)
                            throw new UserMessageException(L.D.Exception__CanNotCreateDeckMissingCardProperty);
                        if (prop.Key.Type == PropertyType.Integer)
                        {
                            if (prop.Value == null || !int.TryParse(prop.Value as string, out _))
                            {
                                values[ix.Key] = null;
                                continue;
                            }
                        }
                        values[ix.Key] = prop.Value;
                    }
                    table.Rows.Add(values);

                }
            }
            return table;
        }

        public static ProxyDefinition GetCardProxyDef(this Game game)
        {
            var retdef = DbContext.Get().ProxyDefinitions.FirstOrDefault(x => (Guid)x.Key == game.Id);
            return retdef;
        }

        /// <summary>
        /// gets the default CardSize property defined by this game.
        /// </summary>
        public static CardSize DefaultSize(this Game game)
        {
            return game.CardSizes[""];
        }
    }
}