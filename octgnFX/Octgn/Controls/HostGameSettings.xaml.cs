using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

using Octgn.Core;
using Octgn.Core.DataManagers;
using Octgn.Library.Exceptions;
using Octgn.Networking;
using Octgn.ViewModels;

using log4net;

using UserControl = System.Windows.Controls.UserControl;
using Octgn.Communication;
using Octgn.Library.Utils;
using Octgn.Extentions;
using Octgn.Library;
using Octgn.Online.Hosting;

namespace Octgn.Controls
{

    public partial class HostGameSettings : UserControl,IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public event Action<object, DialogResult> OnClose;
        protected virtual void FireOnClose(object sender, DialogResult result)
        {
            this.OnClose?.Invoke(sender, result);
        }

        public static DependencyProperty ErrorProperty = DependencyProperty.Register(
            "Error", typeof(String), typeof(HostGameSettings));

        public bool HasErrors { get; private set; }
        public string Error
        {
            get { return this.GetValue(ErrorProperty) as String; }
            private set { this.SetValue(ErrorProperty, value); }
        }

        public bool IsLocalGame { get; private set; }
        public string Gamename { get; private set; }
        public string Password { get; private set; }
        public string Username { get; set; }
        public bool Specators { get; set; }
        public DataNew.Entities.Game Game { get; private set; }
        public bool SuccessfulHost { get; private set; }

        private Decorator Placeholder;
        private Guid lastHostedGameType;

        public ObservableCollection<DataGameViewModel> Games { get; private set; }

