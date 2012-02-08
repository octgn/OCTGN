using System.Windows;

namespace Octgn.Launcher
{
    public partial class UpgradeMessage : Window
    {
        public UpgradeMessage()
        {
            InitializeComponent();
        }

        protected void ContinueClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}