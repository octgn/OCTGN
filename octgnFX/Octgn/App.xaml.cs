using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;
using Exceptionless;
using log4net.Repository.Hierarchy;
using Octgn.Library;

namespace Octgn
{
    using System.Runtime.InteropServices;
    using System.Windows.Markup;
    using System.Windows.Threading;
    using Octgn.Core;
    using Octgn.Core.Util;
    using Octgn.Library.Exceptions;

    using log4net;

    using Octgn.Play;
    using Octgn.Utils;

    public partial class OctgnApp
    {
        // Need this to load Octgn.Core for the logger
        internal static BigInteger bi = new BigInteger(12);
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e)
        {
            int i = 0;
            foreach (var a in e.Args)
            {
                Log.InfoFormat("Arg[{0}]: {1}", i, a);
                i++;
            }

            ExceptionlessClient.Current.Register(false);
            ExceptionlessClient.Current.Configuration.IncludePrivateInformation = true;
            ExceptionlessClient.Current.UnhandledExceptionReporting += (sender, args) =>
            {
                if (args.Exception is InvalidOperationException
                    && args.Exception.Message.StartsWith(
                        "The Application object is being shut down.",
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    args.Cancel = true;
                    return;
                }
            };
            ExceptionlessClient.Current.SendingError += (sender, args) =>
            {
                if (X.Instance.Debug)
                {
                    return;
                }
                X.Instance.Try(() =>
                {
                    args.Error.UserName = Prefs.Username;
                });
                args.Error.AddObject(Paths.Instance, "Registered Paths");
                using (var cf = new ConfigFile())
                {
                    args.Error.AddObject(cf.ConfigData, "Config File");
                }
                args.Error.AddObject(e.Args, "Startup Arguments");
                args.Error.AddRecentTraceLogEntries();

                X.Instance.Try(() =>
                {
                    var ge = Program.GameEngine;
                    var gameString = "";
                    if (ge != null && ge.Definition != null)
                    {
                        var gameObject = new
                        {
                            Game = new
                            {
                                Name=ge.Definition.Name,
								Version = ge.Definition.Version,
								ID = ge.Definition.Id,
								Variables = ge.Variables,
								GlobalVariables = ge.GlobalVariables
                            },
							IsConnected = ge.IsConnected,
							IsLocal = ge.IsLocal,
							SessionId = ge.SessionId,
							WaitingForState = ge.WaitForGameState,
                            Players = Player.All.Select(player=>new
                            {
                                GlobalVariables = player.GlobalVariables,
								Id = player.Id,
								InvertedTable = player.InvertedTable,
								IsGlobalPlayer = player.IsGlobalPlayer,
								Name = player.Name,
								Ready = player.Ready,
								State = player.State,
								Variables = player.Variables,
								WaitingOnPlayers = player.WaitingOnPlayers,
                            })
                        };
						args.Error.AddObject(gameObject,"Game State");
                    }
					

                });

				X.Instance.Try(() =>
				{
                    var hierarchy = LogManager.GetRepository() as Hierarchy;
                    if (hierarchy != null)
                    {
                        var mappender = hierarchy.Root.GetAppender("LimitedMemoryAppender") as LimitedMemoryAppender;
                        if (mappender != null)
                        {
                            var items = new List<string>();

							foreach (var ev in mappender.GetEvents())
							{
								using (var writer = new StringWriter())
								{
									mappender.Layout.Format(writer, ev);
									items.Add(writer.ToString());
									
								}
							}

							args.Error.AddObject( items, "Recent Log Entries");
                        }

                    }
				});

                if (Program.LobbyClient != null)
                {
                    var lc = Program.LobbyClient;
                    var lobbyObject = new
                    {
                        Connected = lc.IsConnected,
                        Me = lc.Me
                    };
                }
            };

            // Need this to load Octgn.Core for the logger
            Debug.WriteLine(bi);
            GlobalContext.Properties["version"] = Const.OctgnVersion;
            GlobalContext.Properties["os"] = Environment.OSVersion.ToString();
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomainOnAssemblyLoad;

            if (X.Instance.Debug)
            {
                AppDomain.CurrentDomain.FirstChanceException += this.CurrentDomainFirstChanceException;
                ExceptionlessClient.Current.Tags.Add("DEBUG");
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
                Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
                if (X.Instance.ReleaseTest)
                {
                    ExceptionlessClient.Current.Tags.Add("TEST");
                }
                else
                {
                    ExceptionlessClient.Current.Tags.Add("LIVE");
                }
            }


            if (e.Args.Any())
            {
                Properties["ArbitraryArgName"] = e.Args[0];
            }

            Log.Debug("Calling Base");
            base.OnStartup(e);
            Log.Debug("Base called.");
            Program.Start(e.Args);

        }

        private void CurrentDomainOnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Log.InfoFormat("LOADED ASSEMBLY: {0} - {1}", args.LoadedAssembly.FullName, args.LoadedAssembly.Location);
        }

