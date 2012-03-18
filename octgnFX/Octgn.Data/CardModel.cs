using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO.Packaging;
using System.Linq;
using System.Xml;

namespace Octgn.Data
{
    public class CardModel
    {
        /*
         *  The CardModel holds all the information the Set Definition gives to us. Access to the model itself
         *  I would expect to be off limits to the game def.
         * 
         * */

        public SortedList<string, object> Properties;
        public Set Set;

        public CardModel()
        {
        }
        /// <summary>
        /// This constructor parses the XML in reader. Used when reading a card from XML, mainly when installing a set.
        /// </summary>
        /// <param name="reader">The XML</param>
        /// <param name="game"></param>
        /// <param name="set"></param>
        /// <param name="definition"></param>
        /// <param name="package"></param>
        public CardModel(XmlReader reader, Game game, Set set, PackagePart definition, Package package)
        {
            Properties = new SortedList<string, object>(game.CustomProperties.Count,
                                                        StringComparer.InvariantCultureIgnoreCase);
            reader.MoveToAttribute("name");
            Name = reader.Value;
            reader.MoveToAttribute("id");
            Id = new Guid(reader.Value);
 //           isMutable = false;
            if ( reader.MoveToAttribute ("alternate") )
            {
                try { Alternate = new Guid(reader.Value); }
                catch (Exception e) 
                {
                    throw new ArgumentException(String.Format("The value {0} is not of expected type for property Alternate. Alternate must be a GUID.",
                                                reader.Value));
                }
            }
            else { Alternate = System.Guid.Empty; }
            if (reader.MoveToAttribute("dependent")) 
            {
                try 
                {
                     Dependent = new Guid(reader.Value).ToString(); 
                }
                catch (Exception e)
                {
                    try
                    {
                        Dependent = Boolean.Parse(reader.Value).ToString();
                    }
                    catch (Exception eff)
                    {

                         throw new ArgumentException(String.Format("The value {0} is not of expected type for property Dependent. Dependent must be either true/false, or the Guid of the card to use instead.",
                                                       reader.Value));
                    }
                }
            }
            else { Dependent = String.Empty; }
            Uri cardImageUri = definition.GetRelationship("C" + Id.ToString("N")).TargetUri;
            ImageUri = cardImageUri.OriginalString;
            if (!package.PartExists(cardImageUri))
                throw new Exception(string.Format("Image for card '{0}', with URI '{1}' was not found in the package.",
                                                  Name, ImageUri));
            reader.Read(); // <card>

            Set = set;

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
                            Properties.Add(prop.Name, reader.Value);
                            break;
                        case PropertyType.Integer:
                            Properties.Add(prop.Name, Int32.Parse(reader.Value));
                            break;
                        case PropertyType.Char:
                            Properties.Add(prop.Name, Char.Parse(reader.Value));
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

            reader.Read(); // </card>
        }

        public CardModel(Guid guid)
        {//This constructor will generate a new CardModel by accessing the database and searching for the appropriate GUID
            
        }

        public Guid Id { get; internal set; }

        public string Name { get; internal set; }

        public string ImageUri { get; internal set; }

        public Guid Alternate { get; internal set; }//The location of the alternate. If none is specified, will be System.Guid.Empty
        public string Dependent { get; internal set; }//If not Guid.Empty.ToString(), this card will not be placed inside a deck - 
        //The card with guid == dependent will be used instead. Mainly used in Deck Editor<TODO>
        public bool isMutable { get; internal set; }//a flag; if true, this card is read-only. (and will only be instanced once<TODO>)

        public string Picture
        {
            get { return Set.GetPackUri() + ImageUri; }
        }

        public string AlternatePicture
        {
            get
            {
                string alternateUri = ImageUri.Replace(".jpg", ".b.jpg");
                return Set.GetPackUri() + alternateUri;
            }
        }

        public static Uri GetPictureUri(Game game, Guid setId, string imageUri)
        {
            return new Uri(game.GetSet(setId).GetPackUri() + imageUri);
        }

        public CardModel Clone()
        {
            return new CardModel
                       {
                           Id = Id,
                           Name = Name,
                           ImageUri = ImageUri,
                           Properties = Properties,
                           Set = Set,
                           Alternate = Alternate,
                           Dependent = Dependent
                       };
        }

        internal static CardModel FromDataRow(Game game, DataRow row)
        {
            DataColumnCollection columns = row.Table.Columns;
            Guid alternateGuid = Guid.Empty;
            bool parseable = Guid.TryParse(row["alternate"].ToString(), out alternateGuid);
            if (!parseable)
            {
                alternateGuid = Guid.Empty;
            }
            var result = new CardModel
                             {
                                 Id = Guid.Parse(row["id"].ToString()),
                                 Name = (string) row["name"],
                                 ImageUri = (string) row["image"],
                                 Set = game.GetSet(Guid.Parse(row["set_id"].ToString())),
                                 Alternate = alternateGuid,
                                 Dependent = (string) row["Dependent"],
                                 Properties =
                                     new SortedList<string, object>(columns.Count - 4,
                                                                    StringComparer.InvariantCultureIgnoreCase)
                             };
            for (int i = 4; i < columns.Count; ++i)
                result.Properties.Add(columns[i].ColumnName, row.IsNull(i) ? null : row[i]);
            return result;
        }

        public bool hasProperty(string propertyName)
        {
            if (propertyName.Equals("Alternate", StringComparison.InvariantCultureIgnoreCase)) return (Alternate != null && Alternate != System.Guid.Empty);
            if (propertyName.Equals("Dependent", StringComparison.InvariantCultureIgnoreCase)) return (Dependent != null || Dependent.Equals(String.Empty));
            return Properties.ContainsKey(propertyName);
        }
    }

    public class CardPropertyComparer : IComparer, IComparer<CardModel>
    {
        private readonly bool _isName;
        private readonly string _propertyName;

        public CardPropertyComparer(string propertyName)
        {
            _propertyName = propertyName;
            _isName = propertyName == "Name";
        }

        #region IComparer Members

        int IComparer.Compare(object x, object y)
        {
            return Compare(x as CardModel, y as CardModel);
        }

        #endregion

        #region IComparer<CardModel> Members

        public int Compare(CardModel x, CardModel y)
        {
            if (_isName)
                return String.CompareOrdinal(x.Name, y.Name);

            object px = x.Properties[_propertyName];
            object py = y.Properties[_propertyName];
            if (px == null) return py == null ? 0 : -1;
            return ((IComparable) px).CompareTo(py);
        }

        #endregion
    }
}