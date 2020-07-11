namespace Octgn.Play
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for DeveloperWindow.xaml
    /// </summary>
    public partial class DeveloperWindow : Window
    {
        public DeveloperWindow()
        {
            this.InitializeComponent();
        }

        private void ButtonReloadScriptsClick(object sender, RoutedEventArgs e)
        {
            Program.GameEngine.ScriptEngine.ReloadScripts();
        }
    }
}
