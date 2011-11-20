using System;
using System.Windows;
using System.Windows.Controls;
using Octgn.Controls;
using Skylabs.Lobby;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for ContactList.xaml
    /// </summary>
    public partial class HostGameSettings : Page
    {
        private Data.Game Game;
        public HostGameSettings(Octgn.Data.Game game)
        {
            InitializeComponent();
            Game = game;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.AddFriend(textBox1.Text);
            NavigationService.GoBack();
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
            
        }
    }
}