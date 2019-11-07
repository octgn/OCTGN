namespace o8build
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Windows.Input;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using IronPython.Hosting;

    using Microsoft.Scripting;
    using Microsoft.Scripting.Hosting;

    using Octgn.Library;
    using Octgn.Library.Exceptions;
    using Octgn.ProxyGenerator;
    using System.Xml;
    using System.Text.RegularExpressions;

    public class GameValidator
    {
        internal DirectoryInfo Directory { get; set; }

        internal delegate void WarningDelegate(string message, params object[] args);
        internal event WarningDelegate OnWarning;

        protected virtual void Warning(string message, params object[] args)
        {
            WarningDelegate handler = OnWarning;
            if (handler != null) handler(message, args);
        }

        public GameValidator(string directory)
        {
            Directory = new DirectoryInfo(directory);
        }

        public void RunTests()
        {
            var tests = typeof(GameValidator)
                .GetMethods()
                .Where(x => x.GetCustomAttributes(typeof(GameValidatorAttribute), true).Any());

            foreach (var test in tests)
            {
                Console.WriteLine("Running Test {0}", test.Name);
                try
                {
                    test.Invoke(this, new object[] { });
                }
                catch (UserMessageException e)
                {
                    throw e;
                }
                catch (Exception e)
                {
                    throw e.InnerException ?? e;
                }
            }
        }

        [GameValidatorAttribute]
        public void TestFileStructure()
        {
            if (Directory.GetFiles().Where(x => x.Extension.ToLower() != ".nupkg" && x.Extension.ToLower() != ".o8g").ToArray().Length != 1)
            {
                var sb = new StringBuilder();
                sb.AppendLine("You can only have 1 file in the root of your game directory.");
                sb.AppendLine("====== These are the files you need to remove to continue ======");

                foreach (var f in Directory.GetFiles().Where(x => x.Extension.ToLower() != ".nupkg" && x.Extension.ToLower() != ".o8g").ToArray())
                {
                    sb.AppendLine(f.FullName);
                }
                sb.AppendLine("================================================================");
                throw new UserMessageException(sb.ToString());
            }
            if (this.Directory.GetFiles().First(x => x.Extension.ToLower() != ".nupkg" && x.Extension.ToLower() != ".o8g").Name != "definition.xml")
                throw new UserMessageException("You must have a definition.xml in the root of your game directory.");
            if (Directory.GetDirectories().Any(x => x.Name == "_rels"))
                throw new UserMessageException("The _rels folder is depreciated.");
            if (Directory.GetDirectories().Any(x => x.Name == "Sets"))
            {
                var setDir = Directory.GetDirectories().First(x => x.Name == "Sets");
                if (setDir.GetFiles("*", SearchOption.TopDirectoryOnly).Any())
                    throw new UserMessageException("You can only have folders in your Sets directory");
                // Check each sub directory of Sets and make sure that it has a proper root.
                foreach (var dir in setDir.GetDirectories())
                {
                    this.VerifySetDirectory(dir);
                }
            }
        }

        [GameValidatorAttribute]
        public void VerifyDef()
        {
            var libAss = Assembly.GetAssembly(typeof(Paths));
            var gamexsd = libAss.GetManifestResourceNames().FirstOrDefault(x => x.Contains("Game.xsd"));
            if (gamexsd == null)
                throw new UserMessageException("ERROR: Cannot load schema Game.xsd");
            var schemas = new XmlSchemaSet();
            var schema = XmlSchema.Read(libAss.GetManifestResourceStream(gamexsd), (sender, args) => { throw args.Exception; });
            schemas.Add(schema);

            var fileName = Directory.GetFiles().First(x => x.Name == "definition.xml").FullName;
            XmlReaderSettings settings = GetXmlReaderSettings();
            settings.Schemas = schemas;
            settings.ValidationEventHandler += new ValidationEventHandler(delegate(Object o, ValidationEventArgs e)
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    throw new UserMessageException(string.Format("line: {0} position: {1} msg: {2} file: {3}", e.Exception.LineNumber, e.Exception.LinePosition, e.Exception.Message, fileName));
                }
            });
            using (XmlReader validatingReader = XmlReader.Create(fileName, settings))
            {
                while (validatingReader.Read()) { /* just loop through document */ }
            }
            //XDocument doc = XDocument.Load(fileName);
            //string msg = "";
            //doc.Validate(schemas, (o, e) =>
            //{
            //    msg = string.Format("{0} line: {1} position: {2}", e.Message, e.Exception.LineNumber, e.Exception.LinePosition);
            //});
            //if (!string.IsNullOrWhiteSpace(msg))
            //    throw new UserMessageException(msg);
        }

        [GameValidatorAttribute]
        public void VerifyDefPaths()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(game));
            var fs = File.Open(Directory.GetFiles().First(x => x.Name == "definition.xml").FullName, FileMode.Open);
            var game = (game)serializer.Deserialize(fs);
            fs.Close();
            var path = "";

            ValidateGameAttributes(game);

            if (game.scripts != null)
            {
                foreach (var s in game.scripts)
                {
                    // Let's check for valid attributes
                    if (String.IsNullOrWhiteSpace(s.src))
                    {
                        throw GenerateEmptyAttributeException("Script", "src");
                    }

                    path = Path.Combine(Directory.FullName, s.src.Trim('\\', '/'));

                    if (!File.Exists(path))
                    {
                        throw GenerateFileDoesNotExistException(path, "Script", s.src, "src", s.src);
                    }
                }
            }

            if (game.fonts != null)
            {
                foreach (var font in game.fonts)
                {
                    // Check for valid attributes
                    if (!String.IsNullOrWhiteSpace(font.src))
                    {
                        path = Path.Combine(Directory.FullName, font.src);

                        if (!File.Exists(path))
                        {
                            throw GenerateFileDoesNotExistException(path, "Font", font.target.ToString(), "src", font.src);
                        }
                    }
                    if (font.size != null && int.Parse(font.size) < 1)
                    {
                        throw new UserMessageException(string.Format("{0} Font Size must be greater than 0.", font.target));
                    }
                }
            }

            if (game.symbols != null)
            {
                foreach (var symbol in game.symbols)
                {
                    // Check for valid attributes
                    if (String.IsNullOrWhiteSpace(symbol.id))
                    {
                        throw GenerateEmptyAttributeException("Symbol", "id");
                    }

                    if (String.IsNullOrWhiteSpace(symbol.src))
                    {
                        throw GenerateEmptyAttributeException("Symbol", "src");
                    }

                    path = Path.Combine(Directory.FullName, symbol.src);

                    if (!File.Exists(path))
                    {
                        throw GenerateFileDoesNotExistException(path, "symbol", symbol.name, "src", symbol.src);
                    }
                }
            }

            if (game.gameboards != null)
            {
                // Check for valid attributes
                if (String.IsNullOrWhiteSpace(game.gameboards.src))
                {
                    throw GenerateEmptyAttributeException("GameBoard", "src");
                }

                path = Path.Combine(Directory.FullName, game.gameboards.src);

                if (!File.Exists(path))
                {
                    throw GenerateFileDoesNotExistException(path, "Default Board", game.gameboards.name, "src", game.gameboards.src);
                }
                if (game.gameboards.gameboard != null)
                {
                    foreach (var board in game.gameboards.gameboard)
                    {
                        // Check for valid attributes
                        if (String.IsNullOrWhiteSpace(board.src))
                        {
                            throw GenerateEmptyAttributeException("GameBoard", "src");
                        }

                        path = Path.Combine(Directory.FullName, board.src);

                        if (!File.Exists(path))
                        {
                            throw GenerateFileDoesNotExistException(path, "Board", board.name, "src", board.src);
                        }
                    }
                }
            }

            if (game.markers != null)
            {
                foreach (var marker in game.markers)
                {
                    // Check for valid attributes
                    if (string.IsNullOrWhiteSpace(marker.id))
                    {
                        throw GenerateEmptyAttributeException("Marker", "id", marker.name);
                    }
                    var defaultMarkersList = new List<string>(Enumerable.Range(1, 10).Select(x => new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)x).ToString())); //TODO: Make the original static list accessible
                    if (defaultMarkersList.Contains(marker.id))
                    {
                        throw GenerateReservedAttributeValueException("Marker", "id", marker.id);
                    }
                    if (string.IsNullOrWhiteSpace(marker.src))
                    {
                        throw GenerateEmptyAttributeException("Marker", "src", marker.name);
                    }
                    // See if the paths specified exist
                    path = Path.Combine(Directory.FullName, marker.src);

                    if (File.Exists(path) == false)
                    {
                        throw GenerateFileDoesNotExistException(path, "Marker", marker.name, "src", marker.src);
                    }
                }
            }
            if (game.documents != null)
            {
                foreach (var doc in game.documents)
                {
                    // Check for valid attributes
                    if (String.IsNullOrWhiteSpace(doc.src))
                    {
                        throw GenerateEmptyAttributeException("Document", "src", doc.name);
                    }
                    // See if the paths specified exist
                    path = Path.Combine(Directory.FullName, doc.src);

                    if (File.Exists(path) == false)
                    {
                        throw GenerateFileDoesNotExistException(path, "Document", doc.name, "src", doc.src);
                    }
                    if (!String.IsNullOrWhiteSpace(doc.icon))
                    {
                        path = Path.Combine(Directory.FullName, doc.icon);

                        if (File.Exists(path) == false)
                        {
                            throw GenerateFileDoesNotExistException(path, "Document", doc.name, "icon", doc.icon);
                        }
                    }
                }
            }

            if (game.proxygen != null)
            {
                // Check for valid attributes
                if (String.IsNullOrWhiteSpace(game.proxygen.definitionsrc))
                {
                    throw GenerateEmptyAttributeException("ProxyGen", "definitionsrc");
                }

                path = Path.Combine(Directory.FullName, game.proxygen.definitionsrc);

                if (!File.Exists(path))
                {
                    throw GenerateFileDoesNotExistException(path, "Proxygen", game.proxygen.definitionsrc, "definitionsrc", game.proxygen.definitionsrc);
                }
            }

            //Test shortcuts
            if (game.table != null)
            {
                this.TestShortcut(game.table.shortcut);
                this.TestGroupsShortcuts(game.table.Items);
            }
            if (game.player != null)
            {
                foreach (var h in game.player.Items.OfType<hand>())
                {
                    this.TestShortcut(h.shortcut);
                    this.TestGroupsShortcuts(h.Items);
                }
                foreach (var g in game.player.Items.OfType<group>())
                {
                    this.TestShortcut(g.shortcut);
                    this.TestGroupsShortcuts(g.Items);
                }
            }

            // Verify card image paths
            if (String.IsNullOrWhiteSpace(game.card.front))
            {
                throw GenerateEmptyAttributeException("Card", "front");
            }

            if (String.IsNullOrWhiteSpace(game.card.back))
            {
                throw GenerateEmptyAttributeException("Card", "back");
            }

            path = Path.Combine(Directory.FullName, game.card.front);

            if (!File.Exists(path))
            {
                throw GenerateFileDoesNotExistException(path, "Card", "Default", "front", game.card.front);
            }

            path = Path.Combine(Directory.FullName, game.card.back);

            if (!File.Exists(path))
            {
                throw GenerateFileDoesNotExistException(path, "Card", "Default", "back", game.card.back);
            }

            if (game.card.size != null)
            {
                foreach (cardsizeDef sizeDef in game.card.size)
                {
                    if (String.IsNullOrWhiteSpace(sizeDef.back))
                    {
                        throw GenerateEmptyAttributeException("Card Size", "back");
                    }
                    
                    path = Path.Combine(Directory.FullName, sizeDef.back);

                    if (!File.Exists(path))
                    {
                        throw GenerateFileDoesNotExistException(path, "Card Size", sizeDef.name, "back", sizeDef.back);
                    }

                    if (String.IsNullOrWhiteSpace(sizeDef.front))
                    {
                        throw GenerateEmptyAttributeException("Card Size", "front");
                    }
        
                    path = Path.Combine(Directory.FullName, sizeDef.front);

                    if (!File.Exists(path))
                    {
                        throw GenerateFileDoesNotExistException(path, "Card Size", sizeDef.name, "front", sizeDef.front);
                    }
                }
            }

            if (game.card.property != null && game.card.property.Any())
            {
                List<string> props = new List<string>();
                foreach (var prop in game.card.property)
                {
                    if (!props.Contains(prop.name))
                    {
                        props.Add(prop.name);
                    }
                    else
                    {
                        throw new UserMessageException("Duplicate property defined named {0} in file: {1}", prop.name, Path.Combine(Directory.FullName, "defintion.xml"));
                    }
                }
                props.Clear();
            }

            // Check for valid attributes
            if (String.IsNullOrWhiteSpace(game.table.board) == false)
            {
                path = Path.Combine(Directory.FullName, game.table.board);

                if (!File.Exists(path))
                {
                    throw GenerateFileDoesNotExistException(path, "Table", "Default", "board", game.table.board);
                }
            }
            if (String.IsNullOrWhiteSpace(game.table.background))
            {
                throw GenerateEmptyAttributeException("Table", "background", game.table.name);
            }

            path = Path.Combine(Directory.FullName, game.table.background);

            if (!File.Exists(path))
            {
                throw GenerateFileDoesNotExistException(path, "Table", "Default", "Background", game.table.background);
            }
            //setup for deck section target testing.
            List<string> groups = new List<string>();

            if (game.player != null)
            {
                foreach (var counter in game.player.Items.OfType<counter>())
                {
                    // Check for valid attributes
                    if (String.IsNullOrWhiteSpace(counter.icon))
                    {
                        throw GenerateEmptyAttributeException("Counter", "icon", counter.name);
                    }

                    path = Path.Combine(Directory.FullName, counter.icon);

                    if (!File.Exists(path))
                    {
                        throw GenerateFileDoesNotExistException(path, "Counter", counter.name, "icon", counter.icon);
                    }
                }

                foreach (var group in game.player.Items.OfType<group>())
                {
                    // Check for valid attributes
                    if (String.IsNullOrWhiteSpace(group.icon))
                    {
                        throw GenerateEmptyAttributeException("Group", "icon", group.name);
                    }

                    path = Path.Combine(Directory.FullName, group.icon);

                    if (!File.Exists(path))
                    {
                        throw GenerateFileDoesNotExistException(path, "Group", group.name, "icon", group.icon);
                    }

                    groups.Add(group.name);
                }
            }

            //test deck sections for correct targets
            if (game.deck != null)
            {
                foreach (var deckSection in game.deck)
                {
                    string groupTarget = deckSection.group;
                    if (!groups.Contains(groupTarget))
                    {
                        throw new UserMessageException("Deck section group is invalid for section named: {0}",
                            deckSection.name);
                    }
                }
            }


            if (game.shared != null)
            {
                if (game.shared.counter != null)
                {
                    foreach (var counter in game.shared.counter)
                    {
                        // Check for valid attributes
                        if (String.IsNullOrWhiteSpace(counter.icon))
                        {
                            throw GenerateEmptyAttributeException("Global Counter", "icon", counter.name);
                        }

                        path = Path.Combine(Directory.FullName, counter.icon);

                        if (!File.Exists(path))
                        {
                            throw GenerateFileDoesNotExistException(path, "Global Counter", counter.name, "icon", counter.icon);
                        }
                    }
                }
                if (game.shared.group != null)
                {
                    foreach (var group in game.shared.group)
                    {
                        // Check for valid attributes
                        if (String.IsNullOrWhiteSpace(group.icon))
                        {
                            throw GenerateEmptyAttributeException("Global Group", "icon", group.name);
                        }

                        path = Path.Combine(Directory.FullName, group.icon);

                        if (!File.Exists(path))
                        {
                            throw GenerateFileDoesNotExistException(path, "Global Group", group.name, "icon", group.icon);
                        }
                    }
                }
            }

        }
        
        private void TestGroupsShortcuts(IEnumerable<baseAction> items)
        {
            if (items == null) return;
            foreach (var i in items)
            {
                if (i is groupAction)
                {
                    this.TestShortcut((i as groupAction).shortcut);
                }
                else if (i is groupActionSubmenu)
                {
                    this.TestGroupsShortcuts((i as groupActionSubmenu).Items);
                }
                else if (i is cardAction)
                {
                    this.TestShortcut((i as cardAction).shortcut);
                }
                else if (i is cardActionSubmenu)
                {
                    this.TestGroupsShortcuts((i as cardActionSubmenu).Items);
                }
            }
        }

        private static readonly KeyGestureConverter KeyConverter = new KeyGestureConverter();
        private void TestShortcut(string shortcut)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(shortcut)) return;
                var g = (KeyGesture)KeyConverter.ConvertFromInvariantString(shortcut);
            }
            catch (Exception)
            {
                throw new UserMessageException("Key combination '" + shortcut + "' is invalid");
            }

        }

        [GameValidatorAttribute]
        public void VerifyScripts()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(game));
            var fs = File.Open(Directory.GetFiles().First(x => x.Name == "definition.xml").FullName, FileMode.Open);
            var game = (game)serializer.Deserialize(fs);
            fs.Close();

            var engine = Python.CreateEngine(AppDomain.CurrentDomain);

            var errorList = new List<string>();

            foreach (var script in game.scripts)
            {
                var scr = script;
                var ss = engine.CreateScriptSourceFromFile(Path.Combine(Directory.FullName, scr.src));
                ss.Compile(new CompileErrorListener((source, message, span, code, severity) =>
                {
                    var errString = String.Format(
                        "[{0} {1}-{2}:{3} to {4}:{5}] {6} - {7}",
                        severity.ToString(),
                        code,
                        span.Start.Line,
                        span.Start.Column,
                        span.End.Line,
                        span.End.Column,
                        scr.src,
                        message);
                    errorList.Add(errString);
                }));
            }

            var sb = new StringBuilder("==Script Errors==");
            foreach (var err in errorList)
            {
                sb.AppendLine(err);
            }
            if (errorList.Count > 0)
                throw new UserMessageException(sb.ToString());
        }

        [GameValidatorAttribute]
        public void TestSetXml()
        {
            if (Directory.GetDirectories().Any(x => x.Name.ToLower() == "sets"))
            {
                var setDir = Directory.GetDirectories().First(x => x.Name.ToLower() == "sets");
                foreach (var dir in setDir.GetDirectories())
                {
                    var setFile = dir.GetFiles().First();
                    TestSetXml(setFile.FullName);
                    CheckSetXML(setFile.FullName);
                }
            }
        }

        public void TestSetXml(string filename)
        {
            var libAss = Assembly.GetAssembly(typeof(Paths));
            var setxsd = libAss.GetManifestResourceNames().FirstOrDefault(x => x.Contains("CardSet.xsd"));
            if (setxsd == null)
                throw new UserMessageException("ERROR: Cannot load schema CardSet.xsd");
            var schemas = new XmlSchemaSet();
            var schema = XmlSchema.Read(libAss.GetManifestResourceStream(setxsd), (sender, args) => { throw args.Exception; });
            schemas.Add(schema);

            XmlReaderSettings settings = GetXmlReaderSettings();
            settings.Schemas = schemas;
            settings.ValidationEventHandler += new ValidationEventHandler(delegate(Object o, ValidationEventArgs e)
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    throw new UserMessageException(string.Format("line: {0} position: {1} msg: {2} file: {3}", e.Exception.LineNumber, e.Exception.LinePosition, e.Exception.Message, filename));
                }
            });
            using (XmlReader validatingReader = XmlReader.Create(filename, settings))
            {
                while (validatingReader.Read()) { /* just loop through document */ }
            }
            //XDocument doc = XDocument.Load(filename);
            //string msg = "";
            //doc.Validate(schemas, (o, e) =>
            //{
            //    msg = string.Format("{0} line: {1} position: {2}", e.Message, e., e.Exception.LinePosition);
            //});
            //if (!string.IsNullOrWhiteSpace(msg))
            //    throw new UserMessageException(msg);
        }

        private XmlReaderSettings GetXmlReaderSettings()
        {
            XmlReaderSettings ret = new XmlReaderSettings();
            ret.ValidationType = ValidationType.Schema;
            ret.ValidationFlags = XmlSchemaValidationFlags.None;
            ret.CloseInput = true;
            ret.IgnoreWhitespace = true;
            ret.IgnoreComments = true;
            ret.IgnoreProcessingInstructions = true;
            return (ret);
        }

        public void VerifySetDirectory(DirectoryInfo dir)
        {
            var files = dir.GetFiles("*", SearchOption.TopDirectoryOnly);
            if (files.Length == 0)
                throw new UserMessageException("You must have a set.xml file inside of your set folder {0}", dir.FullName);
            if (files.Length > 1)
                throw new UserMessageException("You can only have a set.xml file in your set folder {0}", dir.FullName);
            var setFile = files.First();
            if (setFile.Name != "set.xml")
                throw new UserMessageException("You must have a set.xml file inside of your set folder {0}", dir.FullName);

            // Check folders...there should only be two if they exists
            var dirs = dir.GetDirectories("*", SearchOption.TopDirectoryOnly);
            if (dirs.Length > 2)
                throw new UserMessageException("You may only have a Cards and/or Markers folder in your set folder {0}", dir.FullName);
            if (!dirs.All(x => x.Name == "Cards" || x.Name == "Markers" || x.Name == "Decks"))
                throw new UserMessageException("You may only have a Cards, Markers, and/or Decks folder in your set folder {0}", dir.FullName);

            // check Cards directory. There should only be image files in there
            var cardDir = dirs.FirstOrDefault(x => x.Name == "Cards");
            if (cardDir != null)
            {
                var subDirs = cardDir.GetDirectories("*", SearchOption.TopDirectoryOnly);
                if(subDirs.Any(x => x.Name != "Crops"))
                    throw new UserMessageException("You can only have a Crops folder inside the Cards Folder {0}", cardDir.FullName);
                //if (cardDir.GetDirectories("*", SearchOption.AllDirectories))
                    //throw new UserMessageException("You cannot have any folders inside of the Cards folder {0}", cardDir.FullName);
                foreach (var f in cardDir.GetFiles("*", SearchOption.TopDirectoryOnly))
                {
                    var test = Guid.Empty;
                    if (!Guid.TryParse(f.Name.Substring(0, f.Name.IndexOf('.')), out test))
                        throw new UserMessageException("Your card file {0} was named incorrectly", f.FullName);
                }
            }
            var markersDir = dirs.FirstOrDefault(x => x.Name == "Markers");
            if (markersDir != null)
            {
                if (markersDir.GetDirectories("*", SearchOption.AllDirectories).Any())
                    throw new UserMessageException("You cannot have any folders inside of the Markers folder {0}", markersDir.FullName);
                foreach (var f in markersDir.GetFiles("*", SearchOption.TopDirectoryOnly))
                {
                    var test = Guid.Empty;
                    if (!Guid.TryParse(f.Name.Replace(f.Extension, ""), out test))
                        throw new UserMessageException("Your Marker file {0} was named incorrectly", f.FullName);
                }
            }
        }

        public void CheckSetXML(string fileName)
        {
            string definitionPath = Path.Combine(this.Directory.FullName, "definition.xml");
            var gameProperties = new List<Tuple<string, string>>();
            List<string> symbols = new List<string>();
            XmlDocument doc = new XmlDocument();
            doc.Load(definitionPath);
            XmlNode gameDef = doc.GetElementsByTagName("game").Item(0);
            string gameId = gameDef.Attributes["id"].Value?.ToLower();
            XmlNode cardDef = doc.GetElementsByTagName("card").Item(0);
            foreach (XmlNode propNode in cardDef.ChildNodes)
            {
                if (propNode.Name == "property")
                {
                    if (propNode.Attributes["name"] != null)
                    {
                        gameProperties.Add(new Tuple<string, string>(propNode.Attributes["name"].Value, propNode.Attributes["type"].Value));
                    }
                }
            }
            cardDef = null;
            XmlNode symbolDef = doc.GetElementsByTagName("symbols").Item(0);
            if (symbolDef != null)
            {
                foreach (XmlNode symbolNode in symbolDef.ChildNodes)
                {
                    if (symbolNode.Name == "symbol")
                    {
                        if (symbolNode.Attributes["id"] != null)
                        {
                            symbols.Add(symbolNode.Attributes["id"].Value);
                        }
                    }
                }
            }
            symbolDef = null;
            doc.RemoveAll();
            List<string> cardSizes = GetCardSizes(); 
            doc = null;
            doc = new XmlDocument();
            doc.Load(fileName);
            
            var setGameId = doc.GetElementsByTagName("set").Item(0).Attributes["gameId"].Value?.ToLower();
            var setName = doc.GetElementsByTagName("set").Item(0).Attributes["name"].Value;
            if (!gameId.Equals(setGameId))
            {
                throw new UserMessageException("the gameId value '{0}' does not match the game's GUID in set file '{1}'", setGameId, fileName);
            }

            if (doc.GetElementsByTagName("markers").Count > 0)
            {
                throw new UserMessageException("The set '{0}' from '{1}' contains markers, which are no longer supported. Markers must now be listed in the game definition.xml", setName, fileName);
            }
            
            foreach (XmlNode cardNode in doc.GetElementsByTagName("card"))
            {
                string cardName = cardNode.Attributes["name"].Value;
                List<string> cardProps = new List<string>();
                List<string> altTypes = new List<string>();
                if (cardNode.Attributes["size"] != null)
                {
                    string size = cardNode.Attributes["size"].Value;
                    if (!cardSizes.Contains(size))
                    {
                        throw new UserMessageException("Unknown size '{1}' defined on card '{0}' that is not defined in definition.xml in set file '{2}'", cardName, size, fileName);
                    }
                }
                foreach (XmlNode propNode in cardNode.ChildNodes)
                {
                    if (propNode.Name == "alternate")
                    {
                        string altName = propNode.Attributes["name"].Value;
                        string altType = propNode.Attributes["type"].Value;
                        if (altTypes.Contains(altType))
                        {
                            throw new UserMessageException("Duplicate alternate type '{0}' found on card '{1}' in set file '{2}'", altType, cardName, fileName);
                        }
                        altTypes.Add(altType);
                        if (propNode.Attributes["size"] != null)
                        {
                            string size = propNode.Attributes["size"].Value;
                            if (!cardSizes.Contains(size))
                            {
                                throw new UserMessageException("Unknown size '{1}' defined on card '{0}' that is not defined in definition.xml in set file '{2}'", cardName, size, fileName);
                            }
                        }
                        List<string> altProps = new List<string>();

                        foreach (XmlNode altPropNode in propNode.ChildNodes)
                        {
                            if (altPropNode.Name != "property")
                            {
                                throw new UserMessageException("Invalid child node '{0}' in card '{1}' alternate '{2}' in set file '{3}'", altPropNode.Name, cardName, altName, fileName);
                            }

                            string altPropName = altPropNode.Attributes["name"].Value;
                            if (altProps.Contains(altPropName))
                            {
                                throw new UserMessageException("Duplicate property '{0}' found on card '{1}' alternate '{2}' in set file '{3}'", altPropName, cardName, altName, fileName);
                            }
                            altProps.Add(altPropName);

                            var gameProp = gameProperties.FirstOrDefault(x => x.Item1 == altPropName);
                            if (gameProp == null)
                            {
                                throw new UserMessageException("Property '{2}' defined on card '{0}' alternate '{1}' is not defined in definition.xml in set file '{2}'", cardName, altName, altPropName, fileName);
                            }
                            if (gameProp.Item2 == "RichText")
                            {
                                if (altPropNode.Attributes["value"] != null)
                                {
                                    throw new UserMessageException("Property '{0}' defined on card '{1}' alternate '{2}' is defined as RichText and cannot have a value attribute in set file '{3}'", altPropName, cardName, altName, fileName);
                                }
                                var altError = CheckPropertyChildren(altPropNode, symbols);
                                if (altError != null)
                                {
                                    throw new UserMessageException("{0} found in card '{1}' alternate `{2}' property '{3}' in set file '{4}'", altError, cardName, altName, altPropNode.Attributes["name"].Value, fileName);
                                }
                            }
                            else
                            {
                                if (altPropNode.Attributes["value"] == null)
                                {
                                    throw new UserMessageException("Property '{0}' found in card '{1}' alternate '{2}' is missing the 'value' attribute in set file '{3}'", altPropName, cardName, altName, fileName);
                                }
                            }
                        }
                    }
                    else
                    {
                        var propName = propNode.Attributes["name"].Value;
                        if (cardProps.Contains(propName))
                        {
                            throw new UserMessageException("Duplicate property '{0}' found on card '{1}' in set file '{2}'", propNode.Attributes["name"].Value, cardName, fileName);
                        }
                        cardProps.Add(propName);

                        var gameProp = gameProperties.FirstOrDefault(x => x.Item1 == propName);
                        if (gameProp == null)
                        {
                            throw new UserMessageException("Property '{1}' defined on card '{0}' that is not defined in definition.xml in set file '{2}'", cardName, propName, fileName);
                        }
                        if (gameProp.Item2 == "RichText")
                        {
                            if (propNode.Attributes["value"] != null)
                            {
                                throw new UserMessageException("Property '{0}' defined on card '{1}' is defined as RichText and cannot have a value attribute in set file '{2}'", propName, cardName, fileName);
                            }
                            var error = CheckPropertyChildren(propNode, symbols);
                            if (error != null)
                            {
                                throw new UserMessageException("{0} found in card '{1}' property '{2}' in set file '{3}'", error, cardName, propNode.Attributes["name"].Value, fileName);
                            }
                        }
                        else
                        {
                            if (propNode.Attributes["value"] == null)
                            {
                                throw new UserMessageException("Property '{0}' found in card '{1}' is missing the 'value' attribute in set file '{2}'", propName, cardName, fileName);
                            }
                        }
                    }
                }
            }
            doc.RemoveAll();
            doc = null;
        }

        public string CheckPropertyChildren(XmlNode propValueNode, List<string> symbols)
        {
            if (propValueNode.HasChildNodes)
            {
                foreach (XmlNode childNode in propValueNode)
                {
                    if (childNode.NodeType == XmlNodeType.Text) continue;
                    if (childNode.Name.ToUpper() == "S" || childNode.Name.ToUpper() == "SYMBOL")
                    {
                        if (!symbols.Contains(childNode.Attributes["value"].Value))
                        {
                            return "Undefined Symbol '" + childNode.Attributes["value"].Value + "'";
                        }
                    }
                    if (childNode.Name.ToUpper() == "C" || childNode.Name.ToUpper() == "COLOR")
                    {
                        var color = childNode.Attributes["value"].Value;
                        var regexColorCode = new Regex("^#[a-fA-F0-9]{6}$");
                        if (!regexColorCode.IsMatch(color))
                        {
                            return "Invalid Color Code '" + color + "'";
                        }
                    }
                    else
                    {
                        var ret = CheckPropertyChildren(childNode, symbols);
                        if (ret != null)
                            return ret;
                    }
                }
            }
            return null;
        }
        
        public List<string> GetCardSizes()
        {
            List<string> ret = new List<string>();

            XmlSerializer serializer = new XmlSerializer(typeof(game));
            var fs = File.Open(Directory.GetFiles().First(x => x.Name == "definition.xml").FullName, FileMode.Open);
            var game = (game)serializer.Deserialize(fs);
            fs.Close();

            if (game.card.size != null)
            {
                foreach (cardsizeDef sizeDef in game.card.size)
                {
                    if (!ret.Contains(sizeDef.name))
                    {
                        ret.Add(sizeDef.name);
                    }
                }
            }

            return (ret);
        }


        [GameValidatorAttribute]
        public void VerifyProxyDef()
        {
            var libAss = Assembly.GetAssembly(typeof(Paths));
            var proxyxsd = libAss.GetManifestResourceNames().FirstOrDefault(x => x.Contains("CardGenerator.xsd"));
            if (proxyxsd == null)
                throw new UserMessageException("ERROR: Cannot load schema CardGenerator.xsd");
            var schemas = new XmlSchemaSet();
            var schema = XmlSchema.Read(libAss.GetManifestResourceStream(proxyxsd), (sender, args) => { throw args.Exception; });
            schemas.Add(schema);

            XmlSerializer serializer = new XmlSerializer(typeof(game));
            var fs = File.Open(Directory.GetFiles().First(x => x.Name == "definition.xml").FullName, FileMode.Open);
            var game = (game)serializer.Deserialize(fs);
            fs.Close();

            if (game.proxygen == null)
                throw new UserMessageException("You must have a ProxyGen element defined.");

            var fileName = Path.Combine(Directory.FullName, game.proxygen.definitionsrc);

            XDocument doc = XDocument.Load(fileName);
            string msg = "";
            doc.Validate(schemas, (o, e) =>
            {
                msg = e.Message;
            });
            if (!string.IsNullOrWhiteSpace(msg))
                throw new UserMessageException(msg);
        }

        [GameValidatorAttribute]
        public void VerifyProxyDefPaths()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(game));
            var fs = File.Open(Directory.GetFiles().First(x => x.Name == "definition.xml").FullName, FileMode.Open);
            var game = (game)serializer.Deserialize(fs);
            fs.Close();

            var proxyDef = Path.Combine(Directory.FullName, game.proxygen.definitionsrc);

            Dictionary<string, string> blockSources = ProxyDefinition.GetBlockSources(proxyDef);
            foreach (KeyValuePair<string, string> kvi in blockSources)
            {
                // Check for valid attributes
                if (String.IsNullOrWhiteSpace(kvi.Value))
                {
                    throw GenerateEmptyAttributeException("Block", "src", kvi.Key);
                }

                string path = Path.Combine(Directory.FullName, kvi.Value);

                if (!File.Exists(path))
                {
                    throw GenerateFileDoesNotExistException(path, "Block id: " + kvi.Key, "", "", kvi.Value);  //TODO: better description for exception
                }
            }

            List<string> templateSources = ProxyDefinition.GetTemplateSources(proxyDef);
            foreach (string source in templateSources)
            {
                string path = Path.Combine(Directory.FullName, source);

                if (!File.Exists(path))
                {
                    throw GenerateFileDoesNotExistException(path, "Proxy Template", "", "", source);  //TODO: better description for exception
                }
            }
        }

        public static void ValidateGameAttributes(game game)
        {
            if (String.IsNullOrWhiteSpace(game.name))
            {
                throw GameValidator.GenerateEmptyAttributeException("Game", "name");
            }

            if (String.IsNullOrWhiteSpace(game.id))
            {
                throw GameValidator.GenerateEmptyAttributeException("Game", "id", game.name);
            }

            if (String.IsNullOrWhiteSpace(game.gameurl))
            {
                throw GameValidator.GenerateEmptyAttributeException("Game", "gameurl", game.name);
            }

            if (String.IsNullOrWhiteSpace(game.version))
            {
                throw GameValidator.GenerateEmptyAttributeException("Game", "version", game.name);
            }

            if (String.IsNullOrWhiteSpace(game.iconurl))
            {
                throw GameValidator.GenerateEmptyAttributeException("Game", "iconurl", game.name);
            }

            if (game.scriptVersion == "0.0.0.0")
            {
                throw GameValidator.GenerateEmptyAttributeException("Game", "scriptVersion", game.name);
            }
        }


        #region Private Methods

        private static void TryThrow(Action a, string message, params object[] args)
        {
            try
            {
                a();
            }
            catch (Exception e)
            {
                throw new UserMessageException(string.Format(message, args), e);
            }
        }

        private static T TryThrow<T>(Func<T> a, string message, params object[] args)
        {
            try
            {
                return a();
            }
            catch (Exception e)
            {
                throw new UserMessageException(string.Format(message, args), e);
            }
        }

        /// <summary>
        /// This method throws an UserMessageException with the provided information to notify the user
        /// what file/path is misconfigured.
        /// </summary>
        /// <param name="fullPath">The full file path checked</param>
        /// <param name="elementType">The type of element that contains the invalid path</param>
        /// <param name="elementName">The name of the element that contains the invalid path</param>
        /// <param name="attributeName">The name of the attribute that contains the invalid path</param>
        /// <param name="providedPath">The path that was provided</param>
        /// <returns>A UserMessageException containing the generated message</returns>
        private static UserMessageException GenerateFileDoesNotExistException(string fullPath, string elementType, string elementName, string attributeName, string providedPath)
        {
            return new UserMessageException("File {0} does not exist for {1} '{2}' attribute '{3}'. '{4}' was provided.  Remember paths cannot start with / or \\",
                fullPath, elementType, elementName, attributeName, providedPath);
        }

        /// <summary>
        /// This method throws an UserMessageException with the provided information to notify the user
        /// that the supplied value is reserved by OCTGN.
        /// </summary>
        /// <param name="elementType">The name of the element that contains the invalid attribute</param>
        /// <param name="attributeType">The name of the invalid attribute</attributeName>
        /// <param name="attributeValue">The value of the invalid attribute</attributeName>
        /// <returns>A UserMessageException containing the generated message</returns>
        private static UserMessageException GenerateReservedAttributeValueException(string elementType, string attributeType, string attributeValue)
        {
            return new UserMessageException("'{0}' attribute '{1}' value '{2}' is invalid: Reserved for use by OCTGN.",
                elementType, attributeType, attributeValue);
        }

        /// <summary>
        /// This method throws an UserMessageException with the provided information to notify the user
        /// what attribute needs to be configured.
        /// </summary>
        /// <param name="elementType">The type of the element that contains the empty attribute</param>
        /// <param name="attributeName">The name of the empty attribute</attributeName>
        /// <returns>A UserMessageException containing the generated message</returns>
        private static UserMessageException GenerateEmptyAttributeException(string elementType, string attributeName)
        {
            return new UserMessageException("{0} has no value for {1} and it requires one to verify the package.", elementType, attributeName);
        }

        /// <summary>
        /// This method throws an UserMessageException with the provided information to notify the user
        /// what attribute needs to be configured.
        /// </summary>
        /// <param name="elementType">The type of the element that contains the empty attribute</param>
        /// <param name="attributeName">The name of the empty attribute</attributeName>
        /// <param name="elementName">The name of the element that contains the empty attribute</param>
        /// <returns>A UserMessageException containing the generated message</returns>
        private static UserMessageException GenerateEmptyAttributeException(string elementType, string attributeName, string elementName)
        {
            if (String.IsNullOrWhiteSpace(elementName))
            {
                return GenerateEmptyAttributeException(elementType, attributeName);
            }

            return new UserMessageException("{0} {2} has no value for {1} and it requires one to verify the package.", elementType,
                attributeName, elementName);
        }

        #endregion Private Methods


        internal class CompileErrorListener : ErrorListener
        {
            internal delegate void OnErrorDelegate(
                ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity);
            internal OnErrorDelegate OnError;
            public CompileErrorListener(OnErrorDelegate onError)
            {
                OnError = onError;
            }
            public override void ErrorReported(ScriptSource source, string message, SourceSpan span, int errorCode, Severity severity)
            {
                OnError.Invoke(source, message, span, errorCode, severity);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class GameValidatorAttribute : Attribute
    {

    }
}
