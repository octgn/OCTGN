using Skylabs.Lobby;

namespace Octgn.Controls
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Forms;

    using Octgn.Core;
    using Octgn.Core.DataManagers;
    using Octgn.Networking;
    using Octgn.ViewModels;

    using UserControl = System.Windows.Controls.UserControl;

    public partial class ConnectOfflineGame : UserControl, IDisposable
    {
        public event Action<object, DialogResult> OnClose;
        protected virtual void FireOnClose(object sender, DialogResult result)
        {
            var handler = this.OnClose;
            if (handler != null)
            {
                handler(sender, result);
            }
        }
        public static DependencyProperty ErrorProperty = DependencyProperty.Register(
            "Error", typeof(String), typeof(ConnectOfflineGame));
        public bool HasErrors { get; private set; }
        private Decorator Placeholder;
        public string Error
        {
            get { return this.GetValue(ErrorProperty) as String; }
            private set
            {
                this.SetValue(ErrorProperty, value);
                if (string.IsNullOrWhiteSpace(value))
                {
                    ErrorBorder.Visibility = Visibility.Hidden;
                }
                else
                {
                    ErrorBorder.Visibility = Visibility.Visible;
                }
            }
        }



        public bool Successful { get; private set; }
        public DataNew.Entities.Game Game { get; private set; }
        public string Password { get; private set; }
        public bool Spectator { get; private set; }

        public ObservableCollection<DataGameViewModel> Games { get; private set; }
        public ConnectOfflineGame()
        {
            InitializeComponent();
            this.TextBoxPort.Text = Prefs.LastLocalHostedGamePort.ToString();
            Program.Dispatcher = WindowManager.Main.Dispatcher;
            Games = new ObservableCollection<DataGameViewModel>();
            Program.LobbyClient.OnLoginComplete += LobbyClientOnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClientOnDisconnect;
            TextBoxUserName.Text = (Program.LobbyClient.IsConnected == false
                || Program.LobbyClient.Me == null
                || Program.LobbyClient.Me.UserName == null) ? Prefs.Nickname : Program.LobbyClient.Me.UserName;
            TextBoxUserName.IsReadOnly = Program.LobbyClient.IsConnected;
        }

        private void LobbyClientOnDisconnect(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    TextBoxUserName.IsReadOnly = false;
                }));
        }

        private void LobbyClientOnLoginComplete(object sender, LoginResults results)
        {
            if (results != LoginResults.Success) return;
            Dispatcher.Invoke(new Action(() =>
            {
                TextBoxUserName.IsReadOnly = true;
                TextBoxUserName.Text = Program.LobbyClient.Me.UserName;
            }));
        }

        void RefreshInstalledGameList()
        {
            if (Games == null)
                Games = new ObservableCollection<DataGameViewModel>();
            var list = GameManager.Get().Games.Select(x => new DataGameViewModel(x)).ToList();
            Games.Clear();
            foreach (var l in list)
                Games.Add(l);
        }

        async Task Connect(string username, DataGameViewModel game, string userhost, string userport, string password, Guid gameId)
        {
            Successful = false;
            var endpoint = await this.ValidateFields(username, game, userhost, userport);

            Program.IsHost = false;
            Program.GameEngine = new Octgn.GameEngine(game.GetGame(), username,Spectator ,password, true);

            Program.Client = new ClientSocket(endpoint.Address, endpoint.Port, gameId);
            await Program.Client.Connect();
            Successful = true;
        }

        async Task<IPEndPoint> ValidateFields(string username, DataGameViewModel game, string host, string port)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username can't be empty");
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("You must enter a host name/ip");
            if (string.IsNullOrWhiteSpace(port)) throw new ArgumentException("You must enter a port number.");

            if (game == null)
                throw new ArgumentException("You must select a game");

            // validate host/ip
            IPAddress ip = null;
            try
            {
                ip = (await Dns.GetHostAddressesAsync(host))
                    .First(x => x.AddressFamily == AddressFamily.InterNetwork);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new ArgumentException("Ip/Host name is invalid, or unreachable");
            }

            var conPort = -1;

            if (!int.TryParse(port, out conPort))
                throw new ArgumentException("Port number is invalid");
            if (conPort <= 0 || conPort >= Int16.MaxValue)
                throw new ArgumentException("Port number is invalid");

            return new IPEndPoint(ip, conPort);
        }

        private void CheckBoxSpectator_OnChecked(object sender, RoutedEventArgs e)
        {
            Spectator = true;
        }

        private void CheckBoxSpectator_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Spectator = false;
        }

        #region Dialog
        public void Show(Decorator placeholder)
        {
            Placeholder = placeholder;
            RefreshInstalledGameList();
            placeholder.Child = this;
            var game = GameManager.Get().GetById(Prefs.LastHostedGameType);
            if (game != null)
            {
                ComboBoxGame.SelectedItem = Games.First(x => x.Id == game.Id);
            }
        }
        private void Close(DialogResult result)
        {
            Placeholder.Child = null;
            this.FireOnClose(this, result);
        }
        public void Close()
        {
            Close(DialogResult.Abort);
        }
        #endregion

        #region UI Events
        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close(DialogResult.Cancel);
        }

        private async void ButtonConnectClick(object sender, RoutedEventArgs e)
        {
            Error = "";
            var strHost = TextBoxHostName.Text;
            var strPort = TextBoxPort.Text;
            var game = ComboBoxGame.SelectedItem as DataGameViewModel;
            if (game == null) return;

            var username = TextBoxUserName.Text;
            var password = TextBoxPassword.Password ?? "";
            Guid gameId = Guid.Empty;
            if (!Guid.TryParse(txtGameId.Text.Trim(), out gameId)) return;

            this.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            Exception exception = null;
            try {
                await this.Connect(username, game, strHost, strPort, password, gameId);
            } catch (Exception ex) {
                exception = ex;
            }
            this.ProgressBar.Visibility = Visibility.Hidden;
            this.ProgressBar.IsIndeterminate = false;
            this.IsEnabled = true;

            if(exception != null) {
                if (exception is ArgumentException) {
                    this.Error = "Could not connect: " + exception.Message;
                } else this.Error = "Unknown Error";
                Successful = false;
                return;
            }
            this.Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame();
            this.Password = TextBoxPassword.Password ?? "";
            this.Close(DialogResult.OK);
        }
        #endregion

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (OnClose != null)
            {
                foreach (var d in OnClose.GetInvocationList())
                {
                    OnClose -= (Action<object, DialogResult>)d;
                }
            }
            Program.LobbyClient.OnLoginComplete -= LobbyClientOnLoginComplete;
            Program.LobbyClient.OnDisconnect -= LobbyClientOnDisconnect;
        }

        #endregion

        private void ButtonRandomizeUserNameClick(object sender, RoutedEventArgs e)
        {
            if (Program.LobbyClient.IsConnected == false)
                TextBoxUserName.Text = Randomness.GrabRandomJargonWord() + "-" + Randomness.GrabRandomNounWord();
        }
    }
}
