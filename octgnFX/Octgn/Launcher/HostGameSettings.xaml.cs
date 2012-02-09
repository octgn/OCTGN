using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using Octgn.Definitions;
using Octgn.Networking;
using Skylabs.Lobby.Sockets;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for ContactList.xaml
    /// </summary>
    public partial class HostGameSettings
    {
        private readonly Data.Game _game;
        private bool _beginHost;
        private NavigationService _ns;

        public HostGameSettings(Data.Game game)
        {
            InitializeComponent();
            _game = game;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
        }

        private void Button1Click(object sender, RoutedEventArgs e)
        {
            if (_beginHost) return;
            e.Handled = true;
            _beginHost = true;
            _ns = NavigationService;
            Program.LobbyClient.BeginHostGame(EndHostGame, _game, textBox1.Text, textBox2.Text);
            Program.ClientWindow.HostJoinTab();
        }

        private void EndHostGame(SocketMessage sm)
        {
            var port = (int) sm["port"];
            Program.DebugTrace.TraceEvent(TraceEventType.Information, 0,
                                          "Connecting to port: " + port.ToString(CultureInfo.InvariantCulture));
            Program.LobbyClient.CurrentHostedGamePort = port;
            if (port <= -1) return;
            Program.GameSettings.UseTwoSidedTable = true;
            Program.Game = new Game(GameDef.FromO8G(_game.Filename));
            Program.IsHost = true;
#if(DEBUG)
            var ad = new IPAddress[1];
            IPAddress ip = IPAddress.Parse("127.0.0.1");
#else
                var ad = Dns.GetHostAddresses(Program.LobbySettings.Server);
                IPAddress ip = ad[0];
#endif

            if (ad.Length <= 0) return;
            Program.Client = new Client(ip, port);
            Program.Client.Connect();
            Dispatcher.Invoke(new Action(DoTheNavigate));
        }

        private void DoTheNavigate()
        {
            _ns.Navigate(new StartGame());
        }

        private void Button2Click(object sender, RoutedEventArgs e)
        {
            Program.ClientWindow.HostJoinTab();
        }
    }
}