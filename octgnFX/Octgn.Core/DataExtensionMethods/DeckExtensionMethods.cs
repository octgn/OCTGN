namespace Octgn.Core.DataExtensionMethods
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

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
        public static IDeck Load(this IDeck deck, string path)
        {
            //TODO [DB MIGRATION]  DO THIS!
            throw new NotImplementedException("OH GODS");
            return new Deck();
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
                              IsMutable = card.IsMutable,
                              Alternate = card.Alternate,
                              SetId = card.SetId,
                              Dependent = card.Dependent,
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
    }
}