using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Octgn.Networking;
using Octgn.Play;

namespace Octgn.Launcher
{
    using System.Threading.Tasks;
    using System.Windows.Documents;
    using System.Windows.Media;

    public partial class StartGame
    {
        private bool _startingGame;
        private readonly bool _isLocal;
        public StartGame(bool isLocal = false)
        {
            InitializeComponent();
            Player.OnLocalPlayerWelcomed += PlayerOnOnLocalPlayerWelcomed;
            _isLocal = isLocal;
            if (!isLocal)
            {
                this.HorizontalAlignment = HorizontalAlignment.Stretch;
                this.VerticalAlignment = VerticalAlignment.Stretch;
                this.Width = Double.NaN;
                this.Height = Double.NaN;
            }

            if (Program.IsHost)
            {
                descriptionLabel.Text =
                    "The following players have joined your game.\n\nClick 'Start' when everyone has joined. No one will be able to join once the game has started.";
                if (isLocal)
                {
                    descriptionLabel.Text += "\n\nHosting on port: " + Program.Client.Port;
                    GetIps();
                }
            }
            else
            {
                descriptionLabel.Text =
                    "The following players have joined the game.\nPlease wait until the game starts, or click 'Cancel' to leave this game.";
                startBtn.Visibility = Visibility.Collapsed;
                options.IsEnabled = playersList.IsEnabled = false;
            }

            Loaded += delegate
                          {
                              Program.GameSettings.UseTwoSidedTable = Prefs.TwoSidedTable;
                              Program.Dispatcher = Dispatcher;
                              Program.ServerError += HandshakeError;
                              Program.GameSettings.PropertyChanged += SettingsChanged;
                              // Fix: defer the call to Program.Game.Begin(), so that the trace has 
                              // time to connect to the ChatControl (done inside ChatControl.Loaded).
                              // Otherwise, messages notifying a disconnection may be lost
                              try
                              {
                                  if (Program.Game != null)
                                      Dispatcher.BeginInvoke(new Action(Program.Game.Begin));
                              }
                              catch (Exception)
                              {
                                  if (Debugger.IsAttached) Debugger.Break();
                              }
                          };
            Unloaded += delegate
                            {
                                if (_startingGame == false)
                                    Program.StopGame();
                                Program.GameSettings.PropertyChanged -= SettingsChanged;
                                Program.ServerError -= HandshakeError;
                            };
        }

        private void GetIps()
        {
            var task = new Task(GetLocalIps);
            task.ContinueWith(GetExternalIp);
            task.Start();
        }

        private void GetLocalIps()
        {
            try
            {
                var addr = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList;
                    this.Dispatcher.Invoke(
                        new Action(
                            () =>
                                {
                                    var paragraph = new Paragraph(new Run("--Local Ip's--")){Foreground = Brushes.Brown};
                                    foreach (var a in addr)
                                    {
                                        paragraph.Inlines.Add(new LineBreak());
                                        paragraph.Inlines.Add(new Run(a.ToString()));
                                    }
                                    this.chatControl.output.Document.Blocks.Add(paragraph);
                                    this.chatControl.output.ScrollToEnd();
                                }));
            }
            catch (Exception)
            {

            }
        }

        private void GetExternalIp(Task task)
        {
            try
            {
                const string Dyndns = "http://checkip.dyndns.org";
                var wc = new System.Net.WebClient();
                var utf8 = new System.Text.UTF8Encoding();
                var requestHtml = "";
                var ipAddress = "";
                requestHtml = utf8.GetString(wc.DownloadData(Dyndns));
                var fullStr = requestHtml.Split(':');
                ipAddress = fullStr[1].Remove(fullStr[1].IndexOf('<')).Trim();
                var externalIp = System.Net.IPAddress.Parse(ipAddress);
                this.Dispatcher.Invoke(
                    new Action(
                        () =>
                        {
                            var paragraph = new Paragraph(new Run("--Remote Ip--")) { Foreground = Brushes.Brown };
                            paragraph.Inlines.Add(new LineBreak());
                            paragraph.Inlines.Add(new Run(externalIp.ToString()));
                            this.chatControl.output.Document.Blocks.Add(paragraph);
                            this.chatControl.output.ScrollToEnd();
                        }));

            }
            catch (Exception)
            {

            }
        }

        private void PlayerOnOnLocalPlayerWelcomed()
        {
            if (Player.LocalPlayer.Id == 255) return;
            if (Player.LocalPlayer.Id == 1)
            {
                Dispatcher.BeginInvoke(new Action(() => { startBtn.Visibility = Visibility.Visible; }));
            }
        }
        private void SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            if (Program.IsHost)
                Program.Client.Rpc.Settings(Program.GameSettings.UseTwoSidedTable);
            cbTwoSided.IsChecked = Program.GameSettings.UseTwoSidedTable;
        }

        internal void Start()
        {
            _startingGame = true;
            // Reset the InvertedTable flags if they were set and they are not used
            if (!Program.GameSettings.UseTwoSidedTable)
                foreach (Player player in Player.AllExceptGlobal)
                    player.InvertedTable = false;

            // At start the global items belong to the player with the lowest id
            if (Player.GlobalPlayer != null)
            {
                Player host = Player.AllExceptGlobal.OrderBy(p => p.Id).First();
                foreach (Group group in Player.GlobalPlayer.Groups)
                    group.Controller = host;
            }

            if (Program.PlayWindow != null) return;
            Program.Client.Rpc.Start();
            Program.PlayWindow = new PlayWindow(_isLocal);
            Program.PlayWindow.Show();
            if (!_isLocal)
                Program.MainWindow.HostJoinTab();
            else
            {
                Program.LauncherWindow.Navigate(new Login());
                Program.LauncherWindow.Hide();
            }
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            if (_startingGame) return;
            _startingGame = true;
            Program.LobbyClient.HostedGameStarted();
            e.Handled = true;
            Start();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            Back();
        }

        private void Back()
        {
            if (!_isLocal)
                Program.MainWindow.HostJoinTab();
            else
            {
                Program.LauncherWindow.NavigationService.Navigate(new Login());
            }
        }

        private void HandshakeError(object sender, ServerErrorEventArgs e)
        {
            MessageBox.Show("The server returned an error:\n" + e.Message, "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
            e.Handled = true;
            Back();
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (cbTwoSided.IsChecked != null) Program.GameSettings.UseTwoSidedTable = cbTwoSided.IsChecked.Value;
        }
    }
}