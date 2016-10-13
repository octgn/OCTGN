/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

using Octgn.Core;
using Octgn.Core.DataManagers;
using Octgn.Library.Exceptions;
using Octgn.Networking;
using Octgn.ViewModels;

using Skylabs.Lobby;

using log4net;

using Octgn.Online.Library.Models;
using Octgn.Hosting;

namespace Octgn.Controls
{
    public partial class HostGameSettings : OverlayDialog, IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public event Action<object, DialogResult> OnClose;
        protected virtual void FireOnClose(object sender, DialogResult result)
        {
            this.OnClose?.Invoke(sender, result);
        }

        public static readonly DependencyProperty ErrorProperty =
             DependencyProperty.Register(nameof(Error), typeof(string),
             typeof(HostGameSettings), new FrameworkPropertyMetadata(string.Empty));

        public string Error {
            get { return (string)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        public bool HasErrors { get; private set; }
        public bool IsLocalGame { get; private set; }
        public string Gamename { get; private set; }
        public string Password { get; private set; }
        public string Username { get; set; }
        public DataNew.Entities.Game Game { get; private set; }
        public bool SuccessfulHost { get; private set; }

        private Decorator Placeholder;
        private Guid lastHostedGameType;

        public ObservableCollection<DataGameViewModel> Games { get; private set; }

        public HostGameSettings()
        {
            InitializeComponent();
            Program.IsHost = true;
            Games = new ObservableCollection<DataGameViewModel>();
            Program.LobbyClient.OnDataReceived += LobbyClientOnDataReceviedCaller;
            Program.LobbyClient.OnLoginComplete += LobbyClientOnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClientOnDisconnect;
            TextBoxGameName.Text = Prefs.LastRoomName ?? Skylabs.Lobby.Randomness.RandomRoomName();

            HostOnlineSlider.IsChecked = Program.LobbyClient.IsConnected;
            HostOnlineSlider.IsEnabled = Program.LobbyClient.IsConnected;

            lastHostedGameType = Prefs.LastHostedGameType;
            TextBoxUserName.Text = (Program.LobbyClient.IsConnected == false
                || Program.LobbyClient.Me == null
                || Program.LobbyClient.Me.UserName == null) ? Prefs.Nickname : Program.LobbyClient.Me.UserName;
            Program.OnOptionsChanged += ProgramOnOptionsChanged;
            TextBoxUserName.IsReadOnly = Program.LobbyClient.IsConnected;
            if (Program.LobbyClient.IsConnected)
                PasswordGame.IsEnabled = SubscriptionModule.Get().IsSubscribed ?? false;
            else
            {
                PasswordGame.IsEnabled = true;
            }
            HostOnlineSlider.Visibility = Prefs.EnableAdvancedOptions ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ProgramOnOptionsChanged()
        {
            HostOnlineSlider.Visibility = Prefs.EnableAdvancedOptions ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LobbyClientOnDisconnect(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    HostOnlineSlider.IsChecked = false;
                    HostOnlineSlider.IsEnabled = false;
                    TextBoxUserName.IsReadOnly = false;
                }));
        }

        private void LobbyClientOnLoginComplete(object sender, LoginResults results)
        {
            if (results != LoginResults.Success) return;
            Dispatcher.Invoke(new Action(() =>
                {
                    HostOnlineSlider.IsChecked = true;
                    HostOnlineSlider.IsEnabled = true;
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
            {
                this.SetError("You must enter a game name");
                return;
            }
            if (ComboBoxGame.SelectedIndex == -1)
            {
                this.SetError("You must select a game");
                return;
            }
            if (string.IsNullOrWhiteSpace(TextBoxUserName.Text))
            {
                this.SetError("You must enter a user name");
                return;
            }
            if (String.IsNullOrWhiteSpace(PasswordGame.Password))
            {
                // No password, so we don't need to validate it...Good work boys
                this.SetError();
                return;
            }
            if (PasswordGame.Password.Contains(":,:") || PasswordGame.Password.Contains("=") || PasswordGame.Password.Contains("-") || PasswordGame.Password.Contains(" "))
            {
                this.SetError("The password has invalid characters");
                return;
            }
            // yay no error
            this.SetError();
        }

        void SetError(string error = "")
        {
            this.HasErrors = !string.IsNullOrWhiteSpace(error);
            Error = error;
        }

        #region LobbyEvents
        private void LobbyClientOnDataReceviedCaller(object sender, DataRecType type, object data)
        {
            try
            {
                if (type == DataRecType.HostedGameReady)
                {
                    var gameData = data as HostedGameData;
                    if (gameData == null)
                        throw new Exception("Could not start game.");
                    var game = this.Game;
                    Program.LobbyClient.CurrentHostedGamePort = (int)gameData.Port;
                    //Program.GameSettings.UseTwoSidedTable = true;
                    Program.GameEngine = new GameEngine(game, Program.LobbyClient.Me.UserName, false, this.Password);
                    Program.IsHost = true;

                    var hostAddress = Dns.GetHostAddresses(AppConfig.GameServerPath).First();

                    // Should use gameData.IpAddress sometime.
                    Program.Client = new ClientSocket(hostAddress, (int)gameData.Port, gameData.Id);
                    Program.Client.Connect();
                    SuccessfulHost = true;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
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
            Program.LobbyClient.OnDataReceived -= LobbyClientOnDataReceviedCaller;
            IsLocalGame = !HostOnlineSlider.IsChecked;
            Gamename = TextBoxGameName.Text;
            Password = PasswordGame.Password;
            if (ComboBoxGame.SelectedIndex != -1)
                Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame();
            Placeholder.Child = null;
            this.FireOnClose(this, result);
        }

        void StartWait()
        {
            GridHostGame.IsEnabled = false;
            ProgressBar.IsIndeterminate = true;
        }

        void EndWait()
        {
            GridHostGame.IsEnabled = true;
            ProgressBar.IsIndeterminate = false;
        }

        async Task StartLocalGame(DataNew.Entities.Game game, string name, string password)
        {
            var hg = new HostedGameRequest
            {
                Id = Guid.NewGuid(),
                AcceptingPlayers = true,
                GameIconUrl = game.IconUrl,
                GameId = game.Id,
                GameName = game.Name,
                GameVersion = game.Version.ToString(),
                HasPassword = string.IsNullOrWhiteSpace(password),
                Name = name,
                Password = password,
                Spectators = true,
                HostUserName = Username,
                MuteSpectators = false,
                TwoSidedTable = game.UseTwoSidedTable
            };
            var state = GameServer.Instance.HostGame(hg);
            Prefs.Nickname = Username;
            Program.LobbyClient.CurrentHostedGamePort = GameServer.Instance.ConnectionUri.Port;
            Program.GameEngine = new GameEngine(game, Username, false, password, true);
            Program.CurrentOnlineGameName = name;
            Program.IsHost = true;

            for (var i = 0; i < 5; i++)
            {
                try
                {
                    Program.Client = new ClientSocket(IPAddress.Loopback, GameServer.Instance.ConnectionUri.Port, state.Id);
                    await Program.Client.Connect();
                    SuccessfulHost = true;
                    return;
                }
                catch (Exception e)
                {
                    Log.Warn("Start local game error", e);
                    if (i == 4) throw;
                }
                await Task.Delay(2000);
            }

        }

        async Task StartOnlineGame(DataNew.Entities.Game game, string name, string password)
        {
            var client = new Octgn.Site.Api.ApiClient();
            if (!client.IsGameServerRunning(Program.LobbyClient.Username, Program.LobbyClient.Password))
            {
                throw new UserMessageException("The game server is currently down. Please try again later.");
            }
            Program.CurrentOnlineGameName = name;
            // TODO: Replace this with a server-side check
            password = SubscriptionModule.Get().IsSubscribed == true ? password : String.Empty;
            Program.LobbyClient.BeginHostGame(game, name, password, game.Name, game.IconUrl,
                typeof(Octgn.Server.Game).Assembly.GetName().Version, true);
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
            Program.Dispatcher = this.Dispatcher;
            if (this.HasErrors) return;
            this.StartWait();
            this.Game = (ComboBoxGame.SelectedItem as DataGameViewModel).GetGame();
            this.Gamename = TextBoxGameName.Text;
            this.Password = PasswordGame.Password;
            this.Username = TextBoxUserName.Text;
            var isLocalGame = !HostOnlineSlider.IsChecked;

            Prefs.LastRoomName = this.Gamename;
            Prefs.LastHostedGameType = this.Game.Id;

            var error = string.Empty;

            try
            {
                if (isLocalGame) await StartLocalGame(Game, Gamename, Password);
                else await StartOnlineGame(Game, Gamename, Password);
            }
            catch (UserMessageException ex)
            {
                SuccessfulHost = false;
                error = ex.Message;
                Log.Warn("Start Game Error");
            }
            catch (Exception ex)
            {
                SuccessfulHost = false;
                error = "There was a problem, please try again.";
                Log.Error("StartGameError", ex);
            }

            if (string.IsNullOrEmpty(error))
            {
                await Task.Run(async () =>
                {
                    var startTime = DateTime.Now;
                    while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMinutes <= 1)
                    {
                        if (SuccessfulHost) break;
                        await Task.Delay(1000);
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(error))
                this.SetError(error);
            this.EndWait();
            if (SuccessfulHost)
                this.Close(DialogResult.OK);
        }

        private void ButtonRandomizeGameNameClick(object sender, RoutedEventArgs e)
        {
            TextBoxGameName.Text = Skylabs.Lobby.Randomness.GrabRandomJargonWord() + " " + Randomness.GrabRandomNounWord();
        }

        private void ButtonRandomizeUserNameClick(object sender, RoutedEventArgs e)
        {
            if (!Program.LobbyClient.IsConnected)
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
            Program.LobbyClient.OnDataReceived -= LobbyClientOnDataReceviedCaller;
            Program.LobbyClient.OnLoginComplete -= LobbyClientOnLoginComplete;
            Program.LobbyClient.OnDisconnect -= LobbyClientOnDisconnect;
        }

        #endregion

        private void CheckBoxIsLocalGame_OnChecked(object sender, RoutedEventArgs e)
        {
            PasswordGame.IsEnabled = SubscriptionModule.Get().IsSubscribed ?? false;
        }

        private void CheckBoxIsLocalGame_OnUnchecked(object sender, RoutedEventArgs e)
        {
            PasswordGame.IsEnabled = true;
        }
    }
}
