using Octgn.Installer.Shared;
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

            if (InstalledOctgn.Get().IsIncompatible) {
                return;
            } else {
                DoTransition(new DirectorySelectionPageViewModel());
            }
        }
    }
}
