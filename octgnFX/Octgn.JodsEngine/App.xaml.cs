/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Octgn.Library;
using System.Windows.Markup;
using System.Windows.Threading;
using Octgn.Core;
using Octgn.Library.Exceptions;
using log4net;
using Octgn.Utils;
using Octgn.Communication;
using System.Threading;
using Octgn.Windows;
using System.Threading.Tasks;

namespace Octgn
{
    public partial class OctgnApp
    {
        private readonly static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e) {
            Thread.CurrentThread.Name = "MAIN";

            ConfigureLogging();

            var i = 0;
            foreach (var arg in e.Args) {
                Log.InfoFormat("Arg[{0}]: {1}", i, arg);

                i++;
            }

            Signal.OnException += Signal_OnException;
            if (!Debugger.IsAttached) {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
                Dispatcher.UnhandledException += CurrentOnDispatcherUnhandledException;
            }

            base.OnStartup(e);
        }

        private static void ConfigureLogging() {
            GlobalContext.Properties["version"] = Const.OctgnVersion;
            GlobalContext.Properties["os"] = Environment.OSVersion.ToString();

            var repository = LogManager.GetRepository(Assembly.GetCallingAssembly());

            var fileInfo = new FileInfo("logging.config");

            log4net.Config.XmlConfigurator.Configure(repository, fileInfo);

            LoggerFactory.DefaultMethod = (con) => new Log4NetLogger(con.Name);
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            if (e.Exception is InvalidOperationException && e.Exception.Message.StartsWith("The Application object is being shut down.", StringComparison.InvariantCultureIgnoreCase)) {
                e.Handled = true;
            } else {
                e.Handled = true;

                HandleUnhandledException(e.Exception);
            }
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            HandleUnhandledException((Exception)e.ExceptionObject);
        }

        private void Signal_OnException(object sender, ExceptionEventArgs args) {
            Log.Warn("Signal_OnException: " + args.Message, args.Exception);
            HandleUnhandledException(args.Exception);
        }

        private static void HandleUnhandledException(Exception ex) {
            var handled = false;
            var ge = Program.GameEngine;
            var gameString = "";
            if (ge?.Definition != null)
                gameString = "[Game " + ge.Definition.Name + " " + ge.Definition.Version + " " + ge.Definition.Id + "] [Username " + Prefs.Username + "] ";

            if (ex is UserMessageException userMessageException) {
                Log.Warn("Unhandled UserMessageException " + gameString, ex);

                switch (userMessageException.Mode) {
                    case UserMessageExceptionMode.Blocking:
                        ShowErrorMessageBox(userMessageException);

                        handled = true;

                        break;
                    case UserMessageExceptionMode.Background:
                        //TODO: Show windows/growl notification
                        Log.Error($"Can not show background error to user.");
                        Debug.Fail($"Can not show background error to user");

                        handled = true;

                        break;
                    default:
                        Log.Error($"{nameof(UserMessageExceptionMode)}.{userMessageException.Mode} not implemented.");

                        break;
                }
            } else if (ex is XamlParseException er) {
                Log.Warn("unhandled exception " + gameString, ex);

                ShowErrorMessageBox(
                    "There was an unexpected error.\nIf you are using Wine(linux/mac) most likely you didn't set it up right.\nIf you are running on windows, then you should try and repair your .net installation and/or update windows.\nYou can also try reinstalling OCTGN.",
                    ex);

                handled = true;
            } else if (ex is IOException && (ex as IOException).Message.Contains("not enough space")) {
                Log.Warn("unhandled exception out of space ", ex);

                ShowErrorMessageBox(
                    "Your computer has run out of hard drive space and OCTGN will have to shut down. Please resolve this before opening OCTGN back up again.",
                    ex);

                handled = true;
            }

            if (!handled) {
                Log.Fatal("UNHANDLED EXCEPTION " + gameString, ex);

                ShowErrorMessageBox("Something unexpected happened. We will now shut down OCTGN.\nIf this continues to happen please let us know!", ex);
            }

            Program.Exit();
        }

        private static void ShowErrorMessageBox(UserMessageException exception) {
            ShowErrorMessageBox(exception.Message, exception);
        }

        private static void ShowErrorMessageBox(string message, Exception exception) {
            if (Current.Dispatcher.Thread == Thread.CurrentThread) {
                ShowErrorMessageBoxSync(message, exception);
            } else {
                ShowErrorMessageBoxAsync(message, exception).Wait();
            }
        }

        private static void ShowErrorMessageBoxSync(string message, Exception exception) {
            Current.Dispatcher.VerifyAccess(); // Consider calling ShowErrorMessageBox

            try {
                var window = new ErrorWindow(message, exception);

                window.ShowDialog();
            } catch (Exception ex) {
                Log.Error($"Error showing error window {ex}");
            }
        }

        private static async Task ShowErrorMessageBoxAsync(string message, Exception exception) {
            if (Current.Dispatcher.Thread == Thread.CurrentThread) // Consider calling ShowErrorMessageBox
                throw new InvalidOperationException($"Do not run this from the Dispatcher thread");

            try {
                ErrorWindow errorWindow; // Do not set errorWindow to null here, instead consider fixing your broken ass logic.
                using (var cancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(5))) {
                    // This timeout stuff here is mostly to catch any dispatcher deadlocks

                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10), cancellationSource.Token); // Just in case it doesn't respect the cancellation token

                    var createWindowTask = Current.Dispatcher.InvokeAsync(()
                        => new ErrorWindow(message, exception), DispatcherPriority.Normal, cancellationSource.Token);

                    var completedTask = await Task.WhenAny(timeoutTask, createWindowTask.Task);

                    if (completedTask == timeoutTask)
                        throw new TimeoutException("Timed out creating ErrorWindow. Dispatcher failed to respect CancellationToken");

                    if (completedTask != createWindowTask.Task) throw new InvalidOperationException("Task wait logic is borked");

                    try {
                        errorWindow = await createWindowTask.Task;
                    } catch (OperationCanceledException ex) {
                        throw new TimeoutException("Timed out creating ErrorWindow");
                    }
                }

                // If we survived the onslaught above, the Dispatcher isn't locked up, so just call like normal
                Current.Dispatcher.Invoke(() => {
                    errorWindow.ShowDialog();
                });
            } catch (Exception ex) {
                Log.Error($"Error showing error window {ex}");
            }
        }

        protected override void OnExit(ExitEventArgs e) {
            //X.Instance.Try(PlayDispatcher.Instance.Dispose);
            // Fix: this can happen when the user uses the system close button.
            // If a game is running (e.g. in StartGame.xaml) some threads don't
            // stop (i.e. the database thread and/or the networking threads)
            X.Instance.Try(Program.StopGame);
            Sounds.Close();
            base.OnExit(e);
        }
    }
}