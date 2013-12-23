namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;

    using Octgn.DataNew;
    using Octgn.DataNew.Entities;
    using Octgn.Library.Exceptions;

    using log4net;

    public static class DeckExtensionMethods
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static void Save(this IDeck deck, Game game, string path)
        {
            try
            {
                using (var fs = File.Open(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    var settings = new XmlWriterSettings();
                    settings.Indent = true;
                    settings.NewLineHandling = NewLineHandling.Entitize;

                    var writer = XmlWriter.Create(fs, settings);

                    writer.WriteStartDocument(true);
                    writer.WriteStartElement("deck");
                    writer.WriteAttributeString("game", game.Id.ToString());
                    foreach (var section in deck.Sections)
                    {
                        writer.WriteStartElement("section");
                        writer.WriteAttributeString("name", section.Name);
                        writer.WriteAttributeString("shared", section.Shared.ToString());
                        foreach (var c in section.Cards)
                        {
                            writer.WriteStartElement("card");
                            writer.WriteAttributeString("qty", c.Quantity.ToString());
                            writer.WriteAttributeString("id", c.Id.ToString());
                            writer.WriteString(c.Name);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteStartElement("notes");
                    writer.WriteCData(deck.Notes);
                    writer.WriteEndElement();
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (PathTooLongException)
            {
                throw new UserMessageException("Could not save deck to {0}, the file path would be too long.", path);
            }
            catch (IOException e)
            {
                Log.Error(String.Format("Problem saving deck to path {0}", path), e);
                throw new UserMessageException("Could not save deck to {0}, {1}", path, e.Message);
            }
            catch (Exception e)
            {
                Log.Error(String.Format("Problem saving deck to path {0}", path), e);
                throw new UserMessageException("Could not save deck to {0}, there was an unspecified problem.", path);
            }
        }

        public static IDeck Load(this IDeck deck, string path, bool cloneCards = true)
        {
            var ret = new Deck();
            try
            {
                Game game = null;
                using (var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    var doc = XDocument.Load(fs);
                    var gameId = Guid.Parse(doc.Descendants("deck").First().Attribute("game").Value);
                    game = Octgn.Core.DataManagers.GameManager.Get().GetById(gameId);
                    if (game == null)
                    {
                        throw new UserMessageException("Could not load deck from {0}, you do not have the associated game installed.", path);
                    }
                }
                return deck.Load(game, path, cloneCards);
            }
            catch (UserMessageException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(String.Format("Problem loading deck from path {0}", path), e);
                throw new UserMessageException("Could not load deck from {0}, there was an unspecified problem.", path);
            }
            return null;
        }
        public static IDeck Load(this IDeck deck, Game game, string path, bool cloneCards = true)
        {
            var ret = new Deck();
            ret.Sections = new List<ISection>();
            try
            {
                var cards = game.Sets().SelectMany(x => x.Cards).ToArray();
                using (var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    var doc = XDocument.Load(fs);
                    var gameId = Guid.Parse(doc.Descendants("deck").First().Attribute("game").Value);
                    var shared = doc.Descendants("deck").First().Attr<bool>("shared");
                    foreach (var sectionelem in doc.Descendants("section"))
                    {
                        var section = new Section();
                        section.Cards = new List<IMultiCard>();
                        section.Name = sectionelem.Attribute("name").Value;
                        section.Shared = sectionelem.Attr<bool>("shared");
                        // On old style decks, if it's shared, then all subsequent sections are shared
                        if (shared)
                            section.Shared = true;
                        foreach (var cardelem in sectionelem.Descendants("card"))
                        {
                            var cardId = Guid.Parse(cardelem.Attribute("id").Value);
                            var cardq = Int32.Parse(cardelem.Attribute("qty").Value);
                            var card = cards.FirstOrDefault(x => x.Id == cardId);
                            if (card == null)
                            {
                                var cardN = cardelem.Value;
                                card = cards.FirstOrDefault(x => x.Name.Equals(cardN, StringComparison.CurrentCultureIgnoreCase));
                                if (card == null)
                                    throw new UserMessageException(
                                        "Problem loading deck {0}. The card with id: {1} and name: {2} is not installed.", path, cardId, cardN);
                            }
                            (section.Cards as IList<IMultiCard>).Add(card.ToMultiCard(cardq, cloneCards));
                        }
                        if (section.Cards.Any())
                            (ret.Sections as List<ISection>).Add(section);
                    }
                    // Add deck notes
                    var notesElem = doc.Descendants("notes").FirstOrDefault();
                    if (notesElem != null)
                    {
                        var cd = (notesElem.FirstNode as XCData);
                        if (cd != null)
                        {
                            ret.Notes = cd.Value.Clone() as string;
                        }
                    }
                    if (ret.Notes == null) ret.Notes = "";

                    // Add all missing sections so that the game doesn't get pissed off
                    {
                        var combinedList =
                            game.DeckSections.Select(x => x.Value).Concat(game.SharedDeckSections.Select(y => y.Value));
                        foreach (var section in combinedList)
                        {
                            if (ret.Sections.Any(x => x.Name.Equals(section.Name, StringComparison.InvariantCultureIgnoreCase) && x.Shared == section.Shared) == false)
                            {
                                // Section not defined in the deck, so add an empty version of it.
                                (ret.Sections as List<ISection>).Add(
                                    new Section
                                    {
                                        Name = section.Name.Clone() as string,
                                        Cards = new List<IMultiCard>(),
                                        Shared = section.Shared
                                    });
                            }
                        }
                    }
                    ret.GameId = gameId;
                    ret.IsShared = shared;
                }
                // This is an old style shared deck file, we need to convert it now, for posterity sake.
                if (ret.IsShared)
                {
                    ret.IsShared = false;
                    ret.Save(game, path);
                }
                deck = ret;
                return deck;
            }
            catch (UserMessageException)
            {
                throw;
            }
            catch (FormatException e)
            {
                Log.Error(String.Format("Problem loading deck from path {0}", path), e);
                throw new UserMessageException("The deck {0} is corrupt.", path);
            }
            catch (NullReferenceException e)
            {
                Log.Error(String.Format("Problem loading deck from path {0}", path), e);
                throw new UserMessageException("The deck {0} is corrupt.", path);
            }
            catch (XmlException e)
            {
                Log.Error(String.Format("Problem loading deck from path {0}", path), e);
                throw new UserMessageException("The deck {0} is corrupt.", path);
            }
            catch (FileNotFoundException)
            {
                throw new UserMessageException("Could not save deck to {0}, could not file the file.", path);
            }
            catch (IOException e)
            {
                Log.Error(String.Format("Problem loading deck from path {0}", path), e);
                throw new UserMessageException("Could not load deck from {0}, {1}", path, e.Message);
            }
            catch (Exception e)
            {
                Log.Error(String.Format("Problem loading deck from path {0}", path), e);
                throw new UserMessageException("Could not load deck from {0}, there was an unspecified problem.", path);
            }
            return ret;
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
            ret.Name = section.Name.Clone() as string;
            ret.Cards = section.Cards.ToArray();
            ret.Shared = section.Shared;
            return ret;
        }
        public static ObservableMultiCard AsObservable(this IMultiCard card)
        {
            if (card == null) return null;
            var ret = new ObservableMultiCard
                          {
                              Id = card.Id,
                              Name = card.Name.Clone() as string,
                              Properties = card.Properties.ToDictionary(x => x.Key, y => y.Value),
                              ImageUri = card.ImageUri.Clone() as string,
                              Alternate = card.Alternate.Clone() as string,
                              SetId = card.SetId,
                              Quantity = card.Quantity
                          };
            return ret;
        }
        public static ObservableDeck AsObservable(this IDeck deck)
        {
            if (deck == null) return null;
            var ret = new ObservableDeck();
            ret.GameId = deck.GameId;
            ret.IsShared = deck.IsShared;
            if (deck.Sections == null) ret.Sections = new List<ObservableSection>();
            else
            {
                ret.Sections = deck.Sections
                    .Where(x => x != null)
                    .Select(
                        x =>
                        {
                            var sret = new ObservableSection();
                            sret.Name = (x.Name ?? "").Clone() as string;
							if(x.Cards == null)
								sret.Cards = new List<ObservableMultiCard>();
							else
								sret.Cards = x.Cards.Where(y=> y != null).Select(y => y.AsObservable()).ToArray();
                            sret.Shared = x.Shared;
                            return sret;
                        });
            }
            ret.Notes = (deck.Notes ?? "").Clone() as string;
            return ret;
        }
        public static IEnumerable<IMultiCard> AddCard(this IEnumerable<IMultiCard> cards, IMultiCard card)
        {
            if (cards is ObservableCollection<ObservableMultiCard>)
            {
                if (card is ObservableMultiCard)
                    (cards as ObservableCollection<ObservableMultiCard>).Add(card as ObservableMultiCard);
                else
                    (cards as ObservableCollection<ObservableMultiCard>).Add(card.AsObservable());
            }
            else if (cards is ObservableCollection<IMultiCard>) (cards as ObservableCollection<IMultiCard>).Add(card);
            else if (cards is IList<IMultiCard>) (cards as IList<IMultiCard>).Add(card);
            else if (cards is ICollection<IMultiCard>) (cards as ICollection<IMultiCard>).Add(card);
            else
            {
                var g = cards.ToList();
                g.Add(card);
                cards = g;
            }
            return cards;
        }
        public static IEnumerable<IMultiCard> RemoveCard(this IEnumerable<IMultiCard> cards, IMultiCard card)
        {
            if (cards is ObservableCollection<ObservableMultiCard>)
            {
                if (card is ObservableMultiCard) (cards as ObservableCollection<ObservableMultiCard>).Remove(card as ObservableMultiCard);
                else
                {
                    var tcard = (cards as ObservableCollection<ObservableMultiCard>).FirstOrDefault(
                        x => x.Id == card.Id);
                    if (tcard == null) return cards;
                    (cards as ObservableCollection<ObservableMultiCard>).Remove(tcard);
                }
            }
            else if (cards is ObservableCollection<IMultiCard>)
                (cards as ObservableCollection<IMultiCard>).Remove(card);
            else if (cards is IList<IMultiCard>)
                (cards as IList<IMultiCard>).Remove(card);
            else if (cards is ICollection<IMultiCard>)
                (cards as ICollection<IMultiCard>).Remove(card);
            else
            {
                var g = cards.ToList();
                g.Remove(card);
                cards = g;
            }
            return cards;
        }
        public static IEnumerable<IMultiCard> Move(this IEnumerable<IMultiCard> cards, IMultiCard card, int newIndex)
        {
            if (cards is ObservableCollection<ObservableMultiCard>)
            {
                if (card is ObservableMultiCard) (cards as ObservableCollection<ObservableMultiCard>).Move((cards as ObservableCollection<ObservableMultiCard>).IndexOf(card as ObservableMultiCard), newIndex);
                else
                {
                    var tcard = (cards as ObservableCollection<ObservableMultiCard>).FirstOrDefault(
                        x => x.Id == card.Id);
                    if (tcard == null) return cards;
                    (cards as ObservableCollection<ObservableMultiCard>).Move(tcard, newIndex);
                }
            }
            else if (cards is ObservableCollection<IMultiCard>)
                (cards as ObservableCollection<IMultiCard>).Move(card, newIndex);
            else if (cards is IList<IMultiCard>)
                (cards as IList<IMultiCard>).Move(card, newIndex);
            else if (cards is ICollection<IMultiCard>)
                (cards as ICollection<IMultiCard>).Move(card, newIndex);
            else
            {
                var g = cards.ToList();
                g.Move(card, newIndex);
                cards = g;
            }
            return cards;
        }
    }
}
