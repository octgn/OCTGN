using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Octgn.Definitions;
using Octgn.Networking;
using Skylabs.Net;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for ContactList.xaml
    /// </summary>
    public partial class HostGameSettings : Page
    {
        private readonly Data.Game Game;
        private bool beginHost;
        private NavigationService ns;

        public HostGameSettings(Data.Game game)
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
            if (!beginHost)
            {
                e.Handled = true;
                beginHost = true;
                ns = NavigationService;
                Program.lobbyClient.BeginHostGame(EndHostGame, Game, textBox1.Text, textBox2.Text);
                Program.ClientWindow.HostJoinTab();
            }
        }

        private void EndHostGame(SocketMessage sm)
        {
            var port = (int) sm["port"];
            Program.DebugTrace.TraceEvent(TraceEventType.Information, 0,
                                          "Connecting to port: " + port.ToString(CultureInfo.InvariantCulture));
            Program.lobbyClient.CurrentHostedGamePort = port;
            if (port > -1)
            {
                Program.GameSettings.UseTwoSidedTable = true;
                Program.Game = new Game(GameDef.FromO8G(Game.Filename));
                Program.IsHost = true;
#if(DEBUG)
                var ad = new IPAddress[1];
                IPAddress ip = IPAddress.Parse("127.0.0.1");
#else
                var ad = Dns.GetHostAddresses(Program.LobbySettings.Server);
                IPAddress ip = ad[0];
#endif

                if (ad.Length > 0)
                {
                    Program.Client = new Client(ip, port);
                    Program.Client.Connect();
                    Dispatcher.Invoke(new Action(dothenavigate));
                }
            }
        }

        private void dothenavigate()
        {
            ns.Navigate(new StartGame());
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            Program.ClientWindow.HostJoinTab();
        }
    }
}