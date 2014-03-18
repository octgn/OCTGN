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

    public class GameValidator
    {
        internal DirectoryInfo Directory { get; set; }

        public GameValidator(string directory)
        {
            Directory = new DirectoryInfo(directory);
        }

        public void RunTests()
        {
            var tests = typeof(GameValidator)
                .GetMethods()
                .Where(x => x.GetCustomAttributes(typeof(GameValidatorAttribute),true).Any());

            foreach (var test in tests)
            {
                Console.WriteLine("Running Test {0}",test.Name);
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
            if(Directory.GetDirectories().Any(x=>x.Name == "_rels"))
                throw new UserMessageException("The _rels folder is depreciated.");
            if (Directory.GetDirectories().Any(x => x.Name == "Sets"))
            {
                var setDir = Directory.GetDirectories().First(x => x.Name == "Sets");
                if(setDir.GetFiles("*",SearchOption.TopDirectoryOnly).Any())
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
                throw new UserMessageException("Shits fucked bro.");
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
            const string gError = "{0} {1} does not exist here {1}. Remember paths cannot start with / or \\";
            XmlSerializer serializer = new XmlSerializer(typeof(game));
            var fs = File.Open(Directory.GetFiles().First(x => x.Name == "definition.xml").FullName, FileMode.Open);
            var game = (game)serializer.Deserialize(fs);
            fs.Close();
            var path = "";

            foreach (var s in game.scripts)
            {
				if (string.IsNullOrWhiteSpace(s.src))
				{
				    throw new UserMessageException("Scripts `src` attribute can't be empty.");
				}
                path = Path.Combine(Directory.FullName, s.src.Trim('\\', '/'));
                if (!File.Exists(path))
                    throw new UserMessageException(gError, "Script file", s.src, path);
            }

            if (game.fonts != null)
            {
                foreach (var font in game.fonts)
                {
					if(String.IsNullOrWhiteSpace(font.src))
						throw new UserMessageException("Fonts `src` attribute can't be empty.");
                    path = Path.Combine(Directory.FullName, font.src);
                    if (!File.Exists(path))
                        throw new UserMessageException(gError, "Font", font.src, path);
                }
            }

            if (game.documents != null)
            {
                foreach (var doc in game.documents)
                {
					if(String.IsNullOrWhiteSpace(doc.src))
						throw new UserMessageException("Documents `src` attribute can't be empty.");
					if(String.IsNullOrWhiteSpace(doc.icon))
						throw new UserMessageException("Documents `icon` attribute can't be empty.");
                    path = Path.Combine(Directory.FullName, doc.src);
                    if (!File.Exists(path))
                        throw new UserMessageException(gError, "Document", doc.src, path);
                    path = Path.Combine(Directory.FullName, doc.icon);
                    if (!File.Exists(path))
                        throw new UserMessageException(gError, "Document", doc.icon, path);

                }
            }
            if (game.proxygen != null)
            {
                if (String.IsNullOrWhiteSpace(game.proxygen.definitionsrc))
					throw new UserMessageException("Proxygens `definitionsrc` attribute can't be empty.");
                path = Path.Combine(Directory.FullName, game.proxygen.definitionsrc);
                if (!File.Exists(path))
                    throw new UserMessageException(gError, "ProxyGen", game.proxygen.definitionsrc, path);
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
                throw new UserMessageException("Games `front` attribute can't be empty.");
            if (String.IsNullOrWhiteSpace(game.card.back))
                throw new UserMessageException("Games `back` attribute can't be empty.");
            path = Path.Combine(Directory.FullName, game.card.front);
            if (!File.Exists(path))
                throw new UserMessageException(gError, "Card front", game.card.front, path);
            path = Path.Combine(Directory.FullName, game.card.back);
            if (!File.Exists(path))
                throw new UserMessageException(gError, "Card back", game.card.back, path);

            if (game.card.property.Length > 0)
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

            if (!string.IsNullOrWhiteSpace(game.table.board))
            {
                if (String.IsNullOrWhiteSpace(game.table.board))
					throw new UserMessageException("Tables `board` attribute can't be empty.");
                path = Path.Combine(Directory.FullName, game.table.board);
                if (!File.Exists(path)) throw new UserMessageException(gError, "Table board", game.table.board, path);
            }

            if (String.IsNullOrWhiteSpace(game.table.background))
                throw new UserMessageException("Tables `background` attribute can't be empty.");
            path = Path.Combine(Directory.FullName, game.table.background);
            if (!File.Exists(path))
                throw new UserMessageException(gError, "Table background", game.table.background, path);
            if (game.player != null)
            {
                foreach (var counter in game.player.Items.OfType<counter>())
                {
                    if (String.IsNullOrWhiteSpace(counter.icon))
                        throw new UserMessageException("Counters `icon` attribute can't be empty.");
                    path = Path.Combine(Directory.FullName, counter.icon);
                    if (!File.Exists(path))
                        throw new UserMessageException(gError, "Counter icon", counter.icon, path);
                }
                foreach (var hand in game.player.Items.OfType<group>())
                {
                    if (String.IsNullOrWhiteSpace(hand.icon))
                        throw new UserMessageException("Hands `icon` attribute can't be empty.");
                    path = Path.Combine(Directory.FullName, hand.icon);
                    if (!File.Exists(path))
                        throw new UserMessageException(gError, "Group " + hand.name, hand.icon, path);
                }
            }
            if (game.shared != null)
            {
                if (game.shared.counter != null)
                {
                    foreach (var counter in game.shared.counter)
                    {
                        if (String.IsNullOrWhiteSpace(counter.icon))
                            throw new UserMessageException("Counters `icon` attribute can't be empty.");
                        path = Path.Combine(Directory.FullName, counter.icon);
                        if (!File.Exists(path)) throw new UserMessageException(gError, "Counter icon", counter.icon, path);
                    }
                }
                if (game.shared.group != null)
                {
                    foreach (var hand in game.shared.group)
                    {
                        if (String.IsNullOrWhiteSpace(hand.icon))
                            throw new UserMessageException("Hands `icon` attribute can't be empty.");
                        path = Path.Combine(Directory.FullName, hand.icon);
                        if (!File.Exists(path)) throw new UserMessageException(gError, "Group " + hand.name, hand.icon, path);
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
            if (Directory.GetDirectories().Any(x => x.Name == "Sets"))
            {
                var setDir = Directory.GetDirectories().First(x => x.Name == "Sets");
                foreach (var dir in setDir.GetDirectories())
                {
                    var setFile = dir.GetFiles().First();
                    TestSetXml(setFile.FullName);
                    CheckSetXML(setFile.FullName);
                    CheckMarkerPaths(setFile.FullName);
                }
            }
        }

        public void TestSetXml(string filename)
        {
            var libAss = Assembly.GetAssembly(typeof(Paths));
            var setxsd = libAss.GetManifestResourceNames().FirstOrDefault(x => x.Contains("CardSet.xsd"));
            if (setxsd == null)
                throw new UserMessageException("Shits fucked bro.");
            var schemas = new XmlSchemaSet();
            var schema = XmlSchema.Read(libAss.GetManifestResourceStream(setxsd), (sender, args) => { throw args.Exception; });
            schemas.Add(schema);

            XmlReaderSettings settings = GetXmlReaderSettings();
            settings.Schemas = schemas;
            settings.ValidationEventHandler += new ValidationEventHandler(delegate (Object o, ValidationEventArgs e) {
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
            if(files.Length == 0)
                throw new UserMessageException("You must have a set.xml file inside of your set folder {0}",dir.FullName);
            if(files.Length > 1)
                throw new UserMessageException("You can only have a set.xml file in your set folder {0}",dir.FullName);
            var setFile = files.First();
            if(setFile.Name != "set.xml")
                throw new UserMessageException("You must have a set.xml file inside of your set folder {0}",dir.FullName);

            // Check folders...there should only be two if they exists
            var dirs = dir.GetDirectories("*", SearchOption.TopDirectoryOnly);
            if(dirs.Length > 2)
                throw new UserMessageException("You may only have a Cards and/or Markers folder in your set folder {0}",dir.FullName);
            if(!dirs.All(x=>x.Name == "Cards" || x.Name == "Markers" || x.Name == "Decks"))
                throw new UserMessageException("You may only have a Cards, Markers, and/or Decks folder in your set folder {0}", dir.FullName);

            // check Cards directory. There should only be image files in there
            var cardDir = dirs.FirstOrDefault(x => x.Name == "Cards");
            if (cardDir != null)
            {
                if(cardDir.GetDirectories("*",SearchOption.AllDirectories).Any())
                    throw new UserMessageException("You cannot have any folders inside of the Cards folder {0}",cardDir.FullName);
                foreach (var f in cardDir.GetFiles("*",SearchOption.TopDirectoryOnly))
                {
                    var test = Guid.Empty;
                    if(!Guid.TryParse(f.Name.Substring(0,f.Name.IndexOf('.')),out test))
                        throw new UserMessageException("Your card file {0} was named incorrectly",f.FullName);
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
                    if(!Guid.TryParse(f.Name.Replace(f.Extension,""),out test))
                        throw new UserMessageException("Your Marker file {0} was named incorrectly", f.FullName);
                }
            }
        }

        public void CheckSetXML(string fileName)
        {
            string definitionPath = Path.Combine(this.Directory.FullName, "definition.xml");
            List<string> properties = new List<string>();
            XmlDocument doc = new XmlDocument();
            doc.Load(definitionPath);
            XmlNode cardDef = doc.GetElementsByTagName("card").Item(0);
            foreach (XmlNode propNode in cardDef.ChildNodes)
            {
                if (propNode.Name == "property")
                {
                    if (propNode.Attributes["name"] != null)
                    {
                        properties.Add(propNode.Attributes["name"].Value);
                    }
                }
            }
            cardDef = null;
            doc.RemoveAll();
            doc = null;
            doc = new XmlDocument();
            doc.Load(fileName);
            foreach (XmlNode cardNode in doc.GetElementsByTagName("card"))
            {
                string cardName = cardNode.Attributes["name"].Value;
                List<string> cardProps = new List<string>();
                foreach (XmlNode propNode in cardNode.ChildNodes)
                {
                    if (propNode.Name == "alternate")
                    {
                        string altName = propNode.Attributes["name"].Value;
                        List<string> props = new List<string>();
                        foreach (XmlNode altPropNode in propNode.ChildNodes)
                        {
                            string prop = altPropNode.Attributes["name"].Value;
                            if (!props.Contains(prop))
                            {
                                props.Add(prop);
                            }
                            else
                            {
                                throw new UserMessageException("Duplicate property found named {0} on card named {1} within alternate {2} in set file {3}",prop, cardName, altName, fileName);
                            }
                        }
                        foreach (string prop in props)
                        {
                            if (!properties.Contains(prop))
                            {
                                throw new UserMessageException("Property defined on card {0} alternate with name {1} named {2} is not defined in definition.xml in set file {2}", cardName, altName, prop, fileName);
                            }
                        }
                        continue;
                    }
                    if (!cardProps.Contains(propNode.Attributes["name"].Value))
                    {
                        cardProps.Add(propNode.Attributes["name"].Value);
                    }
                    else
                    {
                        throw new UserMessageException("Duplicate property found named {0} on card named {1} in set file {2}", propNode.Attributes["name"].Value, cardName, fileName);
                    }
                }
                foreach (string prop in cardProps)
                {
                    if (!properties.Contains(prop))
                    {
                        throw new UserMessageException("Property defined on card name {0} named {1} that is not defined in definition.xml in set file {2}", cardName, prop, fileName);
                    }
                }
            }
            doc.RemoveAll();
            doc = null;
        }

        public void CheckMarkerPaths(string fileName)
        {
            DirectoryInfo markerDir = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(fileName), "Markers"));
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            if (doc.GetElementsByTagName("markers").Count == 0) return;
            XmlNodeList markerList = doc.GetElementsByTagName("marker");
            if (markerList.Count > 0)
            {
                foreach (XmlNode node in markerList)
                {
                    if(node.Attributes == null || node.Attributes["id"] == null)
                        throw new UserMessageException("Marker id doesn't exist for 'Marker' element in file {1}", fileName);
                    if (node.Attributes == null || node.Attributes["name"] == null)
                        throw new UserMessageException("Marker name doesn't exist for 'Marker' element in file {1}", fileName);
                    string id = node.Attributes["id"].Value;
                    string name = node.Attributes["name"].Value;
                    FileInfo[] f = markerDir.GetFiles(string.Format("{0}.*", id), SearchOption.TopDirectoryOnly);
                    if (f.Length == 0)
                    {
                        throw new UserMessageException("Marker image not found with id {0} and name {1} in file {2}", id, name, fileName);
                    }
                }
            }
            doc.RemoveAll();
            doc = null;
        }


        [GameValidatorAttribute]
        public void VerifyProxyDef()
        {
            var libAss = Assembly.GetAssembly(typeof(Paths));
            var proxyxsd = libAss.GetManifestResourceNames().FirstOrDefault(x => x.Contains("CardGenerator.xsd"));
            if (proxyxsd == null)
                throw new UserMessageException("Shits fucked bro.");
            var schemas = new XmlSchemaSet();
            var schema = XmlSchema.Read(libAss.GetManifestResourceStream(proxyxsd), (sender, args) => { throw args.Exception; });
            schemas.Add(schema);

            XmlSerializer serializer = new XmlSerializer(typeof(game));
            var fs = File.Open(Directory.GetFiles().First(x => x.Name == "definition.xml").FullName, FileMode.Open);
            var game = (game)serializer.Deserialize(fs);
            fs.Close();

            if(game.proxygen == null)
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
            const string gError = "{0} {1} does not exist here {1}. Remember paths cannot start with / or \\";
            XmlSerializer serializer = new XmlSerializer(typeof(game));
            var fs = File.Open(Directory.GetFiles().First(x => x.Name == "definition.xml").FullName, FileMode.Open);
            var game = (game)serializer.Deserialize(fs);
            fs.Close();

            var proxyDef = Path.Combine(Directory.FullName, game.proxygen.definitionsrc);

            Dictionary<string, string> blockSources = ProxyDefinition.GetBlockSources(proxyDef);
            foreach (KeyValuePair<string, string> kvi in blockSources)
            {
                string path = Path.Combine(Directory.FullName, kvi.Value);
                if (!File.Exists(path))
                {
                    throw new UserMessageException(gError, "Block id: " + kvi.Key, "src: " + kvi.Value, path);
                }
            }

            List<string> templateSources = ProxyDefinition.GetTemplateSources(proxyDef);
            foreach (string source in templateSources)
            {
                string path = Path.Combine(Directory.FullName, source);
                if (!File.Exists(path))
                {
                    throw new UserMessageException(gError, "Template", "src: " + source, path);
                }
            }
        }

        

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
                OnError.Invoke(source,message,span,errorCode,severity);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    internal class GameValidatorAttribute : Attribute
    {
        
    }
}
