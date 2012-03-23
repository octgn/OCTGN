using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Xml;
using System.Xml.Linq;
using Octgn.Data;

namespace Octgn.Definitions
{
    public class GameDef
    {
        private GameDef()
        {
        }

        public Version Version { get; private set; }
        private Version OctgnVersion { get; set; }
        public string Name { get; private set; }
        public Guid Id { get; private set; }
        public string FileHash { get; private set; }
        public DeckDef DeckDefinition { get; private set; }
        public DeckDef SharedDeckDefinition { get; private set; }
        public CardDef CardDefinition { get; private set; }
        public GroupDef TableDefinition { get; private set; }
        public PlayerDef PlayerDefinition { get; private set; }
        public SharedDef GlobalDefinition { get; private set; }
        public int MarkerSize { get; private set; }
        public bool CardsRevertToOriginalOnGroupChange { get; private set; }
        public List<ScriptDef> Scripts { get; private set; }
        public List<VariableDef> Variables { get; private set; }
        public List<GlobalVariableDef> GlobalVariables { get; private set; }

        internal string FileName { get; set; }

        internal string BasePath
        {
            get { return GamesRepository.BasePath + '\\'; }
        }

        internal string DecksPath
        {
            get { return BasePath + @"Decks\"; }
        }

        internal string SetsPath
        {
            get { return BasePath + @"Sets\"; }
        }

        internal string MarkersPath
        {
            get { return BasePath + @"Markers\"; }
        }

        internal string PackUri
        {
            get { return "pack://file:,,," + FileName.Replace('\\', ','); }
        }

        internal void CreateFolders()
        {
            var folders = new[] {BasePath, DecksPath, SetsPath};
            foreach (string path in folders.Where(path => !Directory.Exists(path)))
                Directory.CreateDirectory(path);
        }

        public bool CheckVersion()
        {
            if (OctgnVersion > OctgnApp.OctgnVersion)
            {
                MessageBox.Show(
                    string.Format("This game requires Octgn {0} or higher", OctgnVersion.ToString(4)),
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (OctgnVersion < OctgnApp.BackwardCompatibility)
            {
                MessageBox.Show(
                    "This game is incompatible with newer Octgn versions",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private static GameDef LoadFromXml(XElement xml, PackagePart part)
        {
            if (xml.Name != Defs.XmlnsOctgn + "game")
                throw new InvalidFormatException("The root document element must be <game>.");

            return new GameDef
                       {
                           Id = xml.Attr<Guid>("id"),
                           Name = xml.Attr<string>("name"),
                           Version = xml.Attr<Version>("version"),
                           OctgnVersion = xml.Attr<Version>("octgnVersion"),
                           MarkerSize = xml.Attr<int>("markersize"),
                           CardsRevertToOriginalOnGroupChange = xml.Attr<bool>("cardsRevertToOriginalOnGroupChange"),
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
            string fhash = "";
            //Get hash for file.
            using (MD5 md5 = new MD5CryptoServiceProvider()) {
                using (FileStream file = new FileStream(filename, FileMode.Open,FileAccess.Read,FileShare.Read)) {
                    byte[] retVal = md5.ComputeHash(file);
                    fhash =  BitConverter.ToString(retVal).Replace("-", "");	// hex string
                    file.Close();
                }
            }
            using (Package package = Package.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                PackageRelationship defRelationship =
                    package.GetRelationshipsByType("http://schemas.octgn.org/game/definition").First();
                PackagePart definition = package.GetPart(defRelationship.TargetUri);
                using (XmlReader reader = XmlReader.Create(definition.GetStream(FileMode.Open, FileAccess.Read)))
                {
                    XDocument xDoc = XDocument.Load(reader);
                    GameDef result = LoadFromXml(xDoc.Root, definition);
                    result.FileHash = fhash;
                    result.FileName = filename;
                    reader.Close();
                    return result;
                }
            }
        }
    }
}