        private void CurrentDomainFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            //if (X.Instance.Debug)
            //{
            //    if (Program.GameMess != null && Program.GameEngine != null) Program.GameMess.Warning(e.Exception.Message + "\n" + e.Exception.StackTrace);
            //}
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is UserMessageException)
            {
                e.Dispatcher.Invoke(new Action(() => MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation)));
                e.Handled = true;
            }
            if (e.Exception is InvalidOperationException && e.Exception.Message.StartsWith("The Application object is being shut down.",StringComparison.InvariantCultureIgnoreCase))
            {
                e.Handled = true;
            }
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            var handled = false;
            var ge = Program.GameEngine;
            var gameString = "";
            if (ge != null && ge.Definition != null)
                gameString = "[Game " + ge.Definition.Name + " " + ge.Definition.Version + " " + ge.Definition.Id + "] [Username " + Prefs.Username + "] ";
            if (ex is UserMessageException)
            {
                ShowErrorMessageBox("Error", ex.Message);
                Log.Warn("Unhandled Exception " + gameString, ex);
                handled = true;
            }
            else if (ex is COMException)
            {
                var er = ex as COMException;
                if (er.ErrorCode == 0x800706A6)
                {
                    Log.Warn("Unhandled Exception " + gameString, ex);
                    ShowErrorMessageBox("Error", "Your install of wine was improperly configured for OCTGN. Please make sure to follow our guidelines on our wiki.");
                    handled = true;
                }
            }
            else if (ex is XamlParseException)
            {
                var er = ex as XamlParseException;
                Log.Warn("unhandled exception " + gameString, ex);
                handled = true;
                ShowErrorMessageBox("Error", "There was an error. If you are using Wine(linux/mac) most likely you didn't set it up right. If you are running on windows, then you should try and repair your .net installation and/or update windows. You can also try reinstalling OCTGN.");
            }
            if (!handled)
            {
                if (e.IsTerminating)
                    Log.Fatal("UNHANDLED EXCEPTION " + gameString, ex);
                else
                    Log.Error("UNHANDLED EXCEPTION " + gameString, ex);
            }
            if (e.IsTerminating)
            {
                if (handled)
                    ShowErrorMessageBox("Error", "We will now shut down OCTGN.\nIf this continues to happen please let us know!");
                else
                    ShowErrorMessageBox("Error", "Something unexpected happened. We will now shut down OCTGN.\nIf this continues to happen please let us know!");
                Sounds.Close();
                Application.Current.Shutdown(-1);
            }
        }

        private static void ShowErrorMessageBox(string title, string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation)));
        }

        protected override void OnExit(ExitEventArgs e)
        {
			X.Instance.Try(PlayDispatcher.Instance.Dispose);
            ExceptionlessClient.Current.Shutdown();
            // Fix: this can happen when the user uses the system close button.
            // If a game is running (e.g. in StartGame.xaml) some threads don't
            // stop (i.e. the database thread and/or the networking threads)
            if (Program.IsGameRunning) Program.StopGame();
            Sounds.Close();
            try
            {
                Program.Client.Rpc.Leave(Player.LocalPlayer);
            }
            catch
            {

            }
            base.OnExit(e);
        }
    }
}