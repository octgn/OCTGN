namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Xml.Linq;

    using Octgn.Core.DataManagers;
    using Octgn.DataNew;
    using Octgn.DataNew.Entities;
    using Octgn.Library;
    using Octgn.Library.Exceptions;

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
            return SetManager.Get().Sets.Where(x => x.GameId == game.Id);
        }

        public static Game Install(this Game game)
        {
            DbContext.Get().Save(game);
            return game;
        }

        public static Game UpdateGameHash(this Game game, string hash)
        {
            game = GameManager.Get().GetById(game.Id);
            game.FileHash = hash;
            DbContext.Get().Save(game);
            return game;
        }

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
            deck.Sections = game.DeckSections.Select(x=> new Section{Name=x}).ToList();
            return deck;
        }

        public static string GetInstallPath(this Game game)
        {
            return IO.Path.Combine(IO.Path.Combine(Paths.DataDirectory, "Games"), game.Id.ToString());
        }

        public static Uri GetCardBackUri(this Game game)
        {
            //var path = IO.Path.Combine(game.GetInstallPath(), game.CardBack);
            var ret = new Uri(game.CardBack);
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
            return g.Sets().SelectMany(x=> x.Cards.Cards).FirstOrDefault(x => x.Name == name);
        }

        public static Card GetCardById(this Game game, Guid id)
        {
            var g = GameManager.Get().GetById(game.Id);
            if (g == null) return null;
            return g.Sets().SelectMany(x => x.Cards.Cards).FirstOrDefault(x => x.Id == id);
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
            return g.Sets().SelectMany(x => x.Cards.Cards);
        }

        public static DataTable ToDataTable(this IEnumerable<Card> cards)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(Card));
            DataTable table = new DataTable();
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                table.Columns.Add(prop.Name, prop.PropertyType);
            }
            object[] values = new object[props.Count];
            foreach (Card item in cards)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = props[i].GetValue(item);
                }
                table.Rows.Add(values);
            }
            return table;   
        }

        public static Deck LoadDeck(this Game game, string filename)
        {
            if (game == null) throw new ArgumentNullException("game");

            XDocument doc;
            Guid gameId = new Guid();
            try
            {
                doc = XDocument.Load(filename);
            }
            catch (Exception e)
            {
                throw new FileNotReadableException(e);
            }

            if (doc.Root != null)
            {
                XAttribute gameAttribute = doc.Root.Attribute("game");
                if (gameAttribute == null)
                    throw new InvalidFileFormatException("The <deck> tag is missing the 'game' attribute");

                try
                {
                    gameId = new Guid(gameAttribute.Value);
                }
                catch
                {
                    throw new InvalidFileFormatException("The game attribute is not a valid GUID");
                }
            }

            if (gameId != game.Id) throw new WrongGameException(gameId, game.Id.ToString());

            Deck deck;
            try
            {
                var isShared = doc.Root.Attr<bool>("shared");
                IEnumerable<string> defSections = isShared ? game.SharedDeckSections : game.DeckSections;

                deck = new Deck { GameId = game.Id, IsShared = isShared };
                if (doc.Root != null)
                {
                    IEnumerable<Section> sections = from section in doc.Root.Elements("section")
                                                    let xAttribute = section.Attribute("name")
                                                    where xAttribute != null
                                                    select new Section()
                                                    {
                                                        Name = xAttribute.Value,
                                                        Cards = new ObservableCollection<MultiCard>
                                                            (from card in section.Elements("card")
                                                             select new MultiCard
                                                             {
                                                                 Id = new Guid(card.Attr<string>("id")),
                                                                 Name = card.Value,
                                                                 Quantity =card.Attr<byte>("qty", 1)
                                                             })
                                                    };
                    var allSections = new Section[defSections.Count()];
                    int i = 0;
                    foreach (string sectionName in defSections)
                    {
                        allSections[i] = sections.FirstOrDefault(x => x.Name == sectionName);
                        if (allSections[i] == null) allSections[i] = new Section { Name = sectionName };
                        ++i;
                    }
                    deck.Sections = allSections;
                }
            }
            catch
            {
                throw new InvalidFileFormatException();
            }
            // Matches with actual cards in database

            foreach (var sec in deck.Sections)
            {
                var newList = (from e in sec.Cards let card = game.GetCardById(e.Id) select card.ToMultiCard(e.Quantity)).ToList();
                sec.Cards = newList;
            }

            return deck;
        }

        public static void DeleteSet(this Game game, Set set)
        {
            SetManager.Get().UninstallSet(set);
        }
    }
}