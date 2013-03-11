namespace Octgn.Core.DataManagers
{
    using System;
    using System.Collections.Generic;
    using System.IO.Packaging;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.DataNew.Entities;

    using log4net;

    public class CardManager
    {
        #region Singleton
        private static CardManager Context { get; set; }
        private static object locker = new object();
        public static CardManager Get()
        {
            lock (locker) return Context ?? (Context = new CardManager());
        }
        internal CardManager()
        {

        }
        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        public Card GetCardById(Guid id)
        {
            var set = SetManager.Get().Sets.FirstOrDefault(x => x.Cards.Cards.Any(y => y.Id == id));
            if (set == null) return null;
            return set.Cards.Cards.FirstOrDefault(x => x.Id == id);
        }

        public Card FromXmlReader(XmlReader reader, Game game, Set set, PackagePart definition, Package package)
        {
            var ret = new Card();
            var Properties = new SortedList<PropertyDef, object>(game.CustomProperties.Count());
            reader.MoveToAttribute("name");
            ret.Name = reader.Value;
            reader.MoveToAttribute("id");
            ret.Id = new Guid(reader.Value);
            //           isMutable = false;
            if (reader.MoveToAttribute("alternate"))
            {
                try { ret.Alternate = new Guid(reader.Value); }
                catch (Exception e)
                {
                    throw new ArgumentException(String.Format("The value {0} is not of expected type for property Alternate. Alternate must be a GUID.",
                                                reader.Value));
                }
            }
            else { ret.Alternate = System.Guid.Empty; }
            if (reader.MoveToAttribute("dependent"))
            {
                try
                {
                    ret.Dependent = new Guid(reader.Value).ToString();
                }
                catch
                {
                    try
                    {
                        ret.Dependent = Boolean.Parse(reader.Value).ToString();
                    }
                    catch
                    {

                        throw new ArgumentException(String.Format("The value {0} is not of expected type for property Dependent. Dependent must be either true/false, or the Guid of the card to use instead.",
                                                      reader.Value));
                    }
                }
            }
            else { ret.Dependent = String.Empty; }
            Uri cardImageUri = definition.GetRelationship("C" + ret.Id.ToString("N")).TargetUri;
            ret.ImageUri = cardImageUri.OriginalString;
            if (!package.PartExists(cardImageUri))
                throw new Exception(string.Format("Image for card '{0}', with URI '{1}' was not found in the package.",
                                                  ret.Name, ret.ImageUri));
            reader.Read(); // <card>

            while (reader.IsStartElement("property"))
            {
                reader.MoveToAttribute("name");
                PropertyDef prop = game.CustomProperties.FirstOrDefault(p => p.Name == reader.Value);
                if (prop == null)
                    throw new ArgumentException(string.Format("The property '{0}' is unknown", reader.Value));
                reader.MoveToAttribute("value");
                try
                {
                    switch (prop.Type)
                    {
                        case PropertyType.String:
                            Properties.Add(prop, reader.Value);
                            break;
                        case PropertyType.Integer:
                            Properties.Add(prop, Int32.Parse(reader.Value));
                            break;
                        case PropertyType.Char:
                            Properties.Add(prop, Char.Parse(reader.Value));
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
                catch (FormatException)
                {
                    throw new ArgumentException(String.Format("The value {0} is not of expected type for property {1}",
                                                                reader.Value, prop.Name));
                }
                reader.Read(); // <property/>
            }
            ret.Properties = Properties;

            reader.Read(); // </card>
            return ret;
        }

    }
}