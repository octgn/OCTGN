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
using System.Net;
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

        public static DependencyProperty IsJoinableGameSelectedProperty = DependencyProperty.Register(
            nameof(IsJoinableGameSelected), typeof(bool), typeof(PlayTab));

        public ObservableCollection<HostedGameViewModel> HostedGameList { get; set; }

        public bool IsJoinableGameSelected {
            get => (bool)this.GetValue(IsJoinableGameSelectedProperty);
            private set => SetValue(IsJoinableGameSelectedProperty, value);
        }

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

        public bool IsRefreshingGameList {
            get => _isRefreshingGameList;
            set => NotifyAndUpdate(ref _isRefreshingGameList, value);
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

        private HostGameSettings hostGameDialog;
        private ConnectOfflineGame connectOfflineGameDialog;

        private readonly GameBroadcastListener broadcastListener;

        private bool _spectate;
        private bool _showUninstalledGames;
        private bool _showKillGameButton;
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

        private async void RefreshGameListTimer_Tick(object sender, EventArgs e) {
            ShowKillGameButton = Prefs.IsAdmin;

            if (Program.LobbyClient == null || !Program.LobbyClient.IsConnected)
                return;

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

        private void ShowHostGameDialog() {
            hostGameDialog = new HostGameSettings();
            hostGameDialog.Show(DialogPlaceHolder);
            hostGameDialog.OnClose += HostGameSettingsDialogOnClose;
            BorderButtons.IsEnabled = false;
        }

        private void ShowJoinOfflineGameDialog() {
            connectOfflineGameDialog = new ConnectOfflineGame();
            connectOfflineGameDialog.Show(DialogPlaceHolder);
            connectOfflineGameDialog.OnClose += ConnectOfflineGameDialogOnClose;
            BorderButtons.IsEnabled = false;
        }

        private void StartJoinGame(HostedGameViewModel hostedGame, DataNew.Entities.Game game, bool spectate) {
            if (hostedGame.GameSource == "Online") {
                var client = new Octgn.Site.Api.ApiClient();
                if (!client.IsGameServerRunning(Prefs.Username, Prefs.Password.Decrypt())) {
                    throw new UserMessageException("The game server is currently down. Please try again later.");
                }
            }
            Log.InfoFormat("Starting to join a game {0} {1}", hostedGame.GameId, hostedGame.Name);
            Program.IsHost = false;
            var password = "";
            if (hostedGame.HasPassword) {
                Dispatcher.Invoke(new Action(() => {
                    var dlg = new InputDlg("Password", "Please enter this games password", "");
                    password = dlg.GetString();
                }));
            }
            var username = (Program.LobbyClient.IsConnected == false
                || Program.LobbyClient.Me == null
                || Program.LobbyClient.Me.UserName == null) ? Prefs.Nickname : Program.LobbyClient.Me.UserName;
            Program.GameEngine = new GameEngine(game, username, spectate, password);
            Program.CurrentOnlineGameName = hostedGame.Name;
            IPAddress hostAddress = hostedGame.IPAddress;
            if (hostAddress == null) {
                Log.WarnFormat("Dns Error, couldn't resolve {0}", AppConfig.GameServerPath);
                throw new UserMessageException("There was a problem with your DNS. Please try again.");
            }

            try {
                Log.InfoFormat("Creating client for {0}:{1}", hostAddress, hostedGame.Port);
                Program.Client = new ClientSocket(hostAddress, hostedGame.Port);
                Log.InfoFormat("Connecting client for {0}:{1}", hostAddress, hostedGame.Port);
                Program.Client.Connect();
            } catch (Exception e) {
                Log.Warn("Start join game error ", e);
                throw new UserMessageException("Could not connect. Please try again.");
            }

        }

        private void FinishJoinGame(Task task) {
            Log.Info("Finished joining game task");
            BorderButtons.IsEnabled = true;
            if (task.IsFaulted) {
                Log.Warn("Couldn't join game");
                var error = "Unknown Error: Please try again";
                if (task.Exception != null) {
                    Log.Warn("Finish join game exception", task.Exception);
                    var umException = task.Exception.InnerExceptions.OfType<UserMessageException>().FirstOrDefault();
                    if (umException != null) {
                        error = umException.Message;
                    }
                }
                MessageBox.Show(error, "OCTGN", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Log.Info("Starting to join game");
            WindowManager.PlayWindow = new PlayWindow();
            WindowManager.PlayWindow.Show();
        }

        void LobbyClient_OnDisconnect(object sender, DisconnectedEventArgs e) {
            Log.Info("Disconnected");
            Dispatcher.Invoke(new Action(() => this.HostedGameList.Clear()));
        }

        private void GameListItemDoubleClick(object sender, MouseButtonEventArgs e) {
            if (WindowManager.PlayWindow != null) {
                MessageBox.Show(
                    "You are currently in a game or game lobby. Please leave before you join game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            var hostedgame = ListViewGameList.SelectedItem as HostedGameViewModel;
            if (hostedgame == null) return;
            if (hostedgame.Status == HostedGameStatus.GameInProgress && hostedgame.Spectator == false) {
                TopMostMessageBox.Show(
                        "You can't join a game in progress.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                return;
            }
            if (hostedgame.GameSource == "Online") {
                var client = new Octgn.Site.Api.ApiClient();
                if (!client.IsGameServerRunning(Prefs.Username, Prefs.Password.Decrypt())) {
                    TopMostMessageBox.Show(
                        "The game server is currently down. Please try again later.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
            }
            var game = GameManager.Get().GetById(hostedgame.GameId);
            if (game == null) {
                TopMostMessageBox.Show("You don't currently have that game installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            bool spectate = hostedgame.Status == HostedGameStatus.GameInProgress && hostedgame.Spectator;
            var task = new Task(() => this.StartJoinGame(hostedgame, game, spectate));
            task.ContinueWith((t) => { this.Dispatcher.Invoke(new Action(() => this.FinishJoinGame(t))); });
            BorderButtons.IsEnabled = false;
            task.Start();
        }

        private void HostGameSettingsDialogOnClose(object o, DialogResult dialogResult) {
            BorderButtons.IsEnabled = true;
            if (dialogResult == DialogResult.OK) {
                if (hostGameDialog.SuccessfulHost) {
                    if (WindowManager.PlayWindow == null) {
                        WindowManager.PlayWindow = new PlayWindow();
                        WindowManager.PlayWindow.Show();
                    }
                }
            }
            hostGameDialog.Dispose();
            hostGameDialog = null;
        }

        private void ConnectOfflineGameDialogOnClose(object o, DialogResult dialogResult) {
            BorderButtons.IsEnabled = true;
            try {
                if (dialogResult == DialogResult.OK) {
                    if (connectOfflineGameDialog.Successful) {
                        if (WindowManager.PlayWindow == null) {
                            WindowManager.PlayWindow = new PlayWindow();
                            WindowManager.PlayWindow.Show();
                            return;
                        }
                    }
                }

                try {
                    Program.GameEngine.End();
                } catch { }

                Program.GameEngine = null;

            } finally {
                connectOfflineGameDialog.Dispose();
                connectOfflineGameDialog = null;
            }
        }

        private void ListViewGameListSelectionChanged(object sender, SelectionChangedEventArgs e) {
            Log.Info("Changed custom game selection");
            if (ListViewGameList == null) return;
            var game = ListViewGameList.SelectedItem as HostedGameViewModel;
            if (game == null) return;
            Log.InfoFormat("Selected game {0} {1}", game.GameId, game.Name);
            this.IsJoinableGameSelected = game.CanPlay;
        }

        private void ButtonHostClick(object sender, RoutedEventArgs e) {
            if (WindowManager.PlayWindow != null) {
                MessageBox.Show(
                    "You are currently in a game or game lobby. Please leave before you host a new game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            this.ShowHostGameDialog();
        }

        private void ButtonJoinClick(object sender, RoutedEventArgs e) {
            if (WindowManager.PlayWindow != null) {
                MessageBox.Show(
                    "You are currently in a game or game lobby. Please leave before you join game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            var hostedgame = ListViewGameList.SelectedItem as HostedGameViewModel;
            if (hostedgame == null) return;
            if (hostedgame.Status == HostedGameStatus.GameInProgress && hostedgame.Spectator == false) {
                TopMostMessageBox.Show(
                        "You can't join a game in progress.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                return;
            }
            if (hostedgame.GameSource == "Online") {
                var client = new Octgn.Site.Api.ApiClient();
                if (!client.IsGameServerRunning(Prefs.Username, Prefs.Password.Decrypt())) {
                    TopMostMessageBox.Show(
                        "The game server is currently down. Please try again later.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
            }
            var game = GameManager.Get().GetById(hostedgame.GameId);
            if (game == null) {
                TopMostMessageBox.Show("You don't currently have that game installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var spectate = hostedgame.Status == HostedGameStatus.GameInProgress && hostedgame.Spectator == true;
            var task = new Task(() => this.StartJoinGame(hostedgame, game, spectate));
            task.ContinueWith((t) => { this.Dispatcher.Invoke(new Action(() => this.FinishJoinGame(t))); });
            BorderButtons.IsEnabled = false;
            task.Start();
        }

        private void ButtonJoinOfflineGame(object sender, RoutedEventArgs e) {
            if (WindowManager.PlayWindow != null) {
                MessageBox.Show(
                    "You are currently in a game or game lobby. Please leave before you join game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            ShowJoinOfflineGameDialog();
        }

        private void ButtonKillGame(object sender, RoutedEventArgs e) {
            var hostedgame = ListViewGameList.SelectedItem as HostedGameViewModel;
            if (hostedgame == null) return;
            if (Program.LobbyClient != null && Program.LobbyClient.Me != null && Program.LobbyClient.IsConnected) {
                throw new NotImplementedException("sorry bro");
            }
        }

        public void Dispose() {
            broadcastListener.StopListening();
            broadcastListener.Dispose();
            Program.LobbyClient.Disconnected -= LobbyClient_OnDisconnect;
            _refreshGameListTimer.IsEnabled = false;
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
