using System;
using System.Threading;
using System.Windows.Forms;

namespace Octgn.Launcher
{
    class Program : ApplicationContext
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger("Program");

        [STAThread]
        static void Main() {
            Log.Info("Starting");

            CancellationTokenSource cts = null;
            try {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var launcher = new Launcher();
                cts = new CancellationTokenSource();

                var window = new Main(launcher, cts.Token) {
                    Opacity = 0,
                    ShowInTaskbar = false
                };

                Application.Run(window);
            } catch (Exception ex) {
                Log.Error("Error running launcher", ex);

                Environment.ExitCode = 1;

                // Show message to user that error occured
                MessageBox.Show("An error occured while running the launcher. Please check the log files for more details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            try {
                cts?.Cancel();
            } catch (ObjectDisposedException) {
            } catch (Exception ex) {
                Log.Error("Error cancelling cancellation token source", ex);
            }

            try {
                cts?.Dispose();
            } catch (ObjectDisposedException) {
            } catch (Exception ex) {
                Log.Error("Error disposing cancellation token source", ex);
            }
        }
    }
}