/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
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
using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Windows.Threading;
using Octgn.Core;
using Octgn.Core.Util;
using Octgn.Library.Exceptions;

using log4net;
using Octgn.Controls;
using Octgn.Core.Plugin;
using Octgn.Library.Plugin;
using Octgn.Play;
using Octgn.Utils;
using Octgn.Windows;
using Octgn.Communication;

namespace Octgn
{

    public partial class OctgnApp
    {
        // Need this to load Octgn.Core for the logger
        internal static BigInteger bi = new BigInteger(12);
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e)
        {
            // Need this to load Octgn.Core for the logger
            Debug.WriteLine(bi);
            GlobalContext.Properties["version"] = Const.OctgnVersion;
            GlobalContext.Properties["os"] = Environment.OSVersion.ToString();
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomainOnAssemblyLoad;

            int i = 0;
            foreach (var a in e.Args)
            {
                Log.InfoFormat("Arg[{0}]: {1}", i, a);
                i++;
            }

            LoggerFactory.DefaultMethod = (con)=> new Log4NetLogger(con.Name);

            // Check for test mode
            var isTestRelease = false;
            try
            {
                isTestRelease = System.IO.File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn", "Config", "TEST"));
            }
            catch(Exception ex)
            {
                Log.Warn("Error checking for test mode", ex);
            }

            ExceptionlessClient.Default.Register(false);
            ExceptionlessClient.Default.Configuration.IncludePrivateInformation = true;
            ExceptionlessClient.Default.SubmittingEvent += (sender, args) =>
            {
                if (X.Instance.Debug)
                {
                    args.Cancel = true;
                    return;
                }
                if (args.IsUnhandledError)
                {
                    var exception = args.PluginContextData.GetException();
                    if (exception is InvalidOperationException)
                    {
                        bool gotit = exception.Message.StartsWith("The Application object is being shut down.", StringComparison.InvariantCultureIgnoreCase)
                            || exception.Message.StartsWith("El objeto Application se va a cerrar.", StringComparison.CurrentCultureIgnoreCase);
                        gotit = gotit ||
                                (exception.Message.ToLower().Contains("system.windows.controls.grid") &&
                                 exception.Message.ToLower().Contains("row"));
                        args.Cancel = gotit;
                        return;
                    }
                    if (exception is BadImageFormatException)
                    {
                        if (exception.Message.Contains("Could not load file or assembly") && exception.Message.Contains("Microsoft.Dynamic"))
                        {
                            args.Cancel = true;
                            TopMostMessageBox.Show("OCTGN is installed improperly and must close. Please try reinstalling OCTGN. If that doesn't work, you can find help at OCTGN.net", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    var src = exception.Source;
                    try
                    {
                        foreach (var plug in PluginManager.GetPlugins<IDeckBuilderPlugin>())
                        {
                            var pt = plug.GetType();
                            var pn = pt.Assembly.GetName();
                            if (src == pn.Name)
                            {
                                args.Cancel = true;
                                return;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Check against plugins error", ex);
                    }
                }
                X.Instance.Try(() =>
                {
                    args.Event.SetUserIdentity(Prefs.Username);
                });
                args.Event.AddObject(Config.Instance.Paths, "Registered Paths");
                using (var cf = new ConfigFile(Config.Instance.ConfigPath))
                {
                    args.Event.AddObject(cf.ConfigData, "Config File");
                }
                args.Event.AddObject(e.Args, "Startup Arguments");

                X.Instance.Try(() =>
                {
                    var ge = Program.GameEngine;
                    //var gameString = "";
                    if (ge != null && ge.Definition != null)
                    {
                        var gameObject = new
                        {
                            Game = new
                            {
                                Name = ge.Definition.Name,
                                Version = ge.Definition.Version,
                                ID = ge.Definition.Id,
                                Variables = ge.Variables,
                                GlobalVariables = ge.GlobalVariables
                            },
                            IsConnected = ge.IsConnected,
                            IsLocal = ge.IsLocal,
                            SessionId = ge.SessionId,
                            WaitingForState = ge.WaitForGameState,
                            Players = Player.All.Select(player => new
                            {
                                GlobalVariables = player.GlobalVariables,
                                Id = player.Id,
                                InvertedTable = player.InvertedTable,
                                IsGlobalPlayer = player.IsGlobalPlayer,
                                Name = player.Name,
                                Ready = player.Ready,
                                State = player.State,
                                WaitingOnPlayers = player.WaitingOnPlayers,
                            })
                        };
                        args.Event.AddObject(gameObject, "Game State");
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

                            args.Event.AddObject(items, "Recent Log Entries");
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

            Signal.OnException += Signal_OnException;
            if (X.Instance.Debug)
            {
                AppDomain.CurrentDomain.FirstChanceException += this.CurrentDomainFirstChanceException;
                ExceptionlessClient.Default.Configuration.DefaultTags.Add("DEBUG");
            }
            else
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
                Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
                if (isTestRelease)
                {
                    ExceptionlessClient.Default.Configuration.DefaultTags.Add("TEST");
                }
                else
                {
                    ExceptionlessClient.Default.Configuration.DefaultTags.Add("LIVE");
                }
            }


            if (e.Args.Any())
            {
                Properties["ArbitraryArgName"] = e.Args[0];
            }

            Log.Debug("Calling Base");
            base.OnStartup(e);
            Log.Debug("Base called.");
            Program.Start(e.Args, isTestRelease);

        }

        private void CurrentDomainOnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            //Log.InfoFormat("LOADED ASSEMBLY: {0} - {1}", args.LoadedAssembly.FullName, args.LoadedAssembly.IsDynamic == false ? args.LoadedAssembly.Location : "NOLOCATIONDYNAMIC");
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
				if((e.Exception as UserMessageException).Mode == UserMessageExceptionMode.Blocking || WindowManager.GrowlWindow == null)
					ShowErrorMessageBox("Error",e.Exception.Message);
				else
				    WindowManager.GrowlWindow.AddNotification(new ErrorNotification(e.Exception.Message));
                e.Handled = true;
            }
            if (e.Exception is InvalidOperationException && e.Exception.Message.StartsWith("The Application object is being shut down.", StringComparison.InvariantCultureIgnoreCase))
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
                if ((ex as UserMessageException).Mode == UserMessageExceptionMode.Blocking || WindowManager.GrowlWindow == null)
                    ShowErrorMessageBox("Error", ex.Message);
                else
                    WindowManager.GrowlWindow.AddNotification(new ErrorNotification(ex.Message));
                //ShowErrorMessageBox("Error", ex.Message);
                Log.Warn("Unhandled Exception " + gameString, ex);
                handled = true;
            }
            else if (ex is COMException)
            {
                //var er = ex as COMException;
                //if (er.ErrorCode == 0x800706A6) // Th
                //{
                //    Log.Warn("Unhandled Exception " + gameString, ex);
                //    ShowErrorMessageBox("Error", "Your install of wine was improperly configured for OCTGN. Please make sure to follow our guidelines on our wiki.");
                //    handled = true;
                //}
            }
            else if (ex is XamlParseException)
            {
                var er = ex as XamlParseException;
                Log.Warn("unhandled exception " + gameString, ex);
                handled = true;
                ShowErrorMessageBox("Error", "There was an error. If you are using Wine(linux/mac) most likely you didn't set it up right. If you are running on windows, then you should try and repair your .net installation and/or update windows. You can also try reinstalling OCTGN.");
            }
            else if (ex is IOException && (ex as IOException).Message.Contains("not enough space"))
            {
                handled = true;
                ShowErrorMessageBox("Error", "Your computer has run out of hard drive space and OCTGN will have to shut down. Please resolve this before opening OCTGN back up again.");
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

        private void Signal_OnException(object sender, ExceptionEventArgs args) {
            Log.Fatal("Signal_OnException: " + args.Message, args.Exception);
            Sounds.Close();
            Application.Current.Shutdown(-1);
        }

        private static void ShowErrorMessageBox(string title, string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation)));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            //X.Instance.Try(PlayDispatcher.Instance.Dispose);
            ExceptionlessClient.Default.Shutdown();
            // Fix: this can happen when the user uses the system close button.
            // If a game is running (e.g. in StartGame.xaml) some threads don't
            // stop (i.e. the database thread and/or the networking threads)
            X.Instance.Try(Program.StopGame);
            Sounds.Close();
            base.OnExit(e);
        }
    }
}