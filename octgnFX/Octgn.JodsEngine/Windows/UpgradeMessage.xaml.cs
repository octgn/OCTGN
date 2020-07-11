using System.Windows;

namespace Octgn.Windows
{
    public partial class UpgradeMessage
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