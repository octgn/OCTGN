using System;
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
            bError.Visibility = System.Windows.Visibility.Hidden;
            Program.lobbyClient = new Skylabs.Lobby.LobbyClient();
            bool c = Program.lobbyClient.Connect("localhost", int.Parse(Settings.Default.ServePort));
            if(c)
            {
                Program.lobbyClient.Login(LoginFinished, textBox1.Text, passwordBox1.Password);
            }
        }

        private void LoginFinished(bool success)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                circularProgressBar1.Visibility = System.Windows.Visibility.Hidden;
                if(success)
                {
                }
                else
                {
                    bError.Visibility = System.Windows.Visibility.Visible;
                }
            }), new object[0] { });
        }

        private void menuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }

        private void menuOctgn_Click(object sender, RoutedEventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            bError.Visibility = System.Windows.Visibility.Hidden;
        }

        private void passwordBox1_PasswordChanged(object sender, RoutedEventArgs e)
        {
            bError.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}