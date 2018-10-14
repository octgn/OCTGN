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

            if (App.Current.IsInstall) {
                Task = "Installing...";
            } else {
                Task = "Uninstalling...";
            }

            App.Current.CacheAcquireProgress += (sender, args) =>
            {
                this._cacheProgress = args.OverallPercentage;
                this.Progress = (this._cacheProgress + this._executeProgress) / 2;
            };
            App.Current.ExecuteProgress += (sender, args) =>
            {
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
