using System;
using System.Windows.Controls;

namespace Octgn.Installer.Bundle.UI.Pages
{
    public partial class ProgressPage : UserControl
    {
        public ProgressPage() {
            InitializeComponent();
        }
    }

    public class ProgressPageViewModel : PageViewModel
    {
        public string Status {
            get => _status;
            set => SetAndNotify(ref _status, value);
        }
        private string _status;

        public int Progress {
            get => _progress;
            set => SetAndNotify(ref _progress, value);
        }
        private int _progress;

        public string Task {
            get => _task;
            set => SetAndNotify(ref _task, value);
        }
        private string _task;

        private int _cacheProgress;
        private int _executeProgress;

        public ProgressPageViewModel() {
            this.Page = new ProgressPage() {
                DataContext = this
            };

            Button1Text = "Cancel";

            switch (App.Current.RunMode) {
                case RunMode.Install:
                    Task = "Installing...";
                    break;
                case RunMode.Uninstall:
                    Task = "Uninstalling...";
                    break;
                case RunMode.Modify:
                    Task = "Modifying...";
                    break;
                default:
                    throw new NotImplementedException($"RunMode {App.Current.RunMode} not implemented");
            }

            App.Current.CacheAcquireProgress += (sender, args) =>
            {
                if (App.Current.IsCancelling)
                    args.Result = Microsoft.Tools.WindowsInstallerXml.Bootstrapper.Result.Cancel;

                this._cacheProgress = args.OverallPercentage;
                this.Progress = (this._cacheProgress + this._executeProgress) / 2;
            };
            App.Current.ExecuteProgress += (sender, args) =>
            {
                if (App.Current.IsCancelling)
                    args.Result = Microsoft.Tools.WindowsInstallerXml.Bootstrapper.Result.Cancel;

                this._executeProgress = args.OverallPercentage;
                this.Progress = (this._cacheProgress + this._executeProgress) / 2;
            };
        }

        public override void Button1_Action() {
            base.Button1_Action();

            App.Current.Cancel();
        }
    }
}
