using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Octgn.Data
{
    public class Patch
    {
        private static readonly XmlReaderSettings xmlSettings = GetXmlReaderSettings();
        private readonly string filename;
        private int current;
        private Game game;
        private Guid gameId;
        private int max;

        public Patch(string filename)
        {
            this.filename = filename;
        }

        public event Action<int, int, string, bool> Progress;

        protected void OnProgress(string message = null, bool isError = false)
        {
            if (Progress != null)
                Progress(current, max, message, isError);
        }

        public void Apply(GamesRepository repository, bool patchInstalledSets, string patchFolder)
        {
            if (!patchInstalledSets && patchFolder == null) return;

            using (Package package = Package.Open(filename, FileMode.Open, FileAccess.Read))
            {
                ReadPackageDescription(package);

                // Get the list of sets to potentially patch
                game = repository.Games.FirstOrDefault(g => g.Id == gameId);
                if (game == null) return;
                List<string> installedSets = game.Sets.Select(s => s.PackageName).ToList();
                List<string> uninstalledSets;

                if (patchFolder != null)
                {
                    string[] files = Directory.GetFiles(patchFolder, "*.o8s");
                    uninstalledSets = files.Except(installedSets).ToList();
                    if (!patchInstalledSets)
                        installedSets = files.Intersect(installedSets).ToList();
                }
                else
                    uninstalledSets = new List<string>(0);

                current = 0;
                max = installedSets.Count + uninstalledSets.Count;
                OnProgress();

                foreach (string set in installedSets)
                    SafeApply(package, set, true);

                foreach (string set in uninstalledSets)
                    SafeApply(package, set, false);
            }
        }

        private void SafeApply(Package package, string localFilename, bool installed)
        {
            try
            {
                Apply(package, localFilename, installed);
            }
            catch (Exception ex)
            {
                OnProgress(string.Format("Error while patching: {0}\nDetails: {1}", localFilename, ex.Message), true);
            }
            finally
            {
                current++;
                OnProgress();
            }
        }

        private void Apply(Package package, string localFilename, bool installed)
        {
            using (Package setPkg = Package.Open(localFilename, FileMode.Open, FileAccess.ReadWrite))
            {
                // Extract information about the target set
                PackageRelationship defRelationship =
                    setPkg.GetRelationshipsByType("http://schemas.octgn.org/set/definition").First();
                PackagePart definition = setPkg.GetPart(defRelationship.TargetUri);
                Set set;
                using (XmlReader reader = XmlReader.Create(definition.GetStream(), xmlSettings))
                {
                    reader.ReadToFollowing("set"); // <?xml ... ?>
                    set = new Set(localFilename, reader, game.repository);
                    // Check if the set game matches the patch
                    if (set.Game != game) return;
                }

                // Check if there is a patch for this set
                string relationId = "S" + set.Id.ToString("N");
                if (!package.RelationshipExists(relationId)) return;

                PackagePart patchPart = package.GetPart(package.GetRelationship(relationId).TargetUri);
                XDocument patchDoc;
                using (Stream stream = patchPart.GetStream())
                    patchDoc = XDocument.Load(stream);

                // Check if the set is at least the required version for patching
                if (set.Version < patchDoc.Root.Attr<Version>("minVersion")) return;
                if (set.Version > patchDoc.Root.Attr<Version>("maxVersion")) return;

                if (installed)
                    game.DeleteSet(game.Sets.Single(s => s.PackageName == localFilename));

                // Process the set 
                if (patchDoc.Root != null)
                    foreach (XElement action in patchDoc.Root.Elements())
                        switch (action.Name.LocalName)
                        {
                            case "new":
                                {
                                    var targetUri = new Uri(action.Attr<string>("targetUri"), UriKind.Relative);
                                    var relationshipId = action.Attr<string>("relationshipId");
                                    var contentType = action.Attr<string>("contentType");
                                    PackagePart part = setPkg.PartExists(targetUri)
                                                           ? setPkg.GetPart(targetUri)
                                                           : setPkg.CreatePart(targetUri, contentType,
                                                                               CompressionOption.Normal);
                                    if (part != null)
                                        using (Stream targetStream = part.GetStream(FileMode.Create, FileAccess.Write))
                                        using (
                                            Stream srcStream =
                                                package.GetPart(patchPart.GetRelationship(relationshipId).TargetUri).
                                                    GetStream())
                                            srcStream.CopyTo(targetStream);
                                    break;
                                }

                            case "newrel":
                                {
                                    var partUri = new Uri(action.Attr<string>("partUri"), UriKind.Relative);
                                    var relationshipId = action.Attr<string>("relationshipId");
                                    var targetUri = new Uri(action.Attr<string>("targetUri"), UriKind.Relative);
                                    var relationshipType = action.Attr<string>("relationshipType");

                                    PackagePart part = setPkg.GetPart(partUri);
                                    if (part.RelationshipExists(relationshipId))
                                        part.DeleteRelationship(relationshipId);
                                    part.CreateRelationship(targetUri, TargetMode.Internal, relationshipType,
                                                            relationshipId);
                                    break;
                                }

                            default:
                                throw new InvalidFileFormatException("Unknown patch action: " + action.Name);
                        }
            }

            OnProgress(string.Format("{0} patched.", Path.GetFileName(localFilename)));

            if (installed)
                try
                {
                    game.InstallSet(localFilename);
                }
                catch (Exception ex)
                {
                    OnProgress(string.Format("{0} can't be re-installed.\nDetails: {1}", localFilename, ex.Message),
                               true);
                }
        }

        private void ReadPackageDescription(Package package)
        {
            PackagePart part = package.GetPart(package.GetRelationship("PatchDescription").TargetUri);
            using (XmlReader reader = XmlReader.Create(part.GetStream(FileMode.Open, FileAccess.Read)))
            {
                XDocument doc = XDocument.Load(reader);
                if (doc.Root == null) return;
                XAttribute xAttribute = doc.Root.Attribute("gameId");
                if (xAttribute != null) gameId = new Guid(xAttribute.Value);
            }
        }

        private static XmlReaderSettings GetXmlReaderSettings()
        {
            var result = new XmlReaderSettings {ValidationType = ValidationType.Schema, IgnoreWhitespace = true};
            using (
                Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof (GamesRepository),
                                                                                     "CardSet.xsd"))
            using (XmlReader reader = XmlReader.Create(s))
                result.Schemas.Add(null, reader);
            return result;
        }
    }
}