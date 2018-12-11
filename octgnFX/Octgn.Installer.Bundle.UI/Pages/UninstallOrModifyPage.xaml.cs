using System;
using System.Windows.Controls;

namespace Octgn.Installer.Bundle.UI.Pages
{
    public partial class UninstallOrModifyPage : UserControl
    {
        public UninstallOrModifyPage() {
            InitializeComponent();
        }
    }

    public class UninstallOrModifyPageViewModel : PageViewModel
    {
        public bool DoModify {
            get => _doModify;
            set => SetAndNotify(ref _doModify, value);
        }
        private bool _doModify;

        public bool DoUninstall {
            get => _doUninstall;
            set => SetAndNotify(ref _doUninstall, value);
        }
        private bool _doUninstall;

        public UninstallOrModifyPageViewModel() {
            Button1Text = "Continue";

            DoModify = true;

            Page = new UninstallOrModifyPage() {
                DataContext = this
            };
        }

        public override void Button1_Action() {
            base.Button1_Action();

            if (DoModify && DoUninstall)
                throw new InvalidOperationException($"Can't have DoModify and DoUninstall both");

            if (!DoModify && !DoUninstall)
                throw new InvalidOperationException($"Must select DoModify or DoUninstall");

            if (DoModify) {
                App.Current.RunMode = RunMode.Modify;
                DoTransition(new DirectorySelectionPageViewModel());
            } else if (DoUninstall) {
                App.Current.RunMode = RunMode.Uninstall;
                DoTransition(new ProgressPageViewModel());
                App.Current.StartUninstall();
            }
        }
    }
}
