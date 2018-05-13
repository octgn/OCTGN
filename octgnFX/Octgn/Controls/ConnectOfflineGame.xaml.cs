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
using Octgn.Communication;
using Octgn.Library;
using Octgn.Library.Exceptions;

namespace Octgn.Controls
{

    public partial class ConnectOfflineGame : UserControl, IDisposable
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event Action<object, DialogResult> OnClose;
        protected virtual void FireOnClose(object sender, DialogResult result)
        {
            this.OnClose?.Invoke(sender, result);
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
            Program.LobbyClient.Connected += LobbyClient_Connected;
            Program.LobbyClient.Disconnected += LobbyClient_Disconnected;
            TextBoxUserName.Text = (Program.LobbyClient.IsConnected == false
                || Program.LobbyClient.User == null
                || Program.LobbyClient.User.DisplayName == null) ? Prefs.Nickname : Program.LobbyClient.User.DisplayName;
            TextBoxUserName.IsReadOnly = Program.LobbyClient.IsConnected;
        }

        private void LobbyClient_Disconnected(object sender, DisconnectedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    TextBoxUserName.IsReadOnly = false;
                }));
        }

        private void LobbyClient_Connected(object sender, ConnectedEventArgs args)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                TextBoxUserName.IsReadOnly = true;
                TextBoxUserName.Text = Program.LobbyClient.User.DisplayName;
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

        async Task Connect(string username, DataGameViewModel game, string userhost, string userport, string password)
        {
            Successful = false;
            var port = -1;
            this.ValidateFields(username, game, userhost, userport, password, out var host, out port);

            Program.IsHost = false;
            Program.GameEngine = new Octgn.GameEngine(game.GetGame(), username,Spectator ,password, true);

            Program.Client = new ClientSocket(host, port);
            await Program.Client.Connect();
            Successful = true;
        }

        void ValidateFields(string username, DataGameViewModel game, string host, string port, string password, out IPAddress ip, out int conPort)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username can't be empty");
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("You must enter a host name/ip");
            if (string.IsNullOrWhiteSpace(port)) throw new ArgumentException("You must enter a port number.");

            if (game == null)
                throw new ArgumentException("You must select a game");

            // validate host/ip
            try
            {
                ip = Dns.GetHostAddresses(host)
                    .First(x => x.AddressFamily == AddressFamily.InterNetwork);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw new ArgumentException("Ip/Host name is invalid, or unreachable");
            }

            conPort = -1;

            if (!int.TryParse(port, out conPort))
                throw new ArgumentException("Port number is invalid");
            if (conPort <= 0 || conPort >= Int16.MaxValue)
                throw new ArgumentException("Port number is invalid");
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

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close(DialogResult.Cancel);
        }

        private async void ButtonConnectClick(object sender, RoutedEventArgs e)
        {
            try {
                Error = "";
                var game = ComboBoxGame.SelectedItem as DataGameViewModel;
                if (game == null) return;

                var strHost = TextBoxHostName.Text;
                var strPort = TextBoxPort.Text;
                var username = TextBoxUserName.Text;
                var password = TextBoxPassword.Password ?? "";

                this.IsEnabled = false;
                ProgressBar.Visibility = Visibility.Visible;
                ProgressBar.IsIndeterminate = true;

                await this.Connect(username, game, strHost, strPort, password);

                this.Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame();
                this.Password = TextBoxPassword.Password ?? "";
                this.Close(DialogResult.OK);

            } catch (UserMessageException ex) {
                Log.Error(nameof(ButtonConnectClick), ex);
                Error = $"Could not connect: {ex.Message}";
            } catch (ArgumentException ex) {
                Log.Error(nameof(ButtonConnectClick), ex);
                Error = $"Could not connect: {ex.Message}";
            } catch (Exception ex) {
                Log.Error(nameof(ButtonConnectClick), ex);
                Error = "Could not connect: Unknown Error";
            } finally {
                this.ProgressBar.Visibility = Visibility.Hidden;
                this.ProgressBar.IsIndeterminate = false;
                this.IsEnabled = true;
            }
        }

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
            Program.LobbyClient.Connected -= LobbyClient_Connected;
            Program.LobbyClient.Disconnected -= LobbyClient_Disconnected;
        }

        #endregion

        private void ButtonRandomizeUserNameClick(object sender, RoutedEventArgs e)
        {
            if (Program.LobbyClient.IsConnected == false)
                TextBoxUserName.Text = Randomness.GrabRandomJargonWord() + "-" + Randomness.GrabRandomNounWord();
        }
    }
}
