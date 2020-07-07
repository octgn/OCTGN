using Octgn.Sdk.Extensibility.Desktop;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.Desktop.Interfaces.Easy
{
    public partial class MenuScreen : Screen
    {
        private readonly MenuPlugin _menuPlugin;

        [Obsolete("For designer only")]
        public MenuScreen() {
            InitializeComponent();
        }

        public MenuScreen(MenuPlugin menuPlugin) {
            _menuPlugin = menuPlugin ?? throw new ArgumentNullException(nameof(menuPlugin));

            InitializeComponent();

            foreach (var item in menuPlugin.MenuItems) {
                var button = new Button {
                    Content = item.Text,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                ButtonList.Items.Add(button);
            }
            //TODO: Create buttons from menuPlugin

            //var converter = new BrushConverter();
            //var brush = (Brush)converter.ConvertFromString(plugin.Theme.Background);

            //this.Background = brush;
        }

        private void SignIn_Click(object sender, RoutedEventArgs e) {
            NavigationService.NavigateTo(new LoginScreen(this));
        }
    }
}
