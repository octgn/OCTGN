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
using System.Windows.Markup;
using System.Windows.Threading;
using Octgn.Core;
using Octgn.Core.Util;
using Octgn.Library.Exceptions;

using log4net;
using Octgn.Controls;
using Octgn.Core.Plugin;
using Octgn.Library.Plugin;
using Octgn.Utils;
using Octgn.Windows;
using Octgn.Communication;
using System.Net;
using System.Threading;

namespace Octgn
{

    public partial class OctgnApp
    {
        // Need this to load Octgn.Core for the logger
        internal static BigInteger bi = new BigInteger(12);
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e)
        {
            Thread.CurrentThread.Name = "MAIN";

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

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            LoggerFactory.DefaultMethod = (con)=> new Log4NetLogger(con.Name);

            Log.Info("Creating Config class");
            try
            {
                Config.Instance = new Config();
            }
            catch (Exception ex)
            {
                //TODO: Find a better user experience for dealing with this error. Like a wizard to fix the problem or something.
                Log.Fatal("Error loading config", ex);

                MessageBox.Show($"Error loading Config:{Environment.NewLine}{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                Shutdown(1);

                return;
            }

            Environment.SetEnvironmentVariable("OCTGN_DATA", Config.Instance.DataDirectoryFull, EnvironmentVariableTarget.Process);

            if (!Directory.Exists(Config.Instance.Paths.UpdatesPath)) {
                Log.Info($"Creating Updates directory");
                Directory.CreateDirectory(Config.Instance.Paths.UpdatesPath);
            }

            // TODO: Show an error if Octgn 3.3 or earlier is installed

            // Check for test mode
            var isTestRelease = false;
            try
            {
                isTestRelease = File.Exists(Path.Combine(Config.Instance.Paths.ConfigDirectory, "TEST"));
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
                using (var cf = new ConfigFile(Config.Instance.Paths.ConfigDirectory))
                {
                    args.Event.AddObject(cf.ConfigData, "Config File");
                }
                args.Event.AddObject(e.Args, "Startup Arguments");

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
                        Me = lc.User
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
            if (ex is UserMessageException)
            {
                if ((ex as UserMessageException).Mode == UserMessageExceptionMode.Blocking || WindowManager.GrowlWindow == null)
                    ShowErrorMessageBox("Error", ex.Message);
                else
                    WindowManager.GrowlWindow.AddNotification(new ErrorNotification(ex.Message));
                Log.Warn("Unhandled Exception", ex);
                handled = true;
            }
            else if (ex is XamlParseException)
            {
                var er = ex as XamlParseException;
                Log.Warn("unhandled exception", ex);
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
                    Log.Fatal("UNHANDLED EXCEPTION", ex);
                else
                    Log.Error("UNHANDLED EXCEPTION", ex);
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
            Log.Warn("Signal_OnException: " + args.Message, args.Exception);
            Application.Current.Dispatcher.InvokeAsync(() => {
                CurrentDomainUnhandledException(sender, new UnhandledExceptionEventArgs(args.Exception, true));
            });
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
            Sounds.Close();
            base.OnExit(e);
        }
    }
}