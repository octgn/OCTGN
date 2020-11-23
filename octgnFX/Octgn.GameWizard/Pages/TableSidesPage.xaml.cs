using Octgn.GameWizard.Controls;
using System;

namespace Octgn.GameWizard.Pages
{
    public partial class TableSidesPage : WizardPage
    {
        public TableSidesPage() {
            InitializeComponent();
        }

        public override void OnEnteringPage() {
            SingleSideOption.IsChecked = !Game.IsDualSided;
            DualSideOption.IsChecked = Game.IsDualSided;
        }

        public override void OnLeavingPage() {
            if (SingleSideOption.IsChecked == true) {
                Game.IsDualSided = false;
            } else {
                Game.IsDualSided = true;
            }
        }
    }
}
