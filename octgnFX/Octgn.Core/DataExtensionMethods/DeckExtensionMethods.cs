﻿namespace Octgn.Core.DataExtensionMethods
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
                    var writer = XmlWriter.Create(fs);
                    writer.WriteStartDocument(true);
                    writer.WriteStartElement("deck");
                    writer.WriteAttributeString("game",game.Id.ToString());
                    foreach (var section in deck.Sections)
                    {
                        writer.WriteStartElement("section");
                        writer.WriteAttributeString("name",section.Name);
                        foreach (var c in section.Cards)
                        {
                            writer.WriteStartElement("card");
                            writer.WriteAttributeString("qty",c.Quantity.ToString());
                            writer.WriteAttributeString("id",c.Id.ToString());
                            writer.WriteString(c.Name);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Flush();
                    writer.Close();
                }
            }
            catch (PathTooLongException)
            {
                throw new UserMessageException("Could not save deck to {0}, the file path would be too long.",path);
            }
            catch (IOException e)
            {
                Log.Error(String.Format("Problem saving deck to path {0}", path), e);
                throw new UserMessageException("Could not save deck to {0}, {1}", path,e.Message);
            }
            catch (Exception e)
            {
                Log.Error(String.Format("Problem saving deck to path {0}",path),e);
                throw new UserMessageException("Could not save deck to {0}, there was an unspecified problem.",path);
            }
        }
        public static IDeck Load(this IDeck deck, Game game, string path)
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
                        foreach (var cardelem in sectionelem.Descendants("card"))
                        {
                            var cardId = Guid.Parse(cardelem.Attribute("id").Value);
                            var cardq = Int32.Parse(cardelem.Attribute("qty").Value);
                            var card = cards.FirstOrDefault(x => x.Id == cardId);
                            if (card == null)
                            {
                                var cardN = cardelem.Value;
                                card = cards.FirstOrDefault(x => x.Name.Equals(cardN, StringComparison.CurrentCultureIgnoreCase));
                                if(card == null)
                                    throw new UserMessageException(
                                        "Problem loading deck {0}. The card with id: {1} and name: {2} is not installed.", path, cardId, cardN);
                            }
                            (section.Cards as IList<IMultiCard>).Add(card.ToMultiCard(cardq));
                        }
                        (ret.Sections as List<ISection>).Add(section);
                    }
                    foreach (var section in shared ? game.SharedDeckSections : game.DeckSections)
                    {
                        if (ret.Sections.Any(x => x.Name == section.Value.Name) == false)
                            (ret.Sections as List<ISection>).Add(
                                new Section { Name = section.Value.Name, Cards = new List<IMultiCard>() });
                    }
                    ret.GameId = gameId;
                    ret.IsShared = shared;
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
                throw new UserMessageException("The deck {0} is corrupt.",path);
            }
            catch (NullReferenceException e)
            {
                Log.Error(String.Format("Problem loading deck from path {0}", path), e);
                throw new UserMessageException("The deck {0} is corrupt.",path);
            }
            catch (XmlException e)
            {
                Log.Error(String.Format("Problem loading deck from path {0}", path), e);
                throw new UserMessageException("The deck {0} is corrupt.",path);
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
                              Alternate = card.Alternate,
                              SetId = card.SetId,
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
        public static IEnumerable<IMultiCard> AddCard(this IEnumerable<IMultiCard> cards, IMultiCard card)
        {
            if (cards is ObservableCollection<ObservableMultiCard>)
            {
                if(card is ObservableMultiCard)
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
    }
}
