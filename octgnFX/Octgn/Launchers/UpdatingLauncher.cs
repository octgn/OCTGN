namespace Octgn.Launchers
{
    using System.Reflection;

    using log4net;

    using Octgn.Play;
    using Octgn.Windows;

    public abstract class UpdatingLauncher : ILauncher
    {
        public ILog Log { get; private set; }
        public bool Shutdown { get; protected set; }

        protected UpdatingLauncher()
        {
            this.Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void Launch()
        {
            this.BeforeUpdate();
            bool isUpdate = this.RunUpdateChecker();
            if (isUpdate)
            {
                InterProcess.Instance.KillOtherOctgn(true);
                UpdateManager.Instance.UpdateAndRestart();
                this.Shutdown = true;
                return;
            }
            this.AfterUpdate();
        }

        public abstract void BeforeUpdate();

        public abstract void AfterUpdate();

        private bool RunUpdateChecker()
        {
            this.Log.Info("Launching UpdateChecker");
            var uc = new UpdateChecker();
            PlayDispatcher.Instance.UIDispacher = uc.Dispatcher;
            uc.ShowDialog();
            this.Log.Info("UpdateChecker Done.");
            return uc.IsClosingDown;
        }
    }
}