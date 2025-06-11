using Octgn.Library.Exceptions;

namespace Octgn
{
    using System;
    using System.IO;
    using System.Reflection;

    using log4net;

    using Octgn.Launchers;

    public class CommandLineHandler
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
        #region Singleton

        internal static CommandLineHandler SingletonContext { get; set; }

        private static readonly object CommandLineHandlerSingletonLocker = new object();

        public static CommandLineHandler Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (CommandLineHandlerSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new CommandLineHandler();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        public bool DevMode { get; private set; }
        
        public bool SkipUpdate { get; private set; }

        public bool ShowHelp { get; private set; }

		public bool ShutdownProgram { get; private set; }

        public ILauncher HandleArguments(string[] args)
        {
            try
            {
                if (args != null) Log.Debug(string.Join(Environment.NewLine, args));
                if (args.Length < 2) return new MainLauncher();
                Log.Info("Has arguments");
                if (args[1].StartsWith("octgn://", StringComparison.InvariantCultureIgnoreCase))
                {
                    Log.Info("Using protocol");
                    var uri = new Uri(args[1]);
                    return HandleProtocol(uri);
                }
                var tableOnly = false;
                var editorOnly = false;
                int? hostport = null;
                Guid? gameid = null;
                string deckPath = null;
                var os = new Mono.Options.OptionSet()
                {
                    {"t|table", "Launch table only mode", x => tableOnly = true},
                    {"g|game=", "Specify game GUID", x => gameid = Guid.Parse(x)},
                    {"d|deck=", "Specify deck path", x => deckPath = x},
                    {"x|devmode", "Enable developer mode", x => DevMode = true},
                    {"n|no-update|skip-update", "Skip automatic update check", x => SkipUpdate = true},
                    {"e|editor", "Launch deck editor only", x => editorOnly = true},
                    {"h|help", "Show this help message", x => ShowHelp = true}
                };
                try
                {
                    os.Parse(args);
                }
                catch (Exception e)
                {
                    Log.Warn("Parse args exception: " + String.Join(",", Environment.GetCommandLineArgs()), e);
                }

                if (ShowHelp)
                {
                    Log.Info("Command line help requested");
                    ShowCommandLineHelp(os);
                    ShutdownProgram = true;
                    return new MainLauncher();
                }
                if (tableOnly)
                {
                    return new TableLauncher(hostport, gameid);
                }

                if (File.Exists(args[1]))
                {
                    Log.Info("Argument is file");
                    var fi = new FileInfo(args[1]);
                    if (fi.Extension.Equals(".o8d", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Log.Info("Argument is deck");
                        deckPath = args[1];
                    }
                }

                if (deckPath != null || editorOnly)
                {
                    return new DeckEditorLauncher(deckPath, true);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error handling arguments", e);
                if (args != null) Log.Error(string.Join(Environment.NewLine, args));
            }
			return new MainLauncher();
        }

        private void ShowCommandLineHelp(Mono.Options.OptionSet options)
        {
            System.Console.WriteLine("OCTGN - Online Card and Tabletop Gaming Network");
            System.Console.WriteLine("Usage: OCTGN.exe [options] [file]");
            System.Console.WriteLine();
            System.Console.WriteLine("Options:");
            options.WriteOptionDescriptions(System.Console.Out);
            System.Console.WriteLine();
            System.Console.WriteLine("Examples:");
            System.Console.WriteLine("  OCTGN.exe --skip-update          Skip the automatic update check");
            System.Console.WriteLine("  OCTGN.exe -n                     Skip the automatic update check (short form)");
            System.Console.WriteLine("  OCTGN.exe --devmode              Enable developer mode");
            System.Console.WriteLine("  OCTGN.exe --editor               Launch the deck editor only");
            System.Console.WriteLine("  OCTGN.exe mydeck.o8d              Open a deck file");
            System.Console.WriteLine();
        }

        internal ILauncher HandleProtocol(Uri url)
        {
            var host = url.Host.ToLowerInvariant();
            switch (host)
            {
                case "deck":
                    // This is where we either launch the deck viewer(basically
                    //   the same control we use for the deck manager, except
                    //   it has the option to save the deck...or maybe, that's
                    //   all that we do. That way we don't have to talk to 
                    //   the current running octgn.
                    break;
            }
            return new MainLauncher();
        }
    }
}