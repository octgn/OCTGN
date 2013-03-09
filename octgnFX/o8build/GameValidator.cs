namespace o8build
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    using IronPython.Hosting;

    using Microsoft.Scripting;
    using Microsoft.Scripting.Hosting;

    using Octgn.Library;
    using Octgn.Library.Exceptions;

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
                throw new UserMessageException("You can only have 1 file in the root of your game directory.");
            if (Directory.GetFiles().Where(x => x.Extension.ToLower() != ".nupkg" && x.Extension.ToLower() != ".o8g").First().Name != "definition.xml")
                throw new UserMessageException("You must have a definition.xml in the root of your game directory.");
            if(Directory.GetDirectories().Any(x=>x.Name == "_rels"))
                throw new UserMessageException("The _rels folder is depreciated.");
            if (Directory.GetDirectories().Any(x => x.Name == "Sets"))
            {
                var setDir = Directory.GetDirectories().First(x => x.Name == "Sets");
                if(setDir.GetFiles("*.o8s").Length != setDir.GetFiles().Length)
                    throw new UserMessageException("You can only have .o8s files in the Sets folder.");
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
            var schema = XmlSchema.Read(libAss.GetManifestResourceStream(gamexsd), (sender, args) =>{ throw args.Exception; });
            schemas.Add(schema);

            var fileName = Directory.GetFiles().First().FullName;
            XDocument doc = XDocument.Load(fileName);
            string msg = "";
            doc.Validate(schemas, (o, e) =>
            {
                msg = e.Message;
            });
            if(!string.IsNullOrWhiteSpace(msg))
                throw new UserMessageException(msg);
        }

        [GameValidatorAttribute]
        public void VerifyDefPaths()
        {
            const string gError = "{0} {1} does not exist here {1}. Remember paths cannot start with / or \\";
            XmlSerializer serializer = new XmlSerializer(typeof(game));
            var fs = File.Open(Directory.GetFiles().First().FullName, FileMode.Open);
            var game = (game)serializer.Deserialize(fs);
            fs.Close();
            var path = "";

            foreach (var s in game.scripts)
            {
                path = Path.Combine(Directory.FullName,s.src.Trim('\\','/'));
                if(!File.Exists(path))
                    throw new UserMessageException(gError,"Script file",s.src,path);
            }

            if (game.fonts != null)
            {
                foreach (var font in game.fonts)
                {
                    path = Path.Combine(Directory.FullName, font.src);
                    if (!File.Exists(path))
                        throw new UserMessageException(gError, "Font", font.src, path);
                }
            }

            path = Path.Combine(Directory.FullName, game.card.front);
            if(!File.Exists(path))
                throw new UserMessageException(gError,"Card front",game.card.front,path);
            path = Path.Combine(Directory.FullName, game.card.back);
            if(!File.Exists(path))
                throw new UserMessageException(gError,"Card back", game.card.back, path);

            if (!string.IsNullOrWhiteSpace(game.table.board))
            {
                path = Path.Combine(Directory.FullName, game.table.board);
                if (!File.Exists(path)) throw new UserMessageException(gError, "Table board", game.table.board, path);
            }

            path = Path.Combine(Directory.FullName, game.table.background);
            if(!File.Exists(path))
                throw new UserMessageException(gError,"Table background",game.table.background,path);
            if (game.player != null)
            {
                foreach (var counter in game.player.Items.OfType<counter>())
                {
                    path = Path.Combine(Directory.FullName, counter.icon);
                    if(!File.Exists(path))
                        throw new UserMessageException(gError,"Counter icon",counter.icon,path);
                }
                foreach (var hand in game.player.Items.OfType<group>())
                {
                    path = Path.Combine(Directory.FullName, hand.icon);
                    if(!File.Exists(path))
                        throw new UserMessageException(gError,"Group " + hand.name,hand.icon,path);
                }
            }
            if (game.shared!= null)
            {
                if (game.shared.counter != null)
                {
                    foreach (var counter in game.shared.counter)
                    {
                        path = Path.Combine(Directory.FullName, counter.icon);
                        if (!File.Exists(path)) throw new UserMessageException(gError, "Counter icon", counter.icon, path);
                    }
                }
                if (game.shared.group != null)
                {
                    foreach (var hand in game.shared.group)
                    {
                        path = Path.Combine(Directory.FullName, hand.icon);
                        if (!File.Exists(path)) throw new UserMessageException(gError, "Group " + hand.name, hand.icon, path);
                    }
                }
            }
        }

        [GameValidatorAttribute]
        public void VerifyScripts()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(game));
            var fs = File.Open(Directory.GetFiles().First().FullName, FileMode.Open);
            var game = (game)serializer.Deserialize(fs);
            fs.Close();

            var engine = Python.CreateEngine(AppDomain.CurrentDomain);

            var errorList = new List<string>();

            foreach (var script in game.scripts)
            {
                var scr = script;
                var ss = engine.CreateScriptSourceFromFile(Path.Combine(Directory.FullName,scr.src));
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