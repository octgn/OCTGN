namespace Octgn.Launchers
{
    using System.Windows;

    using Octgn.Core.Util;
    using Octgn.Windows;

    public class MainLauncher : UpdatingLauncher
    {
        public override void BeforeUpdate()
        {
            SetupWindows.Instance.RemoveOld(typeof(Program).Assembly);
			SetupWindows.Instance.RegisterOctgnWhatever(typeof(Program).Assembly);
            //SetupWindows.Instance.RegisterCustomProtocol(typeof(Program).Assembly);
            SetupWindows.Instance.RegisterDeckExtension(typeof(Program).Assembly);
			SetupWindows.Instance.RefreshIcons();
            Application.Current.MainWindow = new Window();
            InterProcess.Instance.KillOtherOctgn(false);
        }

        public override void AfterUpdate()
        {
            Log.Info("Creating main window...");
            WindowManager.Main = new Main();
            Log.Info("Main window Created, Launching it.");
            Application.Current.MainWindow = WindowManager.Main;
            Log.Info("Main window set.");
            Log.Info("Launching Main Window");
            WindowManager.Main.Show();
            Log.Info("Main Window Launched");
        }
    }
}