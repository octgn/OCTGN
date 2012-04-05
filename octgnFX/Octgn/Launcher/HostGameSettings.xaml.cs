using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Windows;
using System.Windows.Navigation;
using Octgn.Definitions;
using Octgn.Networking;
using Skylabs.Net;

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
            Program.LobbyClient.OnDataRecieved += LobbyClientOnOnDataRecieved;
        }

        private void PageUnloaded(object sender, RoutedEventArgs e) { Program.LobbyClient.OnDataRecieved -= LobbyClientOnOnDataRecieved; }


        private void LobbyClientOnOnDataRecieved(object sender , Skylabs.Lobby.Client.DataRecType type , object data)
        {
            if (type == Skylabs.Lobby.Client.DataRecType.HostedGameReady)
            {
                var port = data as Int32?;
                if (port != null)
                {
                    EndHostGame((int)port);
                    return;
                }
                EndHostGame(-1);
            }            

        }

        private void Button1Click(object sender, RoutedEventArgs e)
        {
            if (_beginHost) return;
            e.Handled = true;
            _beginHost = true;
            _ns = NavigationService;
            Program.LobbyClient.BeginHostGame(_game, textBox1.Text);
            
        }

        private void EndHostGame(int port)
        {
            Program.DebugTrace.TraceEvent(TraceEventType.Information, 0,
                                          "Connecting to port: " + port.ToString(CultureInfo.InvariantCulture));
            Program.LobbyClient.CurrentHostedGamePort = port;
            if (port <= -1) return;//TODO Somekind of user shit to tell them game couldn't be hosted.
            Program.GameSettings.UseTwoSidedTable = true;
            Program.Game = new Game(GameDef.FromO8G(_game.Filename));
            Program.IsHost = true;
#if(DEBUG)
            var ad = new IPAddress[1];
            IPAddress ip = IPAddress.Parse("127.0.0.1");
#else
            var ad = Dns.GetHostAddresses("www.skylabsonline.com");
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
            Program.MainWindow.HostJoinTab();
        }
    }
}