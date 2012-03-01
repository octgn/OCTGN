using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;

namespace Octgn.Data
{
    public class Deck : INotifyPropertyChanged
    {
        private Section[] _sections;

        internal Deck()
        {
        }

        public Deck(Game game)
        {
            GameId = game.Id;
            Sections = game.DeckSections.Select(s => new Section {Name = s}).ToArray();
        }

        public Guid GameId { get; internal set; }

        public bool IsShared { get; internal set; }

        public Section[] Sections
        {
            get { return _sections; }
            internal set
            {
                _sections = value;
                foreach (Section s in value)
                    s.PropertyChanged +=
                        delegate(object sender, PropertyChangedEventArgs e) { if (e.PropertyName == "CardCount") OnPropertyChanged("CardCount"); };
            }
        }

        public int CardCount
        {
            get { return Sections.Sum(sec => sec.CardCount); }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void Save(string file)
        {
            var doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("deck",
                             new XAttribute("game", GameId),
                             from section in Sections
                             select new XElement("section",
                                                 new XAttribute("name", section.Name),
                                                 from card in section.Cards
                                                 select new XElement("card",
                                                                     new XAttribute("qty", card.Quantity),
                                                                     new XAttribute("id", card.Card.Id),
                                                                     card.Card.Name))));//Card.Name is unnecessary information. Vote to remove (V)_V
            doc.Save(file);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Load

        public static Deck Load(string file, GamesRepository repository)
        {
            if (repository == null) throw new ArgumentNullException("repository");

            Guid gameId;
            XDocument doc = LoadDoc(file, out gameId);

            Game game = repository.Games.FirstOrDefault(g => g.Id == gameId);
            if (game == null) throw new UnknownGameException(gameId);

            return LoadCore(doc, game);
        }

        public static Deck Load(string file, Game game)
        {
            if (game == null) throw new ArgumentNullException("game");

            Guid gameId;
            XDocument doc = LoadDoc(file, out gameId);
            if (gameId != game.Id) throw new WrongGameException(gameId, game.Id.ToString());

            return LoadCore(doc, game);
        }

        private static XDocument LoadDoc(string file, out Guid gameId)
        {
            XDocument doc;
            gameId = new Guid();
            try
            {
                doc = XDocument.Load(file);
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

            return doc;
        }

        private static Deck LoadCore(XDocument doc, Game game)
        {
            Deck deck;
            try
            {
                var isShared = doc.Root.Attr<bool>("shared");
                IEnumerable<string> defSections = isShared ? game.SharedDeckSections : game.DeckSections;

                deck = new Deck {GameId = game.Id, IsShared = isShared};
                if (doc.Root != null)
                {
                    IEnumerable<Section> sections = from section in doc.Root.Elements("section")
                                                    let xAttribute = section.Attribute("name")
                                                    where xAttribute != null
                                                    select new Section(false)
                                                               {
                                                                   Name = xAttribute.Value,
                                                                   Cards = new ObservableCollection<Element>
                                                                       (from card in section.Elements("card")
                                                                        select new Element
                                                                                   {
                                                                                       LoadedId =
                                                                                           card.Attr<string>("id"),
                                                                                       LoadedName = card.Value,
                                                                                       Quantity =
                                                                                           card.Attr<byte>("qty", 1)
                                                                                   })
                                                               };
                    var allSections = new Section[defSections.Count()];
                    int i = 0;
                    foreach (string sectionName in defSections)
                    {
                        allSections[i] = sections.FirstOrDefault(x => x.Name == sectionName);
                        if (allSections[i] == null) allSections[i] = new Section {Name = sectionName};
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
            foreach (Element e in deck.Sections.SelectMany(s => s.Cards))
                try
                {
                    // First try by id, if one is provided
                    if (e.LoadedId != null) e.Card = game.GetCardById(new Guid(e.LoadedId));
                    // If there's no id, or if it doesn't match a card in the database, try to fallback on the name
                    if (e.Card == null) e.Card = game.GetCardByName(e.LoadedName);
                    // If we still can't find the card, report an error
                    if (e.Card == null)
                        throw new UnknownCardException(e.LoadedId, e.LoadedName);
                }
                catch (FormatException)
                {
                    throw new InvalidFileFormatException(string.Format("Could not parse card id {0}.",
                                                                       e.LoadedId));
                }
            return deck;
        }

        #endregion

        #region Nested type: Element

        public class Element : INotifyPropertyChanged
        {
            internal string LoadedId, LoadedName;
            private CardModel _card;
            private byte _quantity = 1;

            public CardModel Card
            {
                get { return _card; }
                set
                {
                    if (_card == value) return;
                    _card = value;
                    OnPropertyChanged("Card");
                }
            }

            public byte Quantity
            {
                get { return _quantity; }
                set
                {
                    if (_quantity == value) return;
                    _quantity = value;
                    OnPropertyChanged("Quantity");
                }
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            protected void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region Nested type: Section

        public class Section : INotifyPropertyChanged
        {
            private ObservableCollection<Element> _cards;

            public Section()
                : this(true)
            {
            }

            internal Section(bool createEmptyList)
            {
                if (createEmptyList) Cards = new ObservableCollection<Element>();
            }

            public string Name { get; internal set; }

            public ObservableCollection<Element> Cards
            {
                get { return _cards; }
                internal set
                {
                    _cards = value;
                    foreach (INotifyPropertyChanged item in value)
                        item.PropertyChanged += ElementChanged;
                    value.CollectionChanged += delegate(object sender, NotifyCollectionChangedEventArgs e)
                                                   {
                                                       if (e.OldItems != null)
                                                           foreach (INotifyPropertyChanged item in e.OldItems)
                                                               item.PropertyChanged -= ElementChanged;
                                                       if (e.NewItems != null)
                                                           foreach (INotifyPropertyChanged item in e.NewItems)
                                                               item.PropertyChanged += ElementChanged;
                                                       OnPropertyChanged("CardCount");
                                                   };
                }
            }

            public int CardCount
            {
                get { return Cards.Sum(el => el.Quantity); }
            }

            #region INotifyPropertyChanged Members

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            private void ElementChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName != "Quantity") return;
                var element = sender as Element;
                if (element != null && element.Quantity <= 0) Cards.Remove(element);
                OnPropertyChanged("CardCount");
            }

            protected void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}