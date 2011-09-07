using System;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Input;
using System.Reflection;
using Octgn.Properties;
using System.Text;

namespace Octgn
{
  public partial class OctgnApp : Application
  {
    internal const string ClientName = "OCTGN.NET";
    internal static readonly Version OctgnVersion = GetClientVersion();
    internal static readonly Version BackwardCompatibility = new Version(0, 2, 0, 0);

    private static Version GetClientVersion()
    {
      Assembly asm = typeof(OctgnApp).Assembly;
      AssemblyProductAttribute at = (AssemblyProductAttribute)asm.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];
      return asm.GetName().Version;
    }

    protected override void OnStartup(StartupEventArgs e)
		{
      if (!System.Diagnostics.Debugger.IsAttached)
   			AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs args)
   			{
   				Exception ex = args.ExceptionObject as Exception;
   				var wnd = new ErrorWindow(ex);
   				wnd.ShowDialog();
   			};

			Updates.UpgradeSettings();

      Updates.PerformHouskeeping();

      Program.GamesRepository = new Octgn.Data.GamesRepository();

      if (Program.GamesRepository.MissingFiles.Any())
      {
        var sb = new StringBuilder("OCTGN cannot find the following files. The corresponding games have been disabled.\n\n");
        foreach (var file in Program.GamesRepository.MissingFiles)
          sb.Append(file).Append("\n\n");
        sb.Append("You should restore those files, or re-install the corresponding games.");

        var oldShutdown = ShutdownMode;
        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
        new MessageWindow(sb.ToString()).ShowDialog();
        ShutdownMode = oldShutdown;
      }

			base.OnStartup(e);
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