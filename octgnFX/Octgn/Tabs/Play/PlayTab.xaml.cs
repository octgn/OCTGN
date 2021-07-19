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
using MessageBox = System.Windows.Forms.MessageBox;
using Octgn.Extentions;
using Octgn.Communication;
using Octgn.Online.Hosting;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Octgn.Controls;
using Octgn.Library;

namespace Octgn.Tabs.Play
{
    public partial class PlayTab : INotifyPropertyChanged, IDisposable
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public ObservableCollection<HostedGameViewModel> HostedGameList { get; set; }

        private bool _loadingGame;
        public bool LoadingGame
        {
            get => _loadingGame;
            set => NotifyAndUpdate(ref _loadingGame, value);
        }

        private HostedGameViewModel _selectedGame;
        public HostedGameViewModel SelectedGame
        {
            get { return _selectedGame; }

            set {
                this.IsJoinableGameSelected = value?.CanPlay == true;
                if (value != null)
                    Log.InfoFormat("Selected game {0} {1}", value.GameId, value.Name);
                NotifyAndUpdate(ref _selectedGame, value);
    }
        }

        public bool HideUninstalledGames  {
            get => Prefs.HideUninstalledGamesInList;
            set
            {
                Prefs.HideUninstalledGamesInList = value;
                OnPropertyChanged(nameof(HideUninstalledGames));
            }
        }
        private bool _showKillGameButton;
        public bool ShowKillGameButton {
            get => _showKillGameButton;
            set => NotifyAndUpdate(ref _showKillGameButton, value);
        }

