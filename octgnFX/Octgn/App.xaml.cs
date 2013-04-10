using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Windows;

using Octgn.Windows;

namespace Octgn
{
    using log4net;

    public partial class OctgnApp
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e)
        {
#if(!DEBUG)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
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

        private static void CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            Log.Fatal(ex);
            if (!Debugger.IsAttached)
            {
                var wnd = new Windows.ErrorWindow(ex);
                wnd.ShowDialog();
                ErrorReporter.SumbitException(ex);
            }
            else
            {
                if (e.IsTerminating)
                    Debugger.Break();
            }

            if (!e.IsTerminating)
                Program.DebugTrace.TraceEvent(TraceEventType.Error, 0, ex.ToString());
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