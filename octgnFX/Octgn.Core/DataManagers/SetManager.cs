namespace Octgn.Core.DataManagers
{
    using System;
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

        public void InstallSet(string filename)
        {
            var set = this.FromFile(filename);
            if (set == null) return; // Really need an exception thrown out of this method

        }

        public Octgn.DataNew.Entities.Set GetById(Guid id)
        {
            return GameManager.Get().Games.SelectMany(x => x.Sets).FirstOrDefault(x => x.Id == id);
        }

        public DataNew.Entities.Set FromFile(string path)
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

                ret.PackageName = path;
                ret.Name = set.name;
                ret.Cards = set.cards.Select(x=>new Card
                                                    {
                                                        Alternate = x.alternate,
                                                        Name = x.name,
                                                        Id = x.id,
                                                        Properties = x.property,
                                                        Dependent = x.dependent,
                                                        
                                                    })

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