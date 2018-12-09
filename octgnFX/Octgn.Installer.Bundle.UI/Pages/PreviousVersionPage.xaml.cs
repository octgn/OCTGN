using System;
using System.Windows.Controls;

namespace Octgn.Installer.Bundle.UI.Pages
{
    public partial class PreviousVersionPage : UserControl
    {
        public PreviousVersionPage() {
            InitializeComponent();
        }
    }

    public class PreviousVersionPageViewModel : PageViewModel
    {
        public PreviousVersionPageViewModel() {
            Button1Text = "Retry";

            Page = new PreviousVersionPage();
        }

        public override void Button1_Action() {
            base.Button1_Action();

            if (App.Current.IsIncompatibleOctgnInstalled()) {
                return;
            } else {
                DoTransition(new DirectorySelectionPageViewModel());
            }
        }
    }
}
