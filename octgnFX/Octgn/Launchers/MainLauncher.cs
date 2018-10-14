using System.Windows;
using Octgn.Windows;
using System.Threading.Tasks;

namespace Octgn.Launchers
{
    public class MainLauncher : UpdatingLauncher
    {
        public override Task BeforeUpdate()
        {
            Application.Current.MainWindow = new Window();
            InterProcess.Instance.KillOtherOctgn(false);
            return Task.CompletedTask;
        }

        public override Task AfterUpdate()
        {
            Log.Info("Creating main window...");
            WindowManager.Main = new Main();
            Log.Info("Main window Created, Launching it.");
            Application.Current.MainWindow = WindowManager.Main;
            Log.Info("Main window set.");
            Log.Info("Launching Main Window");
            WindowManager.Main.Show();
            Log.Info("Main Window Launched");
            return Task.CompletedTask;
        }
    }
}