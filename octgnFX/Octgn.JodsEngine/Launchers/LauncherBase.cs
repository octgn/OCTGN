using System.Reflection;
using log4net;
using Octgn.Windows;
using System.Threading.Tasks;

namespace Octgn.Launchers
{
    public abstract class LauncherBase : ILauncher
    {
        public ILog Log { get; private set; }
        public bool Shutdown { get; protected set; }

        protected LauncherBase() {
            Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public async Task Launch() {
            if (Shutdown == false) {
                ShowLoaderWindow();

                await Loaded();
            }
        }

        protected abstract Task Load();

        protected abstract Task Loaded();

        private void ShowLoaderWindow() {
            var uc = new UpdateChecker();

            uc.AddLoader(() => {
                Load().GetAwaiter().GetResult();
            });

            uc.ShowDialog();
        }
    }
}