        public HostGameSettings()
        {
            InitializeComponent();
            Specators = true;
            Program.IsHost = true;
            Games = new ObservableCollection<DataGameViewModel>();
            Program.LobbyClient.HostedGameReady += LobbyClient_HostedGameReady;
            Program.LobbyClient.Connected += LobbyClient_Connected;
            Program.LobbyClient.Disconnected += LobbyClient_Disconnected;
            TextBoxGameName.Text = Prefs.LastRoomName ?? Randomness.RandomRoomName();
            CheckBoxIsLocalGame.IsChecked = !Program.LobbyClient.IsConnected;
            CheckBoxIsLocalGame.IsEnabled = Program.LobbyClient.IsConnected;
            LabelIsLocalGame.IsEnabled = Program.LobbyClient.IsConnected;
            lastHostedGameType = Prefs.LastHostedGameType;
            TextBoxUserName.Text = (Program.LobbyClient.IsConnected == false
                || Program.LobbyClient.Me == null
                || Program.LobbyClient.Me.UserName == null) ? Prefs.Nickname : Program.LobbyClient.Me.UserName;
			Program.OnOptionsChanged += ProgramOnOptionsChanged;
            TextBoxUserName.IsReadOnly = Program.LobbyClient.IsConnected;
            if(Program.LobbyClient.IsConnected)
                PasswordGame.IsEnabled = SubscriptionModule.Get().IsSubscribed ?? false;
            else
            {
                PasswordGame.IsEnabled = true;
            }
            StackPanelIsLocalGame.Visibility = Prefs.EnableAdvancedOptions ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ProgramOnOptionsChanged()
        {
            StackPanelIsLocalGame.Visibility = Prefs.EnableAdvancedOptions ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LobbyClient_Disconnected(object sender, DisconnectedEventArgs args)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    CheckBoxIsLocalGame.IsChecked = true;
                    CheckBoxIsLocalGame.IsEnabled = false;
                    LabelIsLocalGame.IsEnabled = false;
                    TextBoxUserName.IsReadOnly = false;
                }));
        }

        private void LobbyClient_Connected(object sender, ConnectedEventArgs args)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    CheckBoxIsLocalGame.IsChecked = false;
                    CheckBoxIsLocalGame.IsEnabled = true;
                    LabelIsLocalGame.IsEnabled = true;
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

        void ValidateFields()
        {
            if (string.IsNullOrWhiteSpace(TextBoxGameName.Text))
                this.SetError("You must enter a game name");
            else if (ComboBoxGame.SelectedIndex == -1) this.SetError("You must select a game");
            else
            {
                if(String.IsNullOrWhiteSpace(PasswordGame.Password))
                    this.SetError();
                else
                {
                    if(PasswordGame.Password.Contains(":,:") || PasswordGame.Password.Contains("=") || PasswordGame.Password.Contains("-") || PasswordGame.Password.Contains(" "))
                        this.SetError("The password has invalid characters");
                    else
                        this.SetError();
                }
            }
        }

        void SetError(string error = "")
        {
            this.HasErrors = !string.IsNullOrWhiteSpace(error);
            Error = error;
        }

        #region LobbyEvents

        private async void LobbyClient_HostedGameReady(object sender, HostedGameReadyEventArgs e) {
            try
            {
                var gameData = e.Game;
                var game = this.Game;

                Program.GameEngine = new GameEngine(game,Program.LobbyClient.Me.UserName,false,this.Password);
                Program.IsHost = true;

                var hostAddress = Dns.GetHostAddresses(AppConfig.GameServerPath).First();

                // Should use gameData.IpAddress sometime.
                Program.Client = new ClientSocket(hostAddress, gameData.Port);
                await Program.Client.Connect();
                SuccessfulHost = true;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }

        }
        #endregion

        #region Dialog
        public void Show(Decorator placeholder)
        {
            Placeholder = placeholder;
            this.RefreshInstalledGameList();

            if (lastHostedGameType != Guid.Empty)
            {
                var game = GameManager.Get().Games.FirstOrDefault(x => x.Id == lastHostedGameType);
                if (game != null)
                {
                    var model = Games.FirstOrDefault(x => x.Id == game.Id);
                    if (model != null) this.ComboBoxGame.SelectedItem = model;
                }
            }

            placeholder.Child = this;
        }

        public void Close()
        {
            Close(DialogResult.Abort);
        }

        private void Close(DialogResult result)
        {
            Program.OnOptionsChanged -= ProgramOnOptionsChanged;
            Program.LobbyClient.HostedGameReady -= LobbyClient_HostedGameReady;
            IsLocalGame = CheckBoxIsLocalGame.IsChecked ?? false;
            Gamename = TextBoxGameName.Text;
            Password = PasswordGame.Password;
            if (ComboBoxGame.SelectedIndex != -1)
                Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame();
            Placeholder.Child = null;
            this.FireOnClose(this, result);
        }

        void StartWait()
        {
            BorderHostGame.IsEnabled = false;
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
        }

        void EndWait()
        {
            BorderHostGame.IsEnabled = true;
            ProgressBar.Visibility = Visibility.Hidden;
            ProgressBar.IsIndeterminate = false;
        }

        async Task StartLocalGame(DataNew.Entities.Game game, string name, string password)
        {
            var hostport = new Random().Next(5000,6000);
            while (!NetworkHelper.IsPortAvailable(hostport)) hostport++;

            var hg = new HostedGame() {
                Id = Guid.NewGuid(),
                Name = name,
                HostUserId = Program.LobbyClient?.Me.UserId,
                GameName = game.Name,
                GameId = game.Id,
                GameVersion = game.Version.ToString(),
                HostAddress = $"0.0.0.0:{hostport}",
                Password = password,
                GameIconUrl = game.IconUrl,
                HostUserIconUrl = Program.LobbyClient?.Me.ApiUser?.IconUrl,
                Spectators = true,
            };

            // Since it's a local game, we want to use the username instead of a userid, since that won't exist.
            var hs = new HostedGameProcess(hg, X.Instance.Debug, true);
            hs.Start();

            Prefs.Nickname = Username;
            Program.GameEngine = new GameEngine(game, Username, false, password, true);
            Program.CurrentOnlineGameName = name;
            Program.IsHost = true;

            var ip = IPAddress.Parse("127.0.0.1");

            for (var i = 0; i < 5; i++)
            {
                try
                {
                    Program.Client = new ClientSocket(ip, hostport);
                    await Program.Client.Connect();
                    SuccessfulHost = true;
                    return;
                }
                catch (Exception e)
                {
                    Log.Warn("Start local game error",e);
                    if (i == 4) throw;
                }
                Thread.Sleep(2000);
            }

        }

        async Task StartOnlineGame(DataNew.Entities.Game game, string name, string password)
        {
            var client = new Octgn.Site.Api.ApiClient();
            if (!await client.IsGameServerRunning(Prefs.Username, Prefs.Password.Decrypt()))
            {
                throw new UserMessageException("The game server is currently down. Please try again later.");
            }
            Program.CurrentOnlineGameName = name;
            // TODO: Replace this with a server-side check
            password = SubscriptionModule.Get().IsSubscribed == true ? password : String.Empty;

            var octgnVersion = typeof(Server.Server).Assembly.GetName().Version;

            var req = new HostedGame {
                GameId = game.Id,
                GameVersion = game.Version.ToString(),
                Name = name,
                GameName = game.Name,
                GameIconUrl = game.IconUrl,
                Password = password,
                HasPassword = !string.IsNullOrWhiteSpace(password),
                OctgnVersion = octgnVersion.ToString(),
                Spectators = Specators
            };

            var result = await Program.LobbyClient.HostGame(req);
            Program.CurrentHostedGame = result ?? throw new InvalidOperationException("HostGame returned a null");
            Program.GameEngine = new GameEngine(game, Program.LobbyClient.Me.UserName, false, this.Password);
            Program.IsHost = true;

            foreach(var address in Dns.GetHostAddresses(AppConfig.GameServerPath)) {
                try {
                    // Should use gameData.IpAddress sometime.
                    Log.Info($"{nameof(StartOnlineGame)}: Trying to connect to {address}:{result.Port}");

                    Program.Client = new ClientSocket(address, result.Port);
                    await Program.Client.Connect();
                    SuccessfulHost = true;
                    return;
                } catch (Exception ex) {
                    Log.Error($"{nameof(StartOnlineGame)}: Couldn't connect to address {address}:{result.Port}", ex);
                }
            }
            throw new InvalidOperationException($"Unable to connect to {AppConfig.GameServerPath}.{result.Port}");
        }

        #endregion

        #region UI Events
        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Close(DialogResult.Cancel);
        }

        private async void ButtonHostGameStartClick(object sender, RoutedEventArgs e)
        {
            this.ValidateFields();
            if (this.HasErrors) return;

            var error = "";
            try {
                Program.Dispatcher = this.Dispatcher;
                this.StartWait();
                this.Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame();
                this.Gamename = TextBoxGameName.Text;
                this.Password = PasswordGame.Password;
                this.Username = TextBoxUserName.Text;
                var isLocalGame = CheckBoxIsLocalGame?.IsChecked ?? false;

                //var startTime = DateTime.Now;

                if (isLocalGame) {
                    await StartLocalGame(Game, Gamename, Password);
                } else {
                    await StartOnlineGame(Game, Gamename, Password);
                }

                Prefs.LastRoomName = this.Gamename;
                Prefs.LastHostedGameType = this.Game.Id;

            } catch (Exception ex) {
                if (ex is UserMessageException) {
                    error = ex.Message;
                } else error = "There was a problem, please try again.";
                Log.Warn("Start Game Error", ex);
                SuccessfulHost = false;
            } finally {
                if (!string.IsNullOrWhiteSpace(error))
                    this.SetError(error);
                this.EndWait();
                if(SuccessfulHost)
                    this.Close(DialogResult.OK);
            }
        }

        private void ButtonRandomizeGameNameClick(object sender, RoutedEventArgs e)
        {
            TextBoxGameName.Text = Randomness.GrabRandomJargonWord() + " " + Randomness.GrabRandomNounWord();
        }

        private void ButtonRandomizeUserNameClick(object sender, RoutedEventArgs e)
        {
            if (Program.LobbyClient.IsConnected == false)
                TextBoxUserName.Text = Randomness.GrabRandomJargonWord() + "-" + Randomness.GrabRandomNounWord();
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
            Program.LobbyClient.HostedGameReady -= LobbyClient_HostedGameReady;
            Program.LobbyClient.Connected -= LobbyClient_Connected;
            Program.LobbyClient.Disconnected -= LobbyClient_Disconnected;
        }

        #endregion

        private void CheckBoxIsLocalGame_OnChecked(object sender, RoutedEventArgs e)
        {
            PasswordGame.IsEnabled = true;
        }

        private void CheckBoxIsLocalGame_OnUnchecked(object sender, RoutedEventArgs e)
        {
            PasswordGame.IsEnabled = SubscriptionModule.Get().IsSubscribed ?? false;
        }

        private void CheckBoxSpectators_OnChecked(object sender, RoutedEventArgs e)
        {
            Specators = true;
        }

        private void CheckBoxSpectators_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Specators = false;
        }
    }
}
