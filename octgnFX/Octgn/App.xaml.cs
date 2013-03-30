using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows;

using Octgn.Windows;

namespace Octgn
{
    using log4net;

    public partial class OctgnApp
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal const string ClientName = "Octgn.NET";
        internal static readonly Version OctgnVersion = GetClientVersion();
        internal static readonly Version BackwardCompatibility = new Version(0, 2, 0, 0);

        private static Version GetClientVersion()
        {
            Assembly asm = typeof (OctgnApp).Assembly;
            //var at = (AssemblyProductAttribute) asm.GetCustomAttributes(typeof (AssemblyProductAttribute), false)[0];
            return asm.GetName().Version;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Debug("Calling Base");
            base.OnStartup(e);
            Log.Debug("Base called.");

#if(!DEBUG)
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainUnhandledException;
#else

            AppDomain.CurrentDomain.FirstChanceException += this.CurrentDomainFirstChanceException;
#endif
			//Program.GamesRepository = new GamesRepository();

            //if (Program.GamesRepository.MissingFiles.Any())
            //{
            //    var sb =
            //        new StringBuilder(
            //            "Octgn cannot find the following files. The corresponding games have been disabled.\n\n");
            //    foreach (string file in Program.GamesRepository.MissingFiles)
            //        sb.Append(file).Append("\n\n");
            //    sb.Append("You should restore those files, or re-install the corresponding games.");

            //    ShutdownMode oldShutdown = ShutdownMode;
            //    ShutdownMode = ShutdownMode.OnExplicitShutdown;
            //    new Windows.MessageWindow(sb.ToString()).ShowDialog();
            //    ShutdownMode = oldShutdown;
            //}

            Log.Info("Launching UpdateChecker");
            var uc = new UpdateChecker();
            uc.ShowDialog();
            Log.Info("UpdateChecker Done.");
            if (!uc.IsClosingDown)
            {
                Log.Info("Launching Main Window");
                Program.MainWindowNew.Show();
                Log.Info("Main Window Launched");
            }
            else
            {
                Log.Info("Closing For Updates");
                Program.MainWindowNew.Close();
                Current.MainWindow = null;
                Program.Exit();
            }

            if (e.Args.Any())
            {
                Properties["ArbitraryArgName"] = e.Args[0];
            }



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