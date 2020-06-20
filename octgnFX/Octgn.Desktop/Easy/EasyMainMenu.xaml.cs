using System;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.Desktop.Easy
{
    public partial class EasyMainMenu : Page
    {
        public EasyMainMenu() {
            InitializeComponent();
        }

        private void SignIn_Click(object sender, RoutedEventArgs e) {
            this.NavigationService.Navigate(new Login());
        }
    }
}
