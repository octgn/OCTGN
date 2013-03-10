namespace Octgn.Core.DataManagers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Serialization;

    using Octgn.DataNew;
    using Octgn.DataNew.Entities;
    using Octgn.Library;

    public class SetManager
    {
        #region Singleton
        private static SetManager Context { get; set; }
        private static object locker = new object();
        public static SetManager Get()
        {
            lock (locker) return Context ?? (Context = new SetManager());
        }
        internal SetManager()
        {

        }
        #endregion Singleton

        public IEnumerable<Set> Sets
        {
            get
            {
                return DbContext.Get().Sets;
            }
        } 

        public void InstallSet(string filename)
        {
            try
            {
                using (Package package = Package.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    PackageRelationship defRelationship = package.GetRelationshipsByType("http://schemas.octgn.org/set/definition").First();
                    PackagePart definition = package.GetPart(defRelationship.TargetUri);

                    var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, IgnoreWhitespace = true };
                    using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Paths),"Schema/CardSet.xsd"))
                        if (s != null)
                            using (XmlReader reader = XmlReader.Create(s))
                                settings.Schemas.Add(null, reader);

                    // Read the cards
                    using (XmlReader reader = XmlReader.Create(definition.GetStream(), settings))
                    {
                        reader.ReadToFollowing("set"); // <?xml ... ?>
                        var thisset = this.FromXmlReader(filename, reader);
                        var game = GameManager.Get().GetById(thisset.GameId);
                        if (thisset.GameVersion.Major != game.Version.Major || thisset.GameVersion.Minor != game.Version.Minor)
                            throw new ApplicationException(
                                string.Format(
                                    "The set '{0}' is incompatible with the installed game version.\nGame version: \nSet made for version: {1:2}.",
                                    thisset.Name, thisset.GameVersion));
                        

                        if (reader.IsStartElement("packaging"))
                        {
                            reader.ReadStartElement(); // <packaging>
                            while (reader.IsStartElement("pack"))
                            {
                                string xml = reader.ReadOuterXml();
                                var pack = PackManager.Get().FromXml(thisset, xml);
                                var temp = thisset.Packs.ToList();
                                temp.Add(pack);
                                thisset.Packs = temp;
                            }
                            reader.ReadEndElement(); // </packaging>
                        }

                        if (reader.IsStartElement("markers"))
                        {
                            reader.ReadStartElement(); // <markers>
                            while (reader.IsStartElement("marker"))
                            {
                                var marker = new Marker();
                                reader.MoveToAttribute("name");
                                marker.Name = reader.Value;
                                reader.MoveToAttribute("id");
                                marker.Id = new Guid(reader.Value);
                                Uri markerImageUri = definition.GetRelationship("M" + marker.Id.ToString("N")).TargetUri;
                                marker.IconUri = markerImageUri.OriginalString;
                                if (!package.PartExists(markerImageUri))
                                    throw new ApplicationException(
                                        string.Format(
                                            "Image for marker '{0}', with URI '{1}' was not found in the package.",
                                            marker.Name, marker.IconUri));
                                reader.Read(); // <marker />
                                var temp = thisset.Markers.ToList();
                                temp.Add(marker);
                                thisset.Markers = temp;
                            }
                            reader.ReadEndElement(); // </markers>
                        }

                        if (reader.IsStartElement("cards"))
                        {
                            var cardList = thisset.Cards.ToList();
                            reader.ReadStartElement(); // <cards>
                            while (reader.IsStartElement("card"))
                            {
                                cardList.Add(CardManager.Get().FromXmlReader(reader,game,thisset,definition,package));
                            }
                            thisset.Cards = cardList;
                            // cards are parsed through the CardModel constructor, which are then inserted individually into the database
                            reader.ReadEndElement(); // </cards>
                        }

                        reader.ReadEndElement();
                        DbContext.Get().Save(thisset);
                    }
                    string path = Path.Combine(Paths.DataDirectory, "Decks");
                    PackageRelationshipCollection decks = package.GetRelationshipsByType("http://schemas.octgn.org/set/deck");
                    var buffer = new byte[0x1000];
                    foreach (PackageRelationship deckRel in decks)
                    {
                        PackagePart deck = package.GetPart(deckRel.TargetUri);
                        string deckuri = Path.GetFileName(deck.Uri.ToString());
                        if (deckuri == null) continue;
                        string deckFilename = Path.Combine(path, deckuri);
                        using (Stream deckStream = deck.GetStream(FileMode.Open, FileAccess.Read))
                        using (
                            FileStream targetStream = File.Open(deckFilename, FileMode.Create, FileAccess.Write, FileShare.Read)
                            )
                        {
                            while (true)
                            {
                                int read = deckStream.Read(buffer, 0, buffer.Length);
                                if (read == 0) break;
                                targetStream.Write(buffer, 0, read);
                            }
                        }
                    }
                    package.Close();
                }
            }
            catch (Exception e)
            {
                throw e;
            }

        }

        public void Uninstallset(Set set)
        {
            DbContext.Get().Remove(set);
        }

        public Set GetById(Guid id)
        {
            return Sets.FirstOrDefault(x => x.Id == id);
        }

        public Set FromXmlReader(string packageName, XmlReader reader)
        {
            var ret = new Set();
            ret.PackageName = packageName;
            ret.Name = reader.GetAttribute("name");
            string gaid = reader.GetAttribute("id");
            if (gaid != null) ret.Id = new Guid(gaid);
            string gi = reader.GetAttribute("gameId");
            if (gi != null)
            {
                var gameId = new Guid(gi);
                ret.GameId = GameManager.Get().GetById(gameId).Id;
            }
            string gv = reader.GetAttribute("gameVersion");
            if (gv != null) ret.GameVersion = new Version(gv);
            Version ver;
            Version.TryParse(reader.GetAttribute("version"), out ver);
            ret.Version = ver ?? new Version(0, 0);
            reader.ReadStartElement("set");
            ret.Cards = new List<Card>();
            ret.Markers = new List<Marker>();
            ret.Packs = new List<Pack>();
            return ret;
        }

        public Set FromFile(string path)
        {
            var ret = new DataNew.Entities.Set();
            using (Package package = Package.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                PackageRelationship defRelationship =
                    package.GetRelationshipsByType("http://schemas.octgn.org/set/definition").First();
                PackagePart definition = package.GetPart(defRelationship.TargetUri);

                var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, IgnoreWhitespace = true };

                XmlSerializer serializer = new XmlSerializer(typeof(set));
                var set = (set)serializer.Deserialize(definition.GetStream());

                var game = GameManager.Get().GetById(new Guid(set.gameId));

                ret.PackageName = path;
                ret.Name = set.name;
                ret.Cards = set.cards.Select(
                        card =>
                            {
                                var c = new Card
                                            {
                                                Alternate = new Guid(card.alternate),
                                                Name = card.name,
                                                Id = new Guid(card.id),
                                                IsMutable = false
                                            };
                                c.ImageUri =definition.GetRelationship("C" + c.Id.ToString("N")).TargetUri.OriginalString;

                                if (!string.IsNullOrWhiteSpace(card.dependent))
                                {
                                    try
                                    {
                                        c.Dependent = new Guid(card.dependent).ToString();
                                    }
                                    catch
                                    {
                                        try
                                        {
                                            c.Dependent = Boolean.Parse(card.dependent).ToString();
                                        }
                                        catch
                                        {

                                            throw new ArgumentException(
                                                String.Format(
                                                    "The value {0} is not of expected type for property Dependent. Dependent must be either true/false, or the Guid of the card to use instead.",
                                                    c.Dependent));
                                        }
                                    }
                                }
                                c.Properties = new Dictionary<PropertyDef, object>();
                                foreach (var p in card.property)
                                {
                                    var prop = game.CustomProperties.FirstOrDefault(x => x.Name == p.name);
                                    if (prop == null)
                                        throw new ArgumentException(string.Format("The property '{0}' is unknown", p.name));
                                    switch (prop.Type)
                                    {
                                        case PropertyType.String:
                                            c.Properties.Add(prop,p.value);
                                            break;
                                        case PropertyType.Integer:
                                            c.Properties.Add(prop,Int32.Parse(p.value));
                                            break;
                                        case PropertyType.GUID:
                                            c.Properties.Add(prop, Guid.Parse(p.value));
                                            break;
                                        case PropertyType.Char:
                                            c.Properties.Add(prop, Char.Parse(p.value));
                                            break;
                                        default:
                                            throw new ArgumentOutOfRangeException();
                                    }
                                }
                                return c;
                            });

                using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Paths),"Schemas/CardSet.xsd"))
                    //CardSet.xsd determines the "attributes" of a card (name, guid, alternate, dependent)
                    if (s != null)
                        using (XmlReader reader = XmlReader.Create(s))
                            settings.Schemas.Add(null, reader);

                // Read the cards
                using (XmlReader reader = XmlReader.Create(definition.GetStream(), settings))
                {
                    reader.ReadToFollowing("set"); // <?xml ... ?>

                    ret.PackageName = path;
                    ret.Name = reader.GetAttribute("name");
                    string gaid = reader.GetAttribute("id");
                    if (gaid != null) ret.Id = new Guid(gaid);
                    string gv = reader.GetAttribute("gameVersion");
                    if (gv != null) ret.GameVersion = new Version(gv);
                    string gi = reader.GetAttribute("gameId");
                    if (gi != null)
                    {
                        ret.GameId = new Guid(gi);
                    }
                    Version ver;
                    Version.TryParse(reader.GetAttribute("version"), out ver);
                    ret.Version = ver ?? new Version(0, 0);
                    reader.ReadStartElement("set");

                    return ret;
                }
            }
        }

    }
}