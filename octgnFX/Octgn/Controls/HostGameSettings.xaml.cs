using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Octgn.Core;
using Octgn.Core.DataManagers;
using Octgn.Library.Exceptions;
using Octgn.ViewModels;
using log4net;
using UserControl = System.Windows.Controls.UserControl;
using Octgn.Communication;
using Octgn.Extentions;
using Octgn.Library;
using Octgn.Online.Hosting;
using Octgn.Online;
using System.ComponentModel;
using System.Windows.Media.Animation;

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

        public bool HasError {
            get { return (bool)GetValue(HasErrorProperty); }
            set { SetValue(HasErrorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HasError.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register("HasError", typeof(bool), typeof(HostGameSettings), new PropertyMetadata(false));

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

            if (DesignerProperties.GetIsInDesignMode(this)) return;

            this.Opacity = 0;
            ErrorMessageBorder.Child.Opacity = 0;
            this.Margin = new Thickness(0, -60, 0, 0);

            Specators = true;
            Program.IsHost = true;
            Games = new ObservableCollection<DataGameViewModel>();
            Program.LobbyClient.Connected += LobbyClient_Connected;
            Program.LobbyClient.Disconnected += LobbyClient_Disconnected;
            TextBoxGameName.Text = Prefs.LastRoomName ?? Randomness.RandomRoomName();
            CheckBoxIsLocalGame.IsChecked = !Program.LobbyClient.IsConnected;
            CheckBoxIsLocalGame.IsEnabled = Program.LobbyClient.IsConnected;
            LabelIsLocalGame.IsEnabled = Program.LobbyClient.IsConnected;
            lastHostedGameType = Prefs.LastHostedGameType;
            if (GameManager.Get().GameCount == 1)
            {
                lastHostedGameType = GameManager.Get().Games.First().Id;
            }
            TextBoxUserName.Text = (Program.LobbyClient.IsConnected == false
                || Program.LobbyClient.User == null
                || Program.LobbyClient.User.DisplayName == null) ? Prefs.Nickname : Program.LobbyClient.User.DisplayName;
			Program.OnOptionsChanged += ProgramOnOptionsChanged;
            TextBoxUserName.IsReadOnly = Program.LobbyClient.IsConnected;
            if(Program.LobbyClient.IsConnected)
                PasswordGame.IsEnabled = SubscriptionModule.Get().IsSubscribed ?? false;
            else
            {
                PasswordGame.IsEnabled = true;
            }
            StackPanelIsLocalGame.Visibility = Prefs.EnableLanGames ? Visibility.Visible : Visibility.Collapsed;

            SetError();

            this.Loaded += HostGameSettings_Loaded;
        }

        private void HostGameSettings_Loaded(object sender, RoutedEventArgs e) {
            this.Loaded -= HostGameSettings_Loaded;

            var ease = new CubicEase();
            ease.EasingMode = EasingMode.EaseIn;

            var animation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(200)));
            animation.EasingFunction = ease;
            animation.FillBehavior = FillBehavior.HoldEnd;

            BeginAnimation(OpacityProperty, animation);

            var ease2 = new CubicEase();
            ease2.EasingMode = EasingMode.EaseOut;

            var animation2 = new ThicknessAnimation(new Thickness(0, -100, 0, 0), new Thickness(0), new Duration(TimeSpan.FromMilliseconds(200)));
            animation2.EasingFunction = ease2;
            animation2.FillBehavior = FillBehavior.HoldEnd;

            BeginAnimation(MarginProperty, animation2);
        }

        private void ProgramOnOptionsChanged()
        {
            StackPanelIsLocalGame.Visibility = Prefs.EnableLanGames ? Visibility.Visible : Visibility.Collapsed;
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
            Error = error;
            this.HasError = !string.IsNullOrWhiteSpace(Error);
            ErrorText.Text = Error;
        }

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
            IsLocalGame = CheckBoxIsLocalGame.IsChecked ?? false;
            Gamename = TextBoxGameName.Text;
            Password = PasswordGame.Password;
            if (ComboBoxGame.SelectedIndex != -1)
                Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame();

            var animation = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(200)));
            animation.FillBehavior = FillBehavior.HoldEnd;

            BeginAnimation(OpacityProperty, animation);

            var animation2 = new ThicknessAnimation(new Thickness(0), new Thickness(0, -60, 0, 0), new Duration(TimeSpan.FromMilliseconds(500)));
            animation2.FillBehavior = FillBehavior.HoldEnd;

            BeginAnimation(MarginProperty, animation2);

            Dispatcher.InvokeAsync(async () => {
                await Task.Delay(1000);

                Placeholder.Child = null;
                this.FireOnClose(this, result);
            });
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

        Task<bool> StartLocalGame(DataNew.Entities.Game game, string name, string password)
        {
            var octgnVersion = typeof(Server.Server).Assembly.GetName().Version;

            var user = Program.LobbyClient?.User
                ?? new User(Guid.NewGuid().ToString(), Username);

            var username = user.DisplayName;

            var hg = new HostedGame() {
                Id = Guid.NewGuid(),
                Name = name,
                HostUser = user,
                GameName = game.Name,
                GameId = game.Id,
                GameVersion = game.Version.ToString(),
                HostAddress = $"0.0.0.0:{Prefs.LastLocalHostedGamePort}",
                Password = password,
                OctgnVersion = octgnVersion.ToString(),
                GameIconUrl = game.IconUrl,
                Spectators = true,
                DateCreated = DateTimeOffset.Now
            };
            if (Program.LobbyClient?.User != null) {
                hg.HostUserIconUrl = ApiUserCache.Instance.ApiUser(Program.LobbyClient.User)?.IconUrl;
            }

            return Program.JodsEngine.HostGame(hg, username, password);
        }

        async Task<bool> StartOnlineGame(DataNew.Entities.Game game, string name, string password)
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

            var lobbyClient = Program.LobbyClient ?? throw new InvalidOperationException("lobby client null");

            HostedGame result = null;
            try {
                result = await lobbyClient.HostGame(req);
            } catch (ErrorResponseException ex) {
                if (ex.Code != ErrorResponseCodes.UserOffline) throw;
                throw new UserMessageException("The Game Service is currently offline. Please try again.");
            }

            var launchedEngine = await Program.JodsEngine.HostGame(
                result,
                lobbyClient.User.DisplayName,
                Password
            );

            return launchedEngine;
        }

        #endregion

        #region UI Events
        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.Game = GameSelector.Game;

            this.Close(DialogResult.Cancel);
        }

        private async void ButtonHostGameStartClick(object sender, RoutedEventArgs e)
        {
            this.ValidateFields();
            if (this.HasError) return;

            var error = "";
            try {
                Program.Dispatcher = this.Dispatcher;
                this.StartWait();
                this.Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame();
                this.Game = GameSelector.Game;
                this.Gamename = TextBoxGameName.Text;
                this.Password = PasswordGame.Password;
                this.Username = TextBoxUserName.Text;
                var isLocalGame = CheckBoxIsLocalGame?.IsChecked ?? false;

                //var startTime = DateTime.Now;

                bool result;
                if (isLocalGame) {
                    result = await Task.Run(() => StartLocalGame(Game, Gamename, Password));
                } else {
                    result = await Task.Run(() => StartOnlineGame(Game, Gamename, Password));
                }

                if (!result) {
                    Log.Warn("Failed to start engine");

                    error = "Engine could not be started, please try again. If this continues to happen, please let us know.";

                    SuccessfulHost = false;
                } else {
                    Prefs.LastRoomName = this.Gamename;
                    Prefs.LastHostedGameType = this.Game.Id;
                    SuccessfulHost = true;
                }
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
