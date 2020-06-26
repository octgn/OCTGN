using Octgn.Sdk.Data;
using Octgn.Sdk.Extensibility;
using Octgn.Sdk.Extensibility.MainMenu;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Octgn.Desktop.Easy
{
    public partial class EasyMainMenu : Page
    {
        private readonly PluginIntegration _pluginIntegration;

        [Obsolete("For designer only")]
        public EasyMainMenu() {
            InitializeComponent();
        }

        public EasyMainMenu(PluginIntegration pluginIntegration) {
            _pluginIntegration = pluginIntegration ?? throw new ArgumentNullException(nameof(pluginIntegration));

            InitializeComponent();

            this.Loaded += EasyMainMenu_Loaded;
        }

        private void EasyMainMenu_Loaded(object sender, RoutedEventArgs e) {
            using (var context = new DataContext()) {
                var game = context.Games().First();

                App.Current.MainWindow.Title = game.Name;

                var package = context.Packages.Find(
                    game.PackageId,
                    game.PackageVersion
                );

                var menuPluginRecord = context.GameMenuPlugins(package).First();

                var plugin = _pluginIntegration.Load<IMainMenuPlugin>(menuPluginRecord);

                var converter = new BrushConverter();
                var brush = (Brush)converter.ConvertFromString(plugin.Theme.Background);

                this.Background = brush;
            }
        }

        private void SignIn_Click(object sender, RoutedEventArgs e) {
            this.NavigationService.Navigate(new Login());
        }
    }
}
