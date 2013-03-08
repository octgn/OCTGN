using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace o8build
{
    using System.IO;
    using System.Reflection;

    using Mono.Options;

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
                var tits = Console.KeyAvailable;
                Console.WriteLine();
                Console.WriteLine("== Press any key to quite ==");
                Console.ReadKey();
            }
            catch
            {
                
            }
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
