namespace Octgn.Core.DataManagers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    using Octgn.DataNew;
    using Octgn.DataNew.Entities;
    using Octgn.DataNew.FileDB;
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

        public Set GetById(Guid id)
        {
            return DbContext.Get().SetsById(id).FirstOrDefault();
            //return Sets.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Set> GetByGameId(Guid id)
        {
            return DbContext.Get().SetQuery.By(x => x.GameId,Op.Eq, id);
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
            ret.Markers = new List<Marker>();
            ret.Packs = new List<Pack>();
            ret.Cards = new List<Card>();
            return ret;
        }

        /// <summary>
        /// This is only used once, don't use this as it doesn't import cards.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Set FromFile(string path)
        {
            var ret = new DataNew.Entities.Set();
            using (Package package = Package.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                PackageRelationship defRelationship =
                    package.GetRelationshipsByType("http://schemas.octgn.org/set/definition").First();
                PackagePart definition = package.GetPart(defRelationship.TargetUri);

                var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, IgnoreWhitespace = true };

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