////using System;
////using System.Collections.Generic;
////using System.IO;
////using System.IO.Packaging;
////using System.Linq;
////using System.Security.Cryptography;
////using System.Windows;
////using System.Xml;
////using System.Xml.Linq;
////using Octgn.Data;

////namespace Octgn.Definitions
////{
////    using Octgn.Core.DataExtensionMethods;
////    using Octgn.Core.DataManagers;
////    using Octgn.DataNew;
////    using Octgn.Library;

////    public class GameDef
////    {
////        private GameDef()
////        {
////        }

////        public Version Version { get; private set; }
////        private Version OctgnVersion { get; set; }
////        public string Name { get; private set; }
////        public Guid Id { get; private set; }
////        public string FileHash { get; private set; }
////        public DeckDef DeckDefinition { get; private set; }
////        public DeckDef SharedDeckDefinition { get; private set; }
////        public CardDef CardDefinition { get; private set; }
////        public GroupDef TableDefinition { get; private set; }
////        public PlayerDef PlayerDefinition { get; private set; }
////        public SharedDef GlobalDefinition { get; private set; }
////        public int MarkerSize { get; private set; }
////        public bool CardsRevertToOriginalOnGroupChange { get; private set; }
////        public List<ScriptDef> Scripts { get; private set; }
////        public List<VariableDef> Variables { get; private set; }
////        public List<GlobalVariableDef> GlobalVariables { get; private set; }
////        public List<FontDef> Fonts { get; private set; }
////        public List<IconDef> Icons { get; private set; }

////        internal string FileName { get; set; }

////        internal string BasePath
////        {
////            get { return Paths.DataDirectory + '\\'; }
////        }

////        internal string DecksPath
////        {
////            get { return BasePath + @"Decks\"; }
////        }

////        internal string SetsPath
////        {
////            get { return BasePath + @"Sets\"; }
////        }

////        internal string MarkersPath
////        {
////            get { return BasePath + @"Markers\"; }
////        }

////        internal string PackUri
////        {
////            get { return "pack://file:,,," + FileName.Replace('\\', ','); }
////        }

////        internal void CreateFolders()
////        {
////            var folders = new[] {BasePath, DecksPath, SetsPath};
////            foreach (string path in folders.Where(path => !Directory.Exists(path)))
////                Directory.CreateDirectory(path);
////        }

////        public bool CheckVersion()
////        {
////            if (OctgnVersion > OctgnApp.OctgnVersion)
////            {
////                MessageBox.Show(
////                    string.Format("This game requires Octgn {0} or higher", OctgnVersion.ToString(4)),
////                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
////                return false;
////            }

////            if (OctgnVersion < OctgnApp.BackwardCompatibility)
////            {
////                MessageBox.Show(
////                    "This game is incompatible with newer Octgn versions",
////                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
////                return false;
////            }

////            return true;
////        }
        
////        public bool Install()
////        {
////            //Fix def filename
////            String newFilename = Uri.UnescapeDataString(FileName);
////            if (!newFilename.ToLower().Equals(FileName.ToLower()))
////            {
////                try
////                {
////                    File.Move(FileName, newFilename);
////                }
////                catch (Exception)
////                {
////                    MessageBox.Show(
////                        "This file is currently in use. Please close whatever application is using it and try again.");
////                    return false;
////                }
////            }

////            try
////            {
////                GameDef game = GameDef.FromO8G(newFilename);
////                //Move the definition file to a new location, so that the old one can be deleted
////                string path = Path.Combine(Prefs.DataDirectory,"Games", game.Id.ToString(), "Defs");
////                if (!Directory.Exists(path))
////                    Directory.CreateDirectory(path);
////                var fi = new FileInfo(newFilename);
////                string copyto = Path.Combine(path, fi.Name);
////                try
////                {
////                    if (newFilename.ToLower() != copyto.ToLower())
////                        File.Copy(newFilename, copyto, true);
////                }
////                catch (Exception)
////                {
////                    MessageBox.Show(
////                        "File in use. You shouldn't install games or sets when in the deck editor or when playing a game.");
////                    return false;
////                }
////                newFilename = copyto;
////                // Open the archive
////                game = GameDef.FromO8G(newFilename);
////                if (!game.CheckVersion()) return false;

////                //Check game scripts
////                //if (!Windows.UpdateChecker.CheckGameDef(game))
////                //    return false;

////                // Check if the game already exists
////                if (GameManager.Get().GetById(game.Id) != null)
////                    if (
////                        MessageBox.Show("This game already exists.\r\nDo you want to overwrite it?", "Confirmation",
////                                        MessageBoxButton.YesNo, MessageBoxImage.Exclamation) != MessageBoxResult.Yes)
////                        return false;

////                if (Fonts.Count > 0)
////                {
////                    InstallFonts();
////                }

////                var gameData = new DataNew.Entities.Game
////                                   {
////                                       Id = game.Id,
////                                       Name = game.Name,
////                                       Filename = new FileInfo(newFilename).Name,
////                                       Version = game.Version,
////                                       CardWidth = game.CardDefinition.Width,
////                                       CardHeight = game.CardDefinition.Height,
////                                       CardBack = game.CardDefinition.Back,
////                                       DeckSections = game.DeckDefinition.Sections.Keys.ToList(),
////                                       SharedDeckSections =
////                                           game.SharedDeckDefinition == null
////                                               ? null
////                                               : game.SharedDeckDefinition.Sections.Keys.ToList(),
////                                       FileHash = game.FileHash
////                                   };
////                var rootDir = Path.Combine(Prefs.DataDirectory, "Games", game.Id.ToString()) + "\\";
////                using (Package package = Package.Open(copyto, FileMode.Open, FileAccess.Read, FileShare.Read))
////                {
////                    foreach (var p in package.GetParts())
////                    {
////                        this.ExtractPart(p,rootDir);
////                    }
////                }

////                gameData.CustomProperties = game.CardDefinition.Properties.Select(x => x.Value).ToList();

////                GameManager.Get().InstallGame(gameData);
////                return true;
////            }
////            catch (FileFormatException)
////            {
////                //Removed ex.Message. The user doesn't need to see the exception
////                MessageBox.Show("Your game definition file is corrupt. Please redownload it.", "Error",
////                                MessageBoxButton.OK, MessageBoxImage.Error);
////                return false;
////            }
////        }

////        private static GameDef LoadFromXml(XElement xml, PackagePart part)
////        {
////            if (xml.Name != Defs.XmlnsOctgn + "game")
////                throw new InvalidFormatException("The root document element must be <game>.");

////            return new GameDef
////                       {
////                           Id = xml.Attr<Guid>("id"),
////                           Name = xml.Attr<string>("name"),
////                           Version = xml.Attr<Version>("version"),
////                           OctgnVersion = xml.Attr<Version>("octgnVersion"),
////                           MarkerSize = xml.Attr<int>("markersize"),
////                           CardsRevertToOriginalOnGroupChange = xml.Attr<bool>("cardsRevertToOriginalOnGroupChange"),
////                           CardDefinition = CardDef.LoadFromXml(xml.Child("card"), part),
////                           DeckDefinition = DeckDef.LoadFromXml(xml.Child("deck")),
////                           SharedDeckDefinition = DeckDef.LoadFromXml(xml.Child("sharedDeck")),
////                           TableDefinition = GroupDef.LoadFromXml(xml.Child("table"), part, 0),
////                           PlayerDefinition = PlayerDef.LoadFromXml(xml.Child("player"), part),
////                           GlobalDefinition = SharedDef.LoadFromXml(xml.Child("shared"), part),
////                           GlobalVariables = GlobalVariableDef.LoadAllFromXml(xml.Child("globalvariables")),
////                           Variables = VariableDef.LoadAllFromXml(xml.Child("variables")),
////                           Scripts = ScriptDef.LoadAllFromXml(xml.Child("scripts"), part),
////                           Fonts = FontDef.LoadAllFromXml(xml.Child("fonts"), part),
////                           Icons = IconDef.LoadAllFromXml(xml.Child("iconreplacements"), part)
////                       };
////        }

////        public static GameDef FromO8G(string filename)
////        {
////            // HACK: workaround for the BitmapDecoder bug (see also GameManager.xaml.cs, in c'tor)
////            GC.Collect(GC.MaxGeneration);
////            GC.WaitForPendingFinalizers();
////            string fhash = "";
////            //Get hash for file.
////            using (MD5 md5 = new MD5CryptoServiceProvider()) {
////                using (FileStream file = new FileStream(filename, FileMode.Open,FileAccess.Read,FileShare.Read)) {
////                    byte[] retVal = md5.ComputeHash(file);
////                    fhash =  BitConverter.ToString(retVal).Replace("-", "");	// hex string
////                    file.Close();
////                }
////            }
////            using (Package package = Package.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
////            {
////                PackageRelationship defRelationship =
////                    package.GetRelationshipsByType("http://schemas.octgn.org/game/definition").First();
////                PackagePart definition = package.GetPart(defRelationship.TargetUri);
////                using (XmlReader reader = XmlReader.Create(definition.GetStream(FileMode.Open, FileAccess.Read)))
////                {
////                    XDocument xDoc = XDocument.Load(reader);
////                    GameDef result = LoadFromXml(xDoc.Root, definition);
////                    result.FileHash = fhash;
////                    result.FileName = filename;
////                    reader.Close();
////                    return result;
////                }
////            }
////        }

////        public void InstallFonts()
////        {
////            var uri = new Uri(Program.Game.Definition.PackUri.Replace(',', '/'));
////            string defLoc = uri.LocalPath.Remove(0, 3).Replace('/', '\\');
////            using (Package package = Package.Open(defLoc, FileMode.Open, FileAccess.Read, FileShare.Read))
////            {
////                foreach (FontDef font in Fonts)
////                {
////                    PackagePart fontPart = package.GetPart(new Uri(font.FileName, UriKind.Relative));
////                    string fontFileName = font.FileName.TrimStart('/');
////                    string targetDir = Path.Combine(SimpleConfig.DataDirectory, "Games", Id.ToString(), fontFileName);
////                    ExtractPart(fontPart, targetDir);
////                }
////            }
////        }

////        private void ExtractPart(PackagePart packagePart, string targetDirectory)
////        {
////            string stringPart = packagePart.Uri.ToString().TrimStart('/');
////            Uri partUri = new Uri(stringPart, UriKind.Relative);
////            Uri uriFullFilePath = new Uri(new Uri(targetDirectory, UriKind.Absolute), partUri);

////            // Create the necessary directories based on the full part path
////            if (!Directory.Exists(Path.GetDirectoryName(uriFullFilePath.LocalPath)))
////            {
////                Directory.CreateDirectory(Path.GetDirectoryName(uriFullFilePath.LocalPath));
////            }

////            if (!File.Exists(uriFullFilePath.LocalPath))
////            {
////                // Write the file from the part’s content stream.
////                using (FileStream fileStream = File.Create(uriFullFilePath.LocalPath))
////                {
////                    packagePart.GetStream().CopyTo(fileStream);
////                }
////            }
////        }
////    }
////}