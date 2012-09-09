using System.Windows;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for ContactList.xaml
    /// </summary>
    public partial class AddFriendPage
    {
        public AddFriendPage()
        {
            InitializeComponent();
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button1Click(object sender, RoutedEventArgs e)
        {
            Program.LobbyClient.SendFriendRequest(textBox1.Text);
            //Program.MainWindow.LobbyTab();
        }

        private void Button2Click(object sender, RoutedEventArgs e)
        {
            //Program.MainWindow.LobbyTab();
        }
    }
}