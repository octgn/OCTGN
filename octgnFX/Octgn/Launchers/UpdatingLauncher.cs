using System.Reflection;
using log4net;
using Octgn.Windows;
using System.Threading.Tasks;

namespace Octgn.Launchers
{
    public abstract class UpdatingLauncher : ILauncher
    {
        public ILog Log { get; private set; }
        public bool Shutdown { get; protected set; }

        protected UpdatingLauncher()
        {
            this.Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public async Task Launch()
        {
            await this.BeforeUpdate();
            if (this.Shutdown == false)
            {
                bool isUpdate = false;
                
                // Check if we should skip update check
                if (CommandLineHandler.Instance.SkipUpdate)
                {
                    this.Log.Info("Skipping update check due to --no-update flag");
                }
                else
                {
                    isUpdate = this.RunUpdateChecker();
                }
                
                if (isUpdate)
                {
                    InterProcess.Instance.KillOtherOctgn(true);
                    if (UpdateManager.Instance.UpdateAndRestart()) {
                        this.Shutdown = true;
                        return;
                    }
                }
                await this.AfterUpdate();
            }
        }

        public abstract Task BeforeUpdate();

        public abstract Task AfterUpdate();

        private bool RunUpdateChecker()
        {
            this.Log.Info("Launching UpdateChecker");
            var uc = new UpdateChecker();
            //PlayDispatcher.Instance.UIDispacher = uc.Dispatcher;
            uc.ShowDialog();
            this.Log.Info("UpdateChecker Done.");
            return uc.IsClosingDown;
        }
    }
}