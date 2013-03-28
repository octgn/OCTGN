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
    using System.Xml.Serialization;

    using Mono.Options;

    using NuGet;

    using Octgn.Library;
    using Octgn.Library.Exceptions;

    using log4net;

    class Program
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static string BuildDirectory { get; set; }
        static bool GetHelp { get; set; }
        static bool Validate { get; set; }
        static bool LocalFeedInstall { get; set; }
        static string O8gPath { get; set; }
        static string NupkgPath { get; set; }
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
            var gv = new GameValidator(BuildDirectory);
            Console.WriteLine("Running tests on {0}",BuildDirectory);
            gv.RunTests();
            if (Validate) return;
            Console.WriteLine("Building packages");
            BuildPackages();
            if (LocalFeedInstall)
                InstallLocalFeed();
            //testXsd();
        }

        private static void InstallLocalFeed()
        {
            Console.WriteLine("Installing to local feed at {0}",Paths.Get().LocalFeedPath);
            var fi = new FileInfo(NupkgPath);
            var newPath = Path.Combine(Paths.Get().LocalFeedPath,fi.Name);
            File.Copy(NupkgPath,newPath);
            Console.WriteLine("Installed to local feed at {0}",newPath);
        }

        private static void BuildPackages()
        {
            var directory = new DirectoryInfo(BuildDirectory);
            XmlSerializer serializer = new XmlSerializer(typeof(game));
            var fs = File.Open(directory.GetFiles().First().FullName, FileMode.Open);
            var game = (game)serializer.Deserialize(fs);
            fs.Close();
            var builder = new NuGet.PackageBuilder()
                              {
                                  Id = game.id,
                                  Description = game.description,
                                  ProjectUrl = new Uri(game.gameurl),
                                  Version = new SemanticVersion(game.version),
                                  Title = game.name,
                                  IconUrl = new Uri(game.iconurl),
                              };
            foreach (var author in game.authors.Split(',')) builder.Authors.Add(author);
            foreach (var tag in game.tags.Split(' ')) builder.Tags.Add(tag);
            // files and maybe release notes
            var allFiles = directory
                .GetFiles("*.*", SearchOption.AllDirectories)
                .Where(x=>x.Extension.ToLower() != ".nupkg" && x.Extension.ToLower() != ".o8g");
            foreach (var file in allFiles)
            {
                var path = file.FullName;
                var relPath = path.Replace(directory.FullName, "\\def");
                var pf = new PhysicalPackageFile() { SourcePath = path, TargetPath = relPath };

                builder.Files.Add(pf);
            }
            var feedPath = Path.Combine(directory.FullName, game.name + '-' + game.version + ".nupkg");
            var olPath = Path.Combine(directory.FullName, game.name + '-' + game.version + ".o8g");
            O8gPath = olPath;
            NupkgPath = feedPath;
            Console.WriteLine("Feed Path: " + feedPath);
            Console.WriteLine("Manual Path: " + olPath);
            var filestream = File.Open(feedPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            builder.Save(filestream);
            filestream.Flush(true);
            filestream.Close();
            filestream = File.Open(olPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            builder.Save(filestream);
            filestream.Flush(true);
            filestream.Close();
        }

        private static OptionSet GetOptions()
        {
            var ret =  new OptionSet();
            ret.Add("d|directory=", "Directory of the game/set to build", x => BuildDirectory = x);
            //ret.Add("s|set", "Build a set", x => BuildSet = true);
            //ret.Add("g|game", "Build a game", x => BuildGame = true);
            ret.Add("v|validate", "Only validate the game, don't build the packages", x => Validate = true);
            ret.Add("i|install", "Installs package into the local octgn feed",x=>LocalFeedInstall = true);
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

            //if(BuildSet && BuildGame)
            //    throw new UserMessageException("Don't be dumb. You can't set -s AND -g at the same time!");

            //if(BuildSet == false && BuildGame == false)
            //    throw new UserMessageException("Listen. I know it's late, but you need to pick either -g or -s.");
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
