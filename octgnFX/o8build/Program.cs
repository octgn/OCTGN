using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace o8build
{
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Xml.Linq;
    using System.Xml.Schema;

    using Mono.Options;

    using Octgn.Library;

    using log4net;

    class Program
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static string BuildDirectory { get; set; }
        static bool BuildSet { get; set; }
        static bool BuildGame { get; set; }
        static bool GetHelp { get; set; }
        static void Main(string[] args)
        {
            try
            {
                HandleArguments(args);
                Start();
            }
            catch (UserMessageException e)
            {
                UserError(e.Message);
                WriteOptions();
                Environment.ExitCode = -1;

            }
            catch (Exception e)
            {
                Log.Fatal("Unknown error of doom",e);
                WriteOptions();
                Environment.ExitCode = -2;
            }
            
            try
            {
                // Will throw an exception if the console can't accept input
                // so we can avoid hanging on a key input if it's used in
                // some kind of program.
                var tits = Console.KeyAvailable;
                Debug.WriteLine(tits);
                Console.WriteLine();
                Console.WriteLine("== Press any key to quite ==");
                Console.ReadKey();
            }
            catch
            {
                
            }
        }

        private static void Start()
        {
            testXsd();
        }

        private static void testXsd()
        {
            var dp = Paths.DataDirectory;

            var libAss = Assembly.GetAssembly(typeof(Paths));
            var gamexsd = libAss.GetManifestResourceNames().FirstOrDefault(x => x.Contains("Game.xsd"));
            if(gamexsd == null)
                throw new Octgn.Library.Exceptions.UserMessageException("Shits fucked bro.");
            var schemeString = "";
            using (var sr = new StreamReader(libAss.GetManifestResourceStream(gamexsd))) schemeString = sr.ReadToEnd();
            var schemas = new XmlSchemaSet();
            var schema  = XmlSchema.Read(libAss.GetManifestResourceStream(gamexsd), OnValidationEventHandler);
            schemas.Add(schema);

            var gamesDir = new DirectoryInfo(Path.Combine(dp, "Games"));
            foreach (var file in gamesDir.GetDirectories().SelectMany(x=>x.GetFiles("*.xml").Where(y=>y.Name.ToLower() != "[content_types].xml")))
            {
                XDocument doc = XDocument.Load(file.FullName);
                string msg = "";
                doc.Validate(schemas, (o, e) =>
                {
                    msg = e.Message;
                });
                Console.WriteLine(msg == "" ? "Document {0} is valid" : "Document {0} invalid: " + msg,file.Name);
            }
        }

        private static void OnValidationEventHandler(object sender, ValidationEventArgs args)
        {
            Log.Error(args.Message,args.Exception);
        }

        private static OptionSet GetOptions()
        {
            var ret =  new OptionSet();
            ret.Add("d|directory=", "Directory of the game/set to build", x => BuildDirectory = x);
            ret.Add("s|set", "Build a set", x => BuildSet = true);
            ret.Add("g|game", "Build a game", x => BuildGame = true);
            ret.Add("?|help", "Get help cause you're a real confused guy.", x => GetHelp = true);
            return ret;
        }

        private static void HandleArguments(string[] args)
        {
            var options = GetOptions();

            options.Parse(args);

            if (GetHelp)
            {
                WriteOptions();
            }

            if (String.IsNullOrWhiteSpace(BuildDirectory)) BuildDirectory = System.IO.Directory.GetCurrentDirectory();
            if (!Directory.Exists(BuildDirectory)) throw new UserMessageException("Directory {0} does not exist you FOOL!", BuildDirectory);

            if(BuildSet && BuildGame)
                throw new UserMessageException("Don't be dumb. You can't set -s AND -g at the same time!");

            if(BuildSet == false && BuildGame == false)
                throw new UserMessageException("Listen. I know it's late, but you need to pick either -g or -s.");
        }

        public static void UserError(string message, params object[] args)
        {
            var oc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
            Console.WriteLine(message, args);
            Console.WriteLine("♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
            Console.WriteLine();
            Console.ForegroundColor = oc;
        }

        public static void WriteOptions()
        {
            var oc = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
            GetOptions().WriteOptionDescriptions(Console.Out);
            Console.WriteLine("♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥♥");
            Console.WriteLine();
            Console.ForegroundColor = oc;
        }
    }
}
