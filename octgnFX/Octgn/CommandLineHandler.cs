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
                int? hostport = null;
                Guid? gameid = null;
                string deckPath = null;
                var os = new Mono.Options.OptionSet()
                         {
                             { "t|table", x => tableOnly = true },
                             { "g|game=",x=> gameid=Guid.Parse(x)},
                             { "d|deck=",x=>deckPath= x}
                         };
                try
                {
                    os.Parse(args);
                }
                catch (Exception e)
                {
                    Log.Warn("Parse args exception: " + String.Join(",", Environment.GetCommandLineArgs()), e);
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

                if (deckPath != null)
                {
                    return new DeckEditorLauncher(deckPath);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error handling arguments", e);
                if (args != null) Log.Error(string.Join(Environment.NewLine, args));
            }
			return new MainLauncher();
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