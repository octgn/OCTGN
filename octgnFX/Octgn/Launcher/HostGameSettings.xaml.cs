using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Navigation;
using Octgn.Data;
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
        private Timer _startGameTimer;

        public HostGameSettings(Data.Game game)
        {
            InitializeComponent();
            _game = game;
            textBox1.Text = Prefs.LastRoomName;
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            Program.LobbyClient.OnDataRecieved += LobbyClientOnOnDataRecieved;
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            Program.LobbyClient.OnDataRecieved -= LobbyClientOnOnDataRecieved;
            if(_startGameTimer != null)
                _startGameTimer.Dispose();

        }


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
            lblError.Content = "";
            if (String.IsNullOrWhiteSpace(textBox1.Text))
            {
                lblError.Content = "Please enter a game name.";
                return;
            }
            if (_beginHost) return;
            Prefs.LastRoomName = textBox1.Text;
            _startGameTimer = new Timer((object a)=> EndHostGame(-1) ,null,10000,Timeout.Infinite);
            progressBar1.Visibility = Visibility.Visible;
            button1.IsEnabled = false;
            e.Handled = true;
            _beginHost = true;
            _ns = NavigationService;
            Program.LobbyClient.BeginHostGame(_game, textBox1.Text);
            
        }

        private void EndHostGame(int port)
        {
            if (port <= -1)
            {
                _startGameTimer.Dispose();
                _beginHost = false;
                Dispatcher.Invoke(new Action(() =>
                                             {
                                                 progressBar1.Visibility = Visibility.Hidden;
                                                 button1.IsEnabled = true;
                                                 lblError.Content = "Could not start game.";
                                             }));
                return;
            }
            Trace.WriteLine("Connecting to port: " + port.ToString(CultureInfo.InvariantCulture));
            Program.LobbyClient.CurrentHostedGamePort = port;
            Program.GameSettings.UseTwoSidedTable = true;
            Program.Game = new Game(GameDef.FromO8G(_game.FullPath));
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