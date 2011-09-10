using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Packaging;
using System.Xml;
using System.Xml.Linq;

namespace Octgn.Data
{
  public class Patch
  {
    private string filename;
    private Guid gameId;
    private Game game;
    private int current, max;
    private static readonly XmlReaderSettings xmlSettings = GetXmlReaderSettings();

    public event Action<int, int, string, bool> Progress;

    public Patch(string filename)
    {
      this.filename = filename;
    }

    protected void OnProgress(string message = null, bool isError = false)
    {
      if (Progress != null)
        Progress(current, max, message, isError);
    }

    public void Apply(GamesRepository repository, bool patchInstalledSets, string patchFolder)
    {
      if (!patchInstalledSets && patchFolder == null) return;

      using (var package = Package.Open(filename, FileMode.Open, FileAccess.Read))
      {
        ReadPackageDescription(package);
        
        // Get the list of sets to potentially patch
        game = repository.Games.FirstOrDefault(g => g.Id == gameId);
        var installedSets = game.Sets.Select(s => s.packageName).ToList();
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

    private void SafeApply(Package package, string filename, bool installed)
    {
      try
      {
        Apply(package, filename, installed);
      }
      catch (Exception ex)
      {
        OnProgress(string.Format("Error while patching: {0}\nDetails: {1}", filename, ex.Message), true);
      }
      finally
      {
        current++;
        OnProgress();
      }
    }

    private void Apply(Package package, string filename, bool installed)
    {
      using (var setPkg = Package.Open(filename, FileMode.Open, FileAccess.ReadWrite))
      {
        // Extract information about the target set
        var defRelationship = setPkg.GetRelationshipsByType("http://schemas.octgn.org/set/definition").First();
        var definition = setPkg.GetPart(defRelationship.TargetUri);
        Set set;
        using (var reader = XmlReader.Create(definition.GetStream(), xmlSettings))
        {
          reader.ReadToFollowing("set");  // <?xml ... ?>
          set = new Set(filename, reader, game.repository);
          // Check if the set game matches the patch
          if (set.Game != game) return;
        }

        // Check if there is a patch for this set
        string relationId = "S" + set.Id.ToString("N");
        if (!package.RelationshipExists(relationId)) return;
        
        var patchPart = package.GetPart(package.GetRelationship(relationId).TargetUri);
        XDocument patchDoc;
        using (var stream = patchPart.GetStream())
          patchDoc = XDocument.Load(stream);

        // Check if the set is at least the required version for patching
        if (set.Version < patchDoc.Root.Attr<Version>("minVersion")) return;
        if (set.Version > patchDoc.Root.Attr<Version>("maxVersion")) return;

        if (installed)
          game.DeleteSet(game.Sets.Single(s => s.packageName == filename));

        // Process the set 
        foreach (XElement action in patchDoc.Root.Elements())
          switch (action.Name.LocalName)
          {
            case "new":
              {
                Uri targetUri = new Uri(action.Attr<string>("targetUri"), UriKind.Relative);
                string relationshipId = action.Attr<string>("relationshipId");
                string contentType = action.Attr<string>("contentType");
                PackagePart part = setPkg.PartExists(targetUri) ?
                  setPkg.GetPart(targetUri) :
                  setPkg.CreatePart(targetUri, contentType, CompressionOption.Normal);
                using (var targetStream = part.GetStream(FileMode.Create, FileAccess.Write))
                using (var srcStream = package.GetPart(patchPart.GetRelationship(relationshipId).TargetUri).GetStream())
                  srcStream.CopyTo(targetStream);
                break;
              }

            case "newrel":
              {
                Uri partUri = new Uri(action.Attr<string>("partUri"), UriKind.Relative);
                string relationshipId = action.Attr<string>("relationshipId");
                Uri targetUri = new Uri(action.Attr<string>("targetUri"), UriKind.Relative);
                string relationshipType = action.Attr<string>("relationshipType");

                PackagePart part = setPkg.GetPart(partUri);
                if (part.RelationshipExists(relationshipId)) part.DeleteRelationship(relationshipId);
                part.CreateRelationship(targetUri, TargetMode.Internal, relationshipType, relationshipId);
                break;
              }

            default:
              throw new InvalidFileFormatException("Unknown patch action: " + action.Name);
          }
      }

      OnProgress(string.Format("{0} patched.", System.IO.Path.GetFileName(filename)));

      if (installed)
        try
        { game.InstallSet(filename); }
        catch (Exception ex)
        {
          OnProgress(string.Format("{0} can't be re-installed.\nDetails: {1}", filename, ex.Message), true);
        }
    }

    private void ReadPackageDescription(Package package)
    {
      var part = package.GetPart(package.GetRelationship("PatchDescription").TargetUri);
      using (var reader = XmlReader.Create(part.GetStream(FileMode.Open, FileAccess.Read)))
      {
        var doc = XDocument.Load(reader);
        gameId = new Guid(doc.Root.Attribute("gameId").Value);
      }
    }

    private static XmlReaderSettings GetXmlReaderSettings()
    {
      var result = new XmlReaderSettings();
      result.ValidationType = ValidationType.Schema;
      result.IgnoreWhitespace = true;
      using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(GamesRepository), "CardSet.xsd"))
      using (XmlReader reader = XmlReader.Create(s))
        result.Schemas.Add(null, reader);
      return result;
    }
  }
}
