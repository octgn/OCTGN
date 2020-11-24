using Octgn.GameWizard.Controls;
using System;
using System.Diagnostics;
using System.Windows;

namespace Octgn.GameWizard.Pages
{
    public partial class FinalPage : WizardPage
    {
        public FinalPage() {
            InitializeComponent();
        }

        public override void OnEnteringPage() {
            ForwardEnabled = true;
            BackEnabled = false;
        }

        public override void OnForward(object sender, RoutedEventArgs args) {
            args.Handled = true;

            App.Current.MainWindow.Close();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) {
            var dir = Game.Directory;

            Process.Start("explorer.exe", "\"" + dir + "\"");
        }
    }
}
