using System;

namespace Octgn.Controls
{
    public partial class NewVersionBar
    {
        public NewVersionBar() {
            InitializeComponent();
        }

        private void TextBlock_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            Program.LaunchUrl(AppConfig.WebsitePath);
        }
    }
}
