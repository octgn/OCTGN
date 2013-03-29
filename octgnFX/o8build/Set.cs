﻿namespace o8build
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

            XmlDocument doc = new XmlDocument();
            doc.Load(definition.GetStream());

            return new Set(filename, doc, package);
        }

        public Set(string packageName, XmlDocument doc, Package package)
        {
            this.Doc = doc;
            XmlNode setNode = Doc.GetElementsByTagName("set").Item(0);
            this.PackageName = packageName;
            this.Name = setNode.Attributes["name"].Value;
            string gaid = setNode.Attributes["id"].Value;
            if (gaid != null) this.Id = new Guid(gaid);
            string gi = setNode.Attributes["gameId"].Value;
            if (gi != null)
            {
                var gameId = new Guid(gi);
                this.GameId = gameId;
            }
            string gv = setNode.Attributes["gameVersion"].Value;
            if (gv != null) this.GameVersion = new Version(gv);
            Version ver;
            Version.TryParse(setNode.Attributes["version"].Value, out ver);
            this.Version = ver ?? new Version(0, 0);
            Package = package;
        }

        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
        public Guid GameId { get; internal set; }
        public Version GameVersion { get; internal set; }
        public Version Version { get; internal set; }
        public string Filename { get; internal set; }
        public string PackageName { get; internal set; }
        public Package Package { get; internal set; }

        public XmlDocument Doc { get; internal set; }
        public string UnpackBase { get; set; }

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

        public void ExtractImages()
        {
            PackageRelationship defRelationship = Package.GetRelationshipsByType("http://schemas.octgn.org/set/definition").First();
            PackagePart definition = Package.GetPart(defRelationship.TargetUri);
            XmlNodeList cards = Doc.GetElementsByTagName("card");
            string extractDir = Path.Combine(UnpackBase, "SetImages", Id.ToString(), "Cards");
            foreach (XmlNode card in cards)
            {
                Guid cardID = new Guid(card.Attributes["id"].Value);
                Uri cardImage = definition.GetRelationship("C" + cardID.ToString("N")).TargetUri;
                string imageUri = cardImage.ToString();
                string fileName = cardID.ToString() + imageUri.Substring(imageUri.LastIndexOf('.'));
                PackagePart part = Package.GetPart(cardImage);
                ExtractPart(part, extractDir, fileName);
            }
        }

        public void ExtractSetXML()
        {
            PackageRelationship defRelationship = Package.GetRelationshipsByType("http://schemas.octgn.org/set/definition").First();
            PackagePart definition = Package.GetPart(defRelationship.TargetUri);
            string defUri = definition.Uri.ToString();
            string fileName = defUri.Substring(defUri.LastIndexOf('/')+1);
            Console.WriteLine(fileName);
            ExtractPart(definition, UnpackBase, fileName);
        }

        private void ExtractPart(PackagePart packagePart, string targetDir, string fileName)
        {
            string extractPath = Path.Combine(targetDir, fileName);
            Uri uriFullFilePath = new Uri(extractPath);
            //Console.WriteLine(uriFullFilePath.ToString());
            // Create the necessary directories based on the full part path
            if (!Directory.Exists(Path.GetDirectoryName(uriFullFilePath.LocalPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(uriFullFilePath.LocalPath));
            }

            if (!File.Exists(uriFullFilePath.LocalPath))
            {
                // Write the file from the parts content stream.
                using (FileStream fileStream = File.Create(uriFullFilePath.LocalPath))
                {
                    packagePart.GetStream().CopyTo(fileStream);
                }
            }
        }
    }
}