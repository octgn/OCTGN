using Octgn.Library.Exceptions;

namespace Octgn
{
    using System;
    using System.IO;
    using System.Reflection;

    using log4net;

    using Octgn.Launchers;
    using Octgn.Online.Hosting;

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

		public bool ShutdownProgram { get; private set; }

        public ILauncher HandleArguments(string[] args)
        {
            try
            {
                if (args != null) Log.Debug(string.Join(Environment.NewLine, args));
                if (args.Length < 2) return null;
                Log.Info("Has arguments");
                if (args[1].StartsWith("octgn://", StringComparison.InvariantCultureIgnoreCase))
                {
                    Log.Info("Using protocol");
                    var uri = new Uri(args[1]);
                    return HandleProtocol(uri);
                }
                var tableOnly = false;
                var editorOnly = false;
                var joinGame = false;
                var hostGame = false;
                int? hostport = null;
                Guid? gameid = null;
                string deckPath = null;
                string username = null;
                string hostedGameString = null;
                var os = new Mono.Options.OptionSet()
                {
                    {"t|table", x => tableOnly = true},
                    {"g|game=", x => gameid = Guid.Parse(x)},
                    {"d|deck=", x => deckPath = x},
                    {"x|devmode", x => DevMode = true},
                    {"e|editor", x => editorOnly = true},
                    {"j|join", x => joinGame = true },
                    {"u|username=", x => username = x },
                    {"s|hostedgame=", x => hostedGameString = x },
                    {"h|host", x => hostGame = true }
                };
                try
                {
                    os.Parse(args);
                }
                catch (Exception e)
                {
                    Log.Warn("Parse args exception: " + String.Join(",", Environment.GetCommandLineArgs()), e);
                }
                if (tableOnly) {
                    return new TableLauncher(hostport, gameid);
                } else if (joinGame) {
                    var hostedGame = (HostedGame)DeserializeHostedgameString(hostedGameString);

                    return new JoinGameLauncher(username, hostedGame);
                } else if (hostGame) {
                    var hostedGame = (HostedGame)DeserializeHostedgameString(hostedGameString);

                    return new HostGameLauncher(username, hostedGame);
                } else if(tableOnly && joinGame) {
                    //TODO: Show error
                    return null;
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
                    return new DeckEditorLauncher(deckPath);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error handling arguments", e);
                if (args != null) Log.Error(string.Join(Environment.NewLine, args));
            }
            return null;
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
            throw new NotImplementedException();
        }
    }
}