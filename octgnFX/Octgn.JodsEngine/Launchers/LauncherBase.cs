using System.Reflection;
using log4net;
using Octgn.Windows;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System;

namespace Octgn.Launchers
{
    public abstract class LauncherBase : ILauncher
    {
        protected ILog Log { get; }

        protected abstract Task<Window> Load(ILoadingView loadingView);

        protected LauncherBase() {
            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public async Task<bool> Launch(ILoadingView view) {
            Dispatcher.CurrentDispatcher.VerifyAccess();

            var window = await Load(view);

            if (window == null) {
                Log.Warn("No window created");

                return false;
            }

            // do async so can run in backround
            await Dispatcher.Yield(DispatcherPriority.Background);

            Application.Current.MainWindow = window;

            await Task.Delay(300);

            return true;
        }
    }
}