using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;

using Octgn.Windows;

namespace Octgn
{
    using System.Runtime.InteropServices;
    using System.Windows.Threading;

    using Octgn.Controls;
    using Octgn.Library.Exceptions;

    using log4net;

    public partial class OctgnApp
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e)
        {
            GlobalContext.Properties["version"] = Const.OctgnVersion;
            GlobalContext.Properties["os"] = Environment.OSVersion.ToString();
#if(!DEBUG)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
            Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
#else

            AppDomain.CurrentDomain.FirstChanceException += this.CurrentDomainFirstChanceException;
#endif
            if (e.Args.Any())
            {
                Properties["ArbitraryArgName"] = e.Args[0];
            }

            Log.Debug("Calling Base");
            base.OnStartup(e);
            Log.Debug("Base called.");
            Program.Start();

        }

        private void CurrentDomainFirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
#if(DEBUG)
            Log.Error("FirstChanceException",e.Exception);
#endif
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception is UserMessageException)
            {
                e.Dispatcher.Invoke(new Action(() => MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation)));
                e.Handled = true;
            }
        }

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            var handled = false;
            if (ex is UserMessageException)
            {
                ShowErrorMessageBox("Error",ex.Message);
                Log.Warn("Unhandled Exception ",ex);
                handled = true;
            }
            else if (ex is COMException)
            {
                var er = ex as COMException;
                if (er.ErrorCode == 0x800706A6)
                {
                    Log.Warn("Unhandled Exception",ex);
                    ShowErrorMessageBox("Error","Your install of wine was improperly configured for OCTGN. Please make sure to follow our guidelines on our wiki.");
                    handled = true;
                }
            }
            if(!handled)
            {
                if (e.IsTerminating)
                    Log.Fatal("UNHANDLED EXCEPTION", ex);
                else
                    Log.Error("UNHANDLED EXCEPTION", ex);
            }
            if (e.IsTerminating)
            {
                if(handled)
                    ShowErrorMessageBox("Error","We will now shut down OCTGN.\nIf this continues to happen please let us know!");
                else
                    ShowErrorMessageBox("Error","Something unexpected happened. We will now shut down OCTGN.\nIf this continues to happen please let us know!");
                Application.Current.Shutdown(-1);
            }
        }

        private static void ShowErrorMessageBox(string title, string message)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Exclamation)));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Fix: this can happen when the user uses the system close button.
            // If a game is running (e.g. in StartGame.xaml) some threads don't
            // stop (i.e. the database thread and/or the networking threads)
            if (Program.IsGameRunning) Program.StopGame();
            base.OnExit(e);
        }
    }
}