namespace o8build
{
    using System;
    using System.IO;
    using System.IO.Packaging;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    public class Set : IDisposable
    {
        internal Set()
        {
        }

        public static Set SetFromFile(string filename)
        {
            Package package = Package.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            PackageRelationship defRelationship = package.GetRelationshipsByType("http://schemas.octgn.org/set/definition").First();
            PackagePart definition = package.GetPart(defRelationship.TargetUri);

            var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema, IgnoreWhitespace = true };
            var setxsd = Assembly.GetExecutingAssembly().GetManifestResourceNames().FirstOrDefault(x => x.Contains("CardSet.xsd"));
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(setxsd))
            {
                //CardSet.xsd determines the "attributes" of a card (name, guid, alternate, dependent)
                if (stream != null) using (XmlReader reader = XmlReader.Create(stream)) settings.Schemas.Add(null, reader);
            }
            // Read the cards
            using (XmlReader reader = XmlReader.Create(definition.GetStream(), settings))
            {
                reader.ReadToFollowing("set"); // <?xml ... ?>

                return new Set(filename, reader, package);
            }
        }

        public Set(string packageName, XmlReader reader, Package package)
        {
            this.PackageName = packageName;
            this.Name = reader.GetAttribute("name");
            string gaid = reader.GetAttribute("id");
            if (gaid != null) this.Id = new Guid(gaid);
            string gi = reader.GetAttribute("gameId");
            if (gi != null)
            {
                var gameId = new Guid(gi);
                this.GameId = gameId;
            }
            string gv = reader.GetAttribute("gameVersion");
            if (gv != null) this.GameVersion = new Version(gv);
            Version ver;
            Version.TryParse(reader.GetAttribute("version"), out ver);
            this.Version = ver ?? new Version(0, 0);
            reader.ReadStartElement("set");
            Package = package;
            CardThingys = package.GetRelationshipsByType("http://schemas.octgn.org/picture");
            foreach (var c in CardThingys)
            {
                c.TargetUri; // /images/00.png
                c.Id; // C50d634aa9aed458381d6c3097c090271 "C" + Guid.ToString("N")

            }

        }

        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
        public Guid GameId { get; internal set; }
        public Version GameVersion { get; internal set; }
        public Version Version { get; internal set; }
        public string Filename { get; internal set; }
        public string PackageName { get; internal set; }
        public Package Package { get; internal set; }
        public PackageRelationshipCollection CardThingys { get; set; }
        public override string ToString()
        {
            return this.Name + " " + "(" + this.Version + ")";
        }

        public void Dispose()
        {

        }

        public string GetPackUri()
        {
            return "pack://file:,,," + this.PackageName.Replace('\\', ',');
        }
    }
}