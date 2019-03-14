/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Octgn.Library.Networking;
using Octgn.Site.Api;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Scripting.Utils;
using Octgn.Core;
using Octgn.Core.DataManagers;
using Octgn.Library.Exceptions;
using Octgn.Networking;
using Octgn.Play;
using Octgn.Scripting.Controls;
using MessageBox = System.Windows.Forms.MessageBox;
using Octgn.Extentions;
using Octgn.Communication;
using Octgn.Online.Hosting;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Octgn.Controls;

namespace Octgn.Tabs.Play
{
    public partial class PlayTab : INotifyPropertyChanged, IDisposable
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ObservableCollection<HostedGameViewModel> HostedGameList { get; set; }

        public bool Spectate {
            get => _spectate;
            set {
                if (NotifyAndUpdate(ref _spectate, value)) {
                    Prefs.SpectateGames = _spectate;
                    foreach (var g in HostedGameList.ToArray()) {
                        g.UpdateVisibility();
                    }
                }
            }
        }

        public bool ShowUninstalledGames {
            get => _showUninstalledGames;
            set {
                if (NotifyAndUpdate(ref _showUninstalledGames, value)) {
                    Prefs.HideUninstalledGamesInList = _showUninstalledGames == false;
                    foreach (var g in HostedGameList.ToArray()) {
                        g.UpdateVisibility();
                    }
                }
            }
        }

        public bool ShowKillGameButton {
            get => _showKillGameButton;
            set => NotifyAndUpdate(ref _showKillGameButton, value);
        }

        private readonly GameBroadcastListener broadcastListener;

        private bool _spectate;
        private bool _showUninstalledGames;
        private bool _showKillGameButton;
        public PlayTab() {
            InitializeComponent();
            broadcastListener = new GameBroadcastListener();
            broadcastListener.StartListening();
            HostedGameList = new ObservableCollection<HostedGameViewModel>();
            Program.LobbyClient.Disconnected += LobbyClient_OnDisconnect;

            Spectate = Prefs.SpectateGames;
            ShowKillGameButton = Prefs.IsAdmin;
            ShowUninstalledGames = Prefs.HideUninstalledGamesInList == false;

            _refreshGameListTimer = new DispatcherTimer(InitialRefreshDelay.TimeSpan, DispatcherPriority.Normal, RefreshGameListTimer_Tick, Dispatcher);
        }

        #region Game List Refreshing

        public bool IsRefreshingGameList {
            get => _isRefreshingGameList;
            set => NotifyAndUpdate(ref _isRefreshingGameList, value);
        }

        private bool _isRefreshingGameList;
        private readonly DispatcherTimer _refreshGameListTimer;

        public static Duration InitialRefreshDelay { get; } = new Duration(TimeSpan.FromSeconds(2));
        public static Duration NormalRefreshDelay { get; } = new Duration(TimeSpan.FromSeconds(15));

        public Duration CurrentRefreshDelay {
            get => _currentRefreshDelay;
            set {
                if (!NotifyAndUpdate(ref _currentRefreshDelay, value)) return;
                OnPropertyChanged(nameof(IsInitialRefresh));
                _refreshGameListTimer.Interval = value.TimeSpan;
            }
        }

        public bool IsInitialRefresh => CurrentRefreshDelay.TimeSpan == InitialRefreshDelay.TimeSpan;

        private Duration _currentRefreshDelay = new Duration(TimeSpan.FromDays(10));


        private async void RefreshGameListTimer_Tick(object sender, EventArgs e) {
            ShowKillGameButton = Prefs.IsAdmin;

            try {
                IsRefreshingGameList = true;

                _refreshGameListTimer.IsEnabled = false;

                if(CurrentRefreshDelay == InitialRefreshDelay) {
                    CurrentRefreshDelay = NormalRefreshDelay;
                }

                var games = new List<HostedGameViewModel>();

                // Add online hosted games
                if (Program.LobbyClient?.IsConnected == true) {
                    var client = new ApiClient();
                    var hostedGames = await client.GetGameList();
                    games.AddRange(hostedGames.Select(x=>new HostedGameViewModel(x)));
                }

                // Add local and lan games
                games.AddRange(broadcastListener.Games.Select(x => new HostedGameViewModel(x)));

                // /////// Update the visual list

                // Remove the games that don't exist anymore
                var removeList = HostedGameList.Where(i => games.All(x => x.Id != i.Id)).ToList();
                removeList.ForEach(x => HostedGameList.Remove(x));

                // Add games that don't already exist in the list
                var addList = games.Where(i => this.HostedGameList.All(x => x.Id != i.Id)).ToList();
                HostedGameList.AddRange(addList);

                // Update all the existing items with new data
                var dbgames = GameManager.Get().Games.ToArray();
                foreach (var g in HostedGameList) {
                    var li = games.FirstOrDefault(x => x.Id == g.Id);
                    g.Update(li, dbgames);
                }
            } catch (Exception ex) {
                Log.Warn(nameof(RefreshGameListTimer_Tick), ex);
            } finally {
                IsRefreshingGameList = false;
                _refreshGameListTimer.IsEnabled = true;
            }
        }

        public void VisibleChanged(bool visible) {
            // Switching the interval on this timer allows the list to refresh quickly initially when the tab is ever viewed, then it'll wait the normal delay
            if(visible)
                CurrentRefreshDelay = InitialRefreshDelay;

            _refreshGameListTimer.IsEnabled = visible;

        }

        void LobbyClient_OnDisconnect(object sender, DisconnectedEventArgs e) {
            Log.Info("Disconnected");
            Dispatcher.Invoke(new Action(() => this.HostedGameList.Clear()));
        }

        #endregion Game List Refreshing

        #region Host Game

        private void ButtonHostClick(object sender, RoutedEventArgs e) {
            try {
                if (WindowManager.PlayWindow != null) {
                    throw new UserMessageException("You are currently in a game or game lobby. Please leave before you host a game.");
                }

                HostGameSettings dialog = null;
                try {
                    dialog = new HostGameSettings();
                    dialog.Show(DialogPlaceHolder);
                    dialog.OnClose += HostGameSettingsDialogOnClose;
                    BorderButtons.IsEnabled = false;
                } catch {
                    dialog.OnClose -= HostGameSettingsDialogOnClose;
                    dialog.Dispose();
                    BorderButtons.IsEnabled = true;
                    throw;
                }
            } catch (Exception ex) {
                HandleException(ex);
            }
        }

        private void HostGameSettingsDialogOnClose(object sender, DialogResult dialogResult) {
            BorderButtons.IsEnabled = true;
            using (var dialog = sender as HostGameSettings) {
                dialog.OnClose -= ConnectOfflineGameDialogOnClose;

                if (dialogResult == DialogResult.OK) {
                    if (dialog.SuccessfulHost) {
                        LaunchPlayWindow();
                    }
                }
            }
        }

        #endregion Host Game

        #region Join Game

        public static DependencyProperty IsJoinableGameSelectedProperty = DependencyProperty.Register(
            nameof(IsJoinableGameSelected), typeof(bool), typeof(PlayTab));
        public bool IsJoinableGameSelected {
            get => (bool)this.GetValue(IsJoinableGameSelectedProperty);
            private set => SetValue(IsJoinableGameSelectedProperty, value);
        }

        private void ListViewGameListSelectionChanged(object sender, SelectionChangedEventArgs e) {
            Log.Info("Changed game selection");
            var game = ListViewGameList.SelectedItem as HostedGameViewModel;
            this.IsJoinableGameSelected = game?.CanPlay == true;

            if (game != null)
                Log.InfoFormat("Selected game {0} {1}", game.GameId, game.Name);
        }

        private async void GameListItemDoubleClick(object sender, MouseButtonEventArgs e) {
            await JoinGame();
        }

        private async void ButtonJoinClick(object sender, RoutedEventArgs e) {
            await JoinGame();
        }

        private async Task JoinGame() {
            try {
                Log.Info($"{nameof(JoinGame)}");

                BorderButtons.IsEnabled = false;

                var hostedGame = await VerifyCanJoinGame();

                var spectate = hostedGame.Status == HostedGameStatus.GameInProgress && hostedGame.Spectator;
                Program.IsHost = false;
                var password = "";
                if (hostedGame.HasPassword) {
                    var dlg = new InputDlg("Password", "Please enter this games password", "");
                    password = dlg.GetString();
                }
                var username = (Program.LobbyClient.IsConnected == false
                    || Program.LobbyClient.User == null
                    || Program.LobbyClient.User.DisplayName == null) ? Prefs.Nickname : Program.LobbyClient.User.DisplayName;

                var game = GameManager.Get().GetById(hostedGame.GameId);

                Program.GameEngine = new GameEngine(game, username, spectate, password);
                Program.CurrentOnlineGameName = hostedGame.Name;

                Log.InfoFormat("Creating client for {0}:{1}", hostedGame.IPAddress, hostedGame.Port);
                Program.Client = new ClientSocket(hostedGame.IPAddress, hostedGame.Port);
                await Program.Client.Connect();

                Log.Info($"{nameof(JoinGame)}: Launching {nameof(PlayWindow)}");
                LaunchPlayWindow();
            } catch (Exception ex) {
                HandleException(ex);
            } finally {
                BorderButtons.IsEnabled = true;
           }
        }

        private async Task<HostedGameViewModel> VerifyCanJoinGame() {
            if (WindowManager.PlayWindow != null) {
                throw new UserMessageException("You are currently in a game or game lobby. Please leave before you join game.");
            }

            var hostedGame = ListViewGameList.SelectedItem as HostedGameViewModel;

            if (hostedGame.Status == HostedGameStatus.GameInProgress && hostedGame.Spectator == false) {
                throw new UserMessageException("You can't join a game that is already in progress.");
            }

            if (hostedGame.GameSource == "Online") {
                var client = new ApiClient();
                try {
                    if (!await client.IsGameServerRunning(Prefs.Username, Prefs.Password.Decrypt())) {
                        throw new UserMessageException("The game server is currently down. Please try again later.");
                    }
                } catch (Exception ex) {
                    throw new UserMessageException("The game server is currently down. Please try again later.", ex);
                }
            }

            var game = GameManager.Get().GetById(hostedGame.GameId);
            if (game == null) {
                throw new UserMessageException("You don't have the required game installed.");
            }

            var hostAddress = hostedGame.IPAddress;
            if (hostAddress == null) {
                throw new UserMessageException("There was a problem with your DNS. Please try again.");
            }

            return hostedGame;
        }

        #endregion Join Game

        #region Join Offline Game

        private void ButtonJoinOfflineGame(object sender, RoutedEventArgs e) {
            try {
                if (WindowManager.PlayWindow != null) {
                    throw new UserMessageException("You are currently in a game or game lobby. Please leave before you join game.");
                }

                ConnectOfflineGame dialog = null;
                try {
                    dialog = new ConnectOfflineGame();
                    dialog.Show(DialogPlaceHolder);
                    dialog.OnClose += ConnectOfflineGameDialogOnClose;
                    BorderButtons.IsEnabled = false;
                } catch {
                    dialog.OnClose -= ConnectOfflineGameDialogOnClose;
                    dialog.Dispose();
                    BorderButtons.IsEnabled = true;
                    throw;
                }
            } catch (Exception ex) {
                HandleException(ex);
            }
        }

        private void ConnectOfflineGameDialogOnClose(object sender, DialogResult dialogResult) {
            BorderButtons.IsEnabled = true;
            using (var dialog = sender as ConnectOfflineGame) {
                dialog.OnClose -= ConnectOfflineGameDialogOnClose;

                if (dialogResult == DialogResult.OK) {
                    if (dialog.Successful) {
                        LaunchPlayWindow();
                    }
                }

                Program.GameEngine?.End();
            }
        }

        #endregion Join Offline Game

        private void LaunchPlayWindow() {
            Dispatcher.VerifyAccess();

            if (WindowManager.PlayWindow != null) throw new InvalidOperationException($"Can't run more than one game at a time.");

            Dispatcher.InvokeAsync(async () => {
                await Dispatcher.Yield(DispatcherPriority.Background);
                WindowManager.PlayWindow = new PlayWindow();
                WindowManager.PlayWindow.Show();
                WindowManager.PlayWindow.Activate();
            });
        }

        private void ButtonKillGame(object sender, RoutedEventArgs e) {
            var hostedgame = ListViewGameList.SelectedItem as HostedGameViewModel;
            if (hostedgame == null) return;
            if (Program.LobbyClient != null && Program.LobbyClient.User != null && Program.LobbyClient.IsConnected) {
                throw new NotImplementedException("sorry bro");
            }
        }

        public void Dispose() {
            broadcastListener.StopListening();
            broadcastListener.Dispose();
            Program.LobbyClient.Disconnected -= LobbyClient_OnDisconnect;
            _refreshGameListTimer.IsEnabled = false;
        }

        private void HandleException(Exception ex, [CallerMemberName]string caller = null) {
            Log.Error($"{nameof(HandleException)}: {caller}", ex);

            var error = "Unknown Error: Please try again";
            if(ex is UserMessageException um) {
                error = um.Message;
            }

            MessageBox.Show(error, "OCTGN", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool NotifyAndUpdate<T>(ref T privateField, T value, [CallerMemberName]string propertyName = null) {
            if (object.Equals(privateField, value)) return false;
            privateField = value;

            OnPropertyChanged(propertyName);
            return true;
        }

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null) {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException(nameof(propertyName));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