        private int _gameCount;
        public int GameCount {
            get => _gameCount;
            set => NotifyAndUpdate(ref _gameCount, value);
        }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => NotifyAndUpdate(ref _isLoggedIn, value);
        }

        private readonly GameBroadcastListener broadcastListener;

        public PlayTab() {
            InitializeComponent();
            broadcastListener = new GameBroadcastListener();
            broadcastListener.StartListening();
            HostedGameList = new ObservableCollection<HostedGameViewModel>(); 
            Program.LobbyClient.Connected += LobbyClient_OnConnect;
            Program.LobbyClient.Disconnected += LobbyClient_OnDisconnect;
            if (Program.Discord != null) {
            Program.Discord.JoinGame += Discord_JoinGame;
            }
            LoadingGame = true;
            ShowKillGameButton = Prefs.IsAdmin;
            _refreshGameListTimer = new DispatcherTimer(InitialRefreshDelay.TimeSpan, DispatcherPriority.Normal, RefreshGameListTimer_Tick, Dispatcher);
        }

        #region Game List Refreshing

        private readonly DispatcherTimer _refreshGameListTimer;

        private bool _isRefreshingGameList;
        public bool IsRefreshingGameList {
            get => _isRefreshingGameList;
            set => NotifyAndUpdate(ref _isRefreshingGameList, value);
        }

        public static Duration InitialRefreshDelay { get; } = new Duration(TimeSpan.FromSeconds(2));
        public static Duration NormalRefreshDelay { get; } = new Duration(TimeSpan.FromSeconds(15));

        private Duration _currentRefreshDelay = new Duration(TimeSpan.FromDays(10));
        public Duration CurrentRefreshDelay {
            get => _currentRefreshDelay;
            set {
                if (!NotifyAndUpdate(ref _currentRefreshDelay, value)) return;
                OnPropertyChanged(nameof(IsInitialRefresh));
                _refreshGameListTimer.Interval = value.TimeSpan;
            }
        }

        public bool IsInitialRefresh => CurrentRefreshDelay.TimeSpan == InitialRefreshDelay.TimeSpan;

        private async void Discord_JoinGame(object sender, Guid guid)
        {
            var game = HostedGameList.FirstOrDefault(x => x.Id == guid);
            if (game == null) return;
            await JoinGame(game);
        }

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
                    IsLoggedIn = true;
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

                GameCount = HostedGameList.Count;
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

        void LobbyClient_OnConnect(object sender, ConnectedEventArgs e) {
            Log.Info("Connected");
            Dispatcher.InvokeAsync(new Action(() => {
                this.HostedGameList.Clear();
                this.IsLoggedIn = true;
                this.CurrentRefreshDelay = InitialRefreshDelay;
            }));
        }

        void LobbyClient_OnDisconnect(object sender, DisconnectedEventArgs e) {
            Log.Info("Disconnected"); 
            Dispatcher.InvokeAsync(new Action(() => {
                this.HostedGameList.Clear();
                this.IsLoggedIn = false;
            }));
        }

        #endregion Game List Refreshing

        #region Host Game

        private void ButtonHostClick(object sender, RoutedEventArgs e) {
            try {
                HostGameSettings dialog = null;
                try {
                    dialog = new HostGameSettings();
                    dialog.Show(DialogPlaceHolder);
                    dialog.OnClose += HostGameSettingsDialogOnClose;
                    LoadingGame = false;
                } catch {
                    dialog.OnClose -= HostGameSettingsDialogOnClose;
                    dialog.Dispose();
                    LoadingGame = true;
                    throw;
                }
            } catch (Exception ex) {
                HandleException(ex);
            }
        }

        private void HostGameSettingsDialogOnClose(object sender, DialogResult dialogResult) {
            LoadingGame = true;
            using (var dialog = sender as HostGameSettings) {
                dialog.OnClose -= HostGameSettingsDialogOnClose;
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

        private async void GameListItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            await JoinGame(SelectedGame);
        }

        private async void ButtonJoinClick(object sender, RoutedEventArgs e)
        {
            await JoinGame(SelectedGame);
        }

        private async Task JoinGame(HostedGameViewModel game) {
            try {
                Log.Info($"{nameof(JoinGame)}");

                LoadingGame = false;

                var hostedGame = await VerifyCanJoinGame();

                string username;
                if (hostedGame.GameSource == "Online") {
                    username = Program.LobbyClient.User.DisplayName;
                } else {
                    username = Program.LobbyClient.User?.DisplayName ?? Prefs.Username ?? Randomness.RandomRoomName();
                }

                var spectate
                    = hostedGame.Status == HostedGameStatus.GameInProgress
                    && hostedGame.Spectator;

                await Program.JodsEngine.JoinGame(hostedGame, username, spectate);
            } catch (Exception ex) {
                HandleException(ex);
            } finally {
                LoadingGame = true;
           }
        }

        private async Task<HostedGameViewModel> VerifyCanJoinGame() {
            if (SelectedGame == null) return null;
            if (SelectedGame.Status == HostedGameStatus.GameInProgress && SelectedGame.Spectator == false) {
                throw new UserMessageException("You can't join a game that is already in progress.");
            }

            if (SelectedGame.GameSource == "Online") {
                var client = new ApiClient();
                try {
                    if (!await client.IsGameServerRunning(Prefs.Username, Prefs.Password.Decrypt())) {
                        throw new UserMessageException("The game server is currently down. Please try again later.");
                    }
                } catch (Exception ex) {
                    throw new UserMessageException("The game server is currently down. Please try again later.", ex);
                }
            }

            var game = GameManager.Get().GetById(SelectedGame.GameId);
            if (game == null) {
                throw new UserMessageException("You don't have the required game installed.");
            }

            return SelectedGame;
        }

        #endregion Join Game

        #region Join Offline Game

        private void ButtonJoinOfflineGame(object sender, RoutedEventArgs e) {
            try {
                ConnectOfflineGame dialog = null;
                try {
                    dialog = new ConnectOfflineGame();
                    dialog.VerticalAlignment = VerticalAlignment.Center;
                    dialog.Show(DialogPlaceHolder);
                    dialog.OnClose += ConnectOfflineGameDialogOnClose;
                    LoadingGame = false;
                } catch {
                    dialog.OnClose -= ConnectOfflineGameDialogOnClose;
                    dialog.Dispose();
                    LoadingGame = true;
                    throw;
                }
            } catch (Exception ex) {
                HandleException(ex);
            }
        }

        private void ConnectOfflineGameDialogOnClose(object sender, DialogResult dialogResult) {
            LoadingGame = true;
            using (var dialog = sender as ConnectOfflineGame) {
                dialog.OnClose -= ConnectOfflineGameDialogOnClose;
            }
        }

        #endregion Join Offline Game

        private void ButtonKillGame(object sender, RoutedEventArgs e) {
            if (SelectedGame == null) return;
            if (Program.LobbyClient != null && Program.LobbyClient.User != null && Program.LobbyClient.IsConnected) {
                throw new NotImplementedException("sorry bro");
            }
        }

        public void Dispose() {
            broadcastListener.StopListening();
            broadcastListener.Dispose();
            Program.LobbyClient.Disconnected -= LobbyClient_OnDisconnect;
            if (Program.Discord != null) {
            Program.Discord.JoinGame -= Discord_JoinGame;
            }
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
