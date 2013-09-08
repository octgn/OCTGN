namespace Octgn.Core.DataExtensionMethods
{
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
    using Octgn.ProxyGenerator;

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

        public static IEnumerable<Set> Sets(this Game game)
        {
            var ret = SetManager.Get().GetByGameId(game.Id);
            return ret;
            //return SetManager.Get().Sets.Where(x => x.GameId == game.Id);
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
            deck.Sections = game.DeckSections.Select(x=> new Section{Name=x.Value.Name.Clone() as string,Cards = new List<IMultiCard>(),Shared = x.Value.Shared}).ToList();
            deck.Sections = deck.Sections.Concat(game.SharedDeckSections.Select(x=> new Section{Name=x.Value.Name.Clone() as string,Cards = new List<IMultiCard>(),Shared = x.Value.Shared})).ToList();
            return deck;
        }

        public static IEnumerable<GameScript> GetScripts(this Game game)
        {
            return DbContext.Get().Scripts.Where(x => x.GameId == game.Id);
        }

        public static Uri GetCardBackUri(this Game game)
        {
            var ret = new Uri(game.CardBack);
            return ret;
        }

        public static Uri GetCardFrontUri(this Game game)
        {
            var ret = new Uri(game.CardFront);
            return ret;
        }

        public static string GetDefaultDeckPath(this Game game)
        {
            var path = Paths.Get().DeckPath;
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }

        public static Card GetCardByName(this Game game, string name)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return null;
            return g.Sets().SelectMany(x=> x.Cards).FirstOrDefault(y =>y.Name == name);
        }

        public static Card GetCardById(this Game game, Guid id)
        {
            //var g = GameManager.Get().GetById(game.Id);
            //if (g == null) return null;
            return game.Sets().SelectMany(x => x.Cards).FirstOrDefault(y => y.Id == id);
        }

        public static Set GetSetById(this Game game, Guid id)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return null;
            return g.Sets().FirstOrDefault(x => x.Id == id);
        }

        public static IEnumerable<Marker> GetAllMarkers( this Game game)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return new List<Marker>();
            return g.Sets().SelectMany(x => x.Markers);
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
            return Enumerable.Repeat(new PropertyDef{Name="Name", Type = PropertyType.String}, 1).Union(game.CustomProperties);
        }

        public static IEnumerable<Card> AllCards(this Game game)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return new List<Card>();
            return g.Sets().SelectMany(x => x.Cards);
        }

        public static DataTable ToDataTable(this IEnumerable<Card> cards, Game game)
        {
            DataTable table = new DataTable();
            
            var values = new object[game.CustomProperties.Count + 5 - 1];
            var defaultValues = new object[game.CustomProperties.Count + 5 - 1];
            var indexes = new Dictionary<int, string>();
            var setCache = new Dictionary<Guid, string>();
            var i = 0 + 5;
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("SetName", typeof(string));
            table.Columns.Add("set_id", typeof(String));
            table.Columns.Add("img_uri", typeof(String));
            table.Columns.Add("id", typeof(string));
            defaultValues[0] = "";
            defaultValues[1] = "";
            defaultValues[2] = "";
            defaultValues[3] = "";
            defaultValues[4] = "";
            foreach (var prop in game.CustomProperties)
            {
                if (prop.Name == "Name") continue;
                switch (prop.Type)
                {
                    case PropertyType.String:
                        table.Columns.Add(prop.Name, typeof(string));
                        defaultValues[i] = "";
                        break;
                    case PropertyType.Integer:
                        table.Columns.Add(prop.Name, typeof(double));
                        defaultValues[i] = 0;
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
                for (i = 5; i < values.Length; i++)
                {
                    values[i] = defaultValues[i];
                }
                values[0] = item.Name;
                if(!setCache.ContainsKey(item.SetId))
                    setCache.Add(item.SetId,item.GetSet().Name);
                values[1] = setCache[item.SetId];
                values[2] = item.SetId;
                values[3] = item.ImageUri;
                values[4] = item.Id;
                foreach (var prop in item.PropertySet())
                {
                    if (prop.Key.Name == "Name") continue;
                    var ix = indexes.Where(x => x.Value == prop.Key.Name).Select(x=>new {Key=x.Key,Value=x.Value}).FirstOrDefault();
                    if(ix == null)
                        throw new UserMessageException("The game you are trying to make a deck for has a missing property on a card. Please contact the game developer and let them know.");
                    values[ix.Key] = prop.Value;
                }
                   
                table.Rows.Add(values);
            }
            return table;   
        }

        public static ProxyDefinition GetCardProxyDef(this Game game)
        {
            var retdef = DbContext.Get().ProxyDefinitions.FirstOrDefault(x => (Guid)x.Key == game.Id);
            return retdef;
        }
    }
}