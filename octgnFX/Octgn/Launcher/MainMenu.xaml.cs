using System.Windows;
using System.Windows.Controls;
using Octgn.DeckBuilder;

namespace Octgn.Launcher
{
    public sealed partial class MainMenu : Page
    {
        public MainMenu()
        {
            InitializeComponent();
            versionText.Text = string.Format("Version {0}", OctgnApp.OctgnVersion.ToString(4));
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Serve());
        }

        private void JoinGame(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Join());
        }

        private void ManageGames(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GameManager());
        }

        private void EditDecks(object sender, RoutedEventArgs e)
        {
            if (Program.GamesRepository.Games.Count == 0)
            {
                MessageBox.Show("You have no game installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Window launcherWnd = Application.Current.MainWindow;
            var deckWnd = new DeckBuilderWindow();
            deckWnd.Show();
            Application.Current.MainWindow = deckWnd;
            launcherWnd.Close();
        }

        private void QuitClicked(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        #region Hint texts

        private void EnterBtn(object sender, RoutedEventArgs e)
        {
            var btn = (Button) sender;
            hintText.Text = (string) btn.Tag;
        }

        private void LeaveBtn(object sender, RoutedEventArgs e)
        {
            hintText.Text = "";
        }

        #endregion
    }
}