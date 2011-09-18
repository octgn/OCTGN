using System.Windows;
using System.Windows.Controls;
using Octgn.Properties;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
            versionText.Text = string.Format("Version {0}", OctgnApp.OctgnVersion.ToString(4));
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            circularProgressBar1.Visibility = Visibility.Visible;
            Program.lobbyClient = new Skylabs.Lobby.LobbyClient();
            bool c = Program.lobbyClient.Connect("localhost", int.Parse(Settings.Default.ServePort));
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void menuOctgn_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}