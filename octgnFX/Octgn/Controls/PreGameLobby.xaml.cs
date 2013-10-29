using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Octgn.Controls
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Controls;

    using Octgn.Core;
    using Octgn.Networking;
    using Octgn.Play;

    /// <summary>
    /// Interaction logic for PreGameLobby.xaml
    /// </summary>
    public partial class PreGameLobby: UserControl ,IDisposable
    {
        public event Action<object> OnClose;

        protected virtual void FireOnClose(object obj)
        {
            var handler = this.OnClose;
            if (handler != null)
            {
                handler(obj);
            }
        }

        private bool _startingGame;
        private readonly bool _isLocal;

        public PreGameLobby(bool isLocal = false)
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
                    descriptionLabel.Text += "\n\nHosting on port: " + Program.Client.EndPoint.Port;
                    GetIps();

                    // save game/port so a new client can start up and connect
                    Prefs.LastLocalHostedGamePort = Program.Client.EndPoint.Port;
                    Prefs.LastHostedGameType = Program.GameEngine.Definition.Id;
                }
            }
            else
            {
                descriptionLabel.Text =
                    "The following players have joined the game.\nPlease wait until the game starts, or click 'Cancel' to leave this game.";
                startBtn.Visibility = Visibility.Collapsed;
                options.IsEnabled = playersList.IsEnabled = false;
            }
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_startingGame == false)
                    Program.StopGame();
            Program.GameSettings.PropertyChanged -= SettingsChanged;
            Program.ServerError -= HandshakeError;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            Loaded -= OnLoaded;
            //new KickstarterWindow().ShowDialog();
            Program.GameSettings.UseTwoSidedTable = Program.GameEngine.Definition.UseTwoSidedTable;
            cbTwoSided.IsChecked = Program.GameSettings.UseTwoSidedTable;
            Program.Dispatcher = Dispatcher;
            Program.ServerError += HandshakeError;
            Program.GameSettings.PropertyChanged += SettingsChanged;
            // Fix: defer the call to Program.Game.Begin(), so that the trace has 
            // time to connect to the ChatControl (done inside ChatControl.Loaded).
            // Otherwise, messages notifying a disconnection may be lost
            try
            {
                if (Program.GameEngine != null)
                    Dispatcher.BeginInvoke(new Action(()=>Program.GameEngine.Begin(false)));
            }
            catch (Exception)
            {
                if (Debugger.IsAttached) Debugger.Break();
            }
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
                            var paragraph = new Paragraph(new Run("--Local Ip's--")) { Foreground = Brushes.Brown };
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
                Program.Client.Rpc.Settings(Program.GameSettings.UseTwoSidedTable);
            }
            _startingGame = true;
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
            Program.StartGame();
            Back();
        }

        private void StartClicked(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;
            _startingGame = true;
            if(!_isLocal)
                Program.LobbyClient.HostedGameStarted();
            e.Handled = true;
            Start();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            _startingGame = false;
            e.Handled = true;
            Back();
        }

        private void Back()
        {
            this.FireOnClose(this);
        }

        private void HandshakeError(object sender, ServerErrorEventArgs e)
        {
            TopMostMessageBox.Show("The server returned an error:\n" + e.Message, "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
            e.Handled = true;
            Back();
        }

        private void CheckBoxClick(object sender, RoutedEventArgs e)
        {
            if (cbTwoSided.IsChecked != null) Program.GameSettings.UseTwoSidedTable = cbTwoSided.IsChecked.Value;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Player.OnLocalPlayerWelcomed -= PlayerOnOnLocalPlayerWelcomed;
            if (OnClose != null)
            {
                foreach (var d in OnClose.GetInvocationList())
                {
                    OnClose -= (Action<object>)d;
                }
            }
        }

        #endregion
    }
}
