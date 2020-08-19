namespace Octgn
{
    using System;
    using System.IO;
    using System.Reflection;

    using log4net;
    using Octgn.DataNew;
    using Octgn.Launchers;
    using Octgn.Library.Exceptions;
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
                var spectate = false;
                var desktopContext = false;
                int? hostport = null;
                Guid? gameid = null;
                string deckPath = null;
                string username = null;
                string hostedGameString = null;
                string historyPath = null;
                var os = new Mono.Options.OptionSet()
                {
                    {"t|table", x => tableOnly = true},
                    {"g|game=", x => gameid = Guid.Parse(x)},
                    {"d|deck=", x => deckPath = x},
                    {"x|devmode", x => DevMode = true},
                    {"e|editor", x => editorOnly = true},
                    {"j|join", x => joinGame = true },
                    {"h|joinashost", x => hostGame = true },
                    {"u|username=", x => username = x },
                    {"k|hostedgame=", x => hostedGameString = x },
                    {"s|spectate", x => spectate = true },
                    {"r|replay=", x => historyPath = x },
                    {"p|desktopcontext", x => desktopContext = true }
                };
                try
                {
                    os.Parse(args);
                }
                catch (Exception e)
                {
                    Log.Warn("Parse args exception: " + String.Join(",", Environment.GetCommandLineArgs()), e);
                }

                if (desktopContext) {
                    DbContext.SetContext(new DesktopIntegration.DesktopDbContext());
                }

                if (tableOnly) {
                    return new TableLauncher(hostport, gameid);
                } else if (joinGame) {
                    var hostedGame = HostedGame.Deserialize(hostedGameString);

                    Validate(hostedGame);

                    return new JoinGameLauncher(
                        hostedGame,
                        username,
                        false,
                        spectate
                    );
                } else if (hostGame) {
                    var hostedGame = HostedGame.Deserialize(hostedGameString);

                    Validate(hostedGame);

                    return new JoinGameLauncher(
                        hostedGame,
                        username,
                        true,
                        spectate
                    );
                } else if(tableOnly && joinGame) {
                    //TODO: Show error
                    return null;
                }else if (!string.IsNullOrWhiteSpace(historyPath)) {
                    return new ReplayLauncher(historyPath);
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
            catch (UserMessageException) {
                throw;
            }
            catch (Exception e)
            {
                Log.Error("Error handling arguments", e);
                if (args != null) Log.Error(string.Join(Environment.NewLine, args));
            }
            return null;
        }

        private static void Validate(HostedGame hostedGame) {
            var minCreatedDate = DateTimeOffset.Now.AddHours(6);
            var maxCreatedDate = DateTimeOffset.Now.AddMinutes(1);
            if (hostedGame.DateCreated.UtcDateTime < minCreatedDate || hostedGame.DateCreated > maxCreatedDate)
                throw new UserMessageException($"Invalid game CreatedDate {hostedGame.DateCreated}");

            if (hostedGame.Id == Guid.Empty)
                throw new UserMessageException($"Game Id Empty");
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