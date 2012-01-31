using System;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Octgn.Data;
using Octgn.Properties;
using System.Xml;
using System.Collections.Generic;

namespace Octgn.Definitions
{
  public class GameDef
  {
    public Version Version { get; private set; }
    private Version OctgnVersion { get; set; }
    public string Name { get; private set; }
    public Guid Id { get; private set; }    
    public DeckDef DeckDefinition { get; private set; }
    public DeckDef SharedDeckDefinition { get; private set; }
    public CardDef CardDefinition { get; private set; }
    public GroupDef TableDefinition { get; private set; }
    public PlayerDef PlayerDefinition { get; private set; }
    public SharedDef GlobalDefinition { get; private set; }
    public int MarkerSize { get; private set; }
    public List<ScriptDef> Scripts { get; private set; }
    public List<VariableDef> Variables { get; private set; }
    public List<GlobalVariableDef> GlobalVariables { get; private set; }

    internal string FileName
    { get; set; }
    internal string BasePath
    { get { return Path.Combine(GamesRepository.BasePath, Id.ToString()) + '\\'; } }
    internal string DecksPath
    { get { return BasePath + @"Decks\"; } }
    internal string SetsPath
    { get { return BasePath + @"Sets\"; } }
    internal string MarkersPath
    { get { return BasePath + @"Markers\"; } }
    internal string PackUri
    { get { return "pack://file:,,," + FileName.Replace('\\', ','); } }

    private GameDef()
    { }

    internal void CreateFolders()
    {
      string[] folders = new string[] { BasePath, DecksPath, SetsPath };
      foreach (string path in folders)
        if (!Directory.Exists(path))
          Directory.CreateDirectory(path);
    }

    public bool CheckVersion()
    {
      if (OctgnVersion > OctgnApp.OctgnVersion)
      {
        MessageBox.Show(
               string.Format("This game requires OCTGN {0} or higher", OctgnVersion.ToString(4)),
          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
      }

      if (OctgnVersion < OctgnApp.BackwardCompatibility)
      {
        MessageBox.Show(
          "This game is incompatible with newer OCTGN versions",
          "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        return false;
      }

      return true;
    }

    private static GameDef LoadFromXml(XElement xml, PackagePart part)
    {
      if (xml.Name != Defs.xmlnsOctgn + "game")
        throw new InvalidFormatException("The root document element must be <game>.");

      return new GameDef
      {
        Id = xml.Attr<Guid>("id"),
        Name = xml.Attr<string>("name"),
        Version = xml.Attr<Version>("version"),
        OctgnVersion = xml.Attr<Version>("octgnVersion"),
        MarkerSize = xml.Attr<int>("markersize"),
        CardDefinition = CardDef.LoadFromXml(xml.Child("card"), part),
        DeckDefinition = DeckDef.LoadFromXml(xml.Child("deck")),
        SharedDeckDefinition = DeckDef.LoadFromXml(xml.Child("sharedDeck")),
        TableDefinition = GroupDef.LoadFromXml(xml.Child("table"), part, 0),
        PlayerDefinition = PlayerDef.LoadFromXml(xml.Child("player"), part),
        GlobalDefinition = SharedDef.LoadFromXml(xml.Child("shared"), part),
        GlobalVariables = GlobalVariableDef.LoadAllFromXml(xml.Child("globalvariables")),
        Variables = VariableDef.LoadAllFromXml(xml.Child("variables")),
        Scripts = ScriptDef.LoadAllFromXml(xml.Child("scripts"), part)
      };
    }

    public static GameDef FromO8G(string filename)
    {
      // HACK: workaround for the BitmapDecoder bug (see also GameManager.xaml.cs, in c'tor)
      GC.Collect(GC.MaxGeneration);
      GC.WaitForPendingFinalizers();

      using (Package package = Package.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        var defRelationship = package.GetRelationshipsByType("http://schemas.octgn.org/game/definition").First();
        var definition = package.GetPart(defRelationship.TargetUri);
        using (var reader = XmlReader.Create(definition.GetStream(FileMode.Open, FileAccess.Read)))
        {
          var xDoc = XDocument.Load(reader);
          GameDef result = LoadFromXml(xDoc.Root, definition);
          result.FileName = filename;
          return result;
        }
      }
    }
  }
}
