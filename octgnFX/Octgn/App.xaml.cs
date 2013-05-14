using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;

using Octgn.Windows;

namespace Octgn
{
    using System.Windows.Threading;

    using Octgn.Library.Exceptions;

    using log4net;

    public partial class OctgnApp
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e)
        {
            GlobalContext.Properties["version"] = Const.OctgnVersion;
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
            if (e.IsTerminating) Log.Fatal("",ex);
            else Log.Error("",ex);
            if (ex is UserMessageException)
            {
                Current.Dispatcher.Invoke(new Action(() => MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation)));
            }
            
            Application.Current.Dispatcher.Invoke(new Action(() => MessageBox.Show("Something unexpected happened. We will now shut down OCTGN.\nIf this continues to happen please let us know!", "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation)));
            Application.Current.Shutdown(0);
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