/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Octgn.Library;
using System.Windows.Markup;
using System.Windows.Threading;
using Octgn.Core;
using Octgn.Core.Util;
using Octgn.Library.Exceptions;

using log4net;
using Octgn.Utils;
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
            Console.WriteLine("Hi");


            Thread.CurrentThread.Name = "MAIN";

            // Need this to load Octgn.Core for the logger
            Debug.WriteLine(bi);

            { // Configure logging
                GlobalContext.Properties["version"] = Const.OctgnVersion;
                GlobalContext.Properties["os"] = Environment.OSVersion.ToString();

                var repository = LogManager.GetRepository(Assembly.GetCallingAssembly());

                var fileInfo = new FileInfo("logging.config");

                log4net.Config.XmlConfigurator.Configure(repository, fileInfo);
            }

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

            Signal.OnException += Signal_OnException;
            if (!X.Instance.Debug)
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
                Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
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

        private static void ShowUserMessageException(UserMessageException userMessageException) {
            switch (userMessageException.Mode) {
                case UserMessageExceptionMode.Blocking:
                    ShowErrorMessageBox("Error", userMessageException.Message);

                    break;
                case UserMessageExceptionMode.Background:
                    //TODO: Show windows/growl notification
                    Log.Error($"Can not show background error to user.");
                    Debug.Fail($"Can not show background error to user");

                    break;
                default:
                    Log.Error($"{nameof(UserMessageExceptionMode)}.{userMessageException.Mode} not implemented.");

                    break;
            }
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is UserMessageException userMessageException) {
                ShowUserMessageException(userMessageException);

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
            if (ge?.Definition != null)
                gameString = "[Game " + ge.Definition.Name + " " + ge.Definition.Version + " " + ge.Definition.Id + "] [Username " + Prefs.Username + "] ";

            if (ex is UserMessageException userMessageException) {
                Log.Warn("Unhandled UserMessageException " + gameString, ex);

                ShowUserMessageException(userMessageException);

                handled = true;
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
            //ExceptionlessClient.Default.Shutdown();
            // Fix: this can happen when the user uses the system close button.
            // If a game is running (e.g. in StartGame.xaml) some threads don't
            // stop (i.e. the database thread and/or the networking threads)
            X.Instance.Try(Program.StopGame);
            Sounds.Close();
            base.OnExit(e);
        }
    }
}