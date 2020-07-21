using System.Reflection;
using log4net;
using Octgn.Windows;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Octgn.Launchers
{
    public abstract class LauncherBase : ILauncher
    {
        public ILog Log { get; private set; }
        public bool Shutdown { get; protected set; }

        protected LauncherBase() {
            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void Launch() {
            Dispatcher.CurrentDispatcher.VerifyAccess();

            if (Shutdown) return;

            var uc = new UpdateChecker();

            uc.AddLoader(async (view) => {
                var window = await Load(view);

                if (window == null) {
                    Shutdown = true;

                    return;
                }

                // do async so can run in backround
                await Dispatcher.Yield(DispatcherPriority.Background);

                Application.Current.MainWindow = window;

                await Task.Delay(1000);
            });

            uc.ShowDialog();

            Shutdown = uc.Shutdown;
        }

        protected abstract Task<Window> Load(ILoadingView loadingView);
    }
}