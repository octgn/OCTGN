using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Octgn.Controls;
using Octgn.Definitions;
using Skylabs.Lobby;
using Skylabs.Net;
using System.Net;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for ContactList.xaml
    /// </summary>
    public partial class HostGameSettings : Page
    {
        private Data.Game Game;
        private NavigationService ns;
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
            ns = NavigationService;
            Program.lobbyClient.BeginHostGame(EndHostGame,Game,textBox1.Text,textBox2.Text);
            NavigationService.GoBack();
        }
        private void EndHostGame(SocketMessage sm)
        {
            int port = (int)sm["port"];
            Program.lobbyClient.CurrentHostedGamePort = port;
            if(port > -1)
            {
                Program.GameSettings.UseTwoSidedTable = true;
                Program.Game = new Game(GameDef.FromO8G(Game.Filename));
                Program.IsHost = true;
                IPAddress[] ad = new IPAddress[0];
#if(DEBUG)
                ad = new IPAddress[1];
                IPAddress ip = IPAddress.Parse("127.0.0.1");
#else
                ad = Dns.GetHostAddresses("www.skylabsonline.com");
                IPAddress ip = ad[0];
#endif

                if (ad.Length > 0)
                {
                    Program.Client = new Networking.Client(ip, port);
                    Program.Client.Connect();
                    this.Dispatcher.Invoke(new Action(dothenavigate));
                    
                }
            }            
        }
        private void dothenavigate()
        {
            
            ns.Navigate(new StartGame());
        }
        private void button2_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
            
        }
    }
}