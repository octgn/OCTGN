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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Program.LobbyClient.AddFriend(textBox1.Text);
            Program.ClientWindow.LobbyTab();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Program.ClientWindow.LobbyTab();
        }
    }
}