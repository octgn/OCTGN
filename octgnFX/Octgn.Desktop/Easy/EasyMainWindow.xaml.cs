using Octgn.Sdk.Extensibility;
using System;
using System.Windows;

namespace Octgn.Desktop.Easy
{
    public partial class EasyMainWindow : Window
    {
        public EasyMainWindow() {
            InitializeComponent();

            this.Loaded += EasyMainWindow_Loaded;
        }

        private void EasyMainWindow_Loaded(object sender, RoutedEventArgs e) {
            //TODO: Default display should be some kind of 'Loading' screen
            this.Content = new EasyMainMenu(new PluginIntegration());
        }
    }
}
