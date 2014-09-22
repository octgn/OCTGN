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
using Octgn.Site.Api.Models;
using Skylabs.Lobby;
using System.Collections.ObjectModel;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

using Microsoft.Scripting.Utils;

using Octgn.Core;
using Octgn.Core.DataManagers;
using Octgn.Library;
using Octgn.Library.Exceptions;
using Octgn.Networking;
using Octgn.Play;
using Octgn.Scripting.Controls;
using Octgn.ViewModels;
using MessageBox = System.Windows.Forms.MessageBox;
using Timer = System.Threading.Timer;

namespace Octgn.Controls
{

    using log4net;

    /// <summary>
    /// Interaction logic for CustomGames.xaml
    /// </summary>
    public partial class CustomGameList : INotifyPropertyChanged, IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static DependencyProperty IsJoinableGameSelectedProperty = DependencyProperty.Register(
            "IsJoinableGameSelected", typeof(bool), typeof(CustomGameList));

        public ObservableCollection<HostedGameViewModel> HostedGameList { get; set; }

        public bool IsJoinableGameSelected
        {
            get
            {
                return (bool)this.GetValue(IsJoinableGameSelectedProperty);
            }
            private set
            {
                SetValue(IsJoinableGameSelectedProperty, value);
            }
        }

        public bool Spectate
        {
            get { return _spectate; }
            set
            {
                if (value.Equals(_spectate)) return;
                _spectate = value;
                OnPropertyChanged("Spectate");
                Prefs.SpectateGames = _spectate;
                foreach (var g in HostedGameList.ToArray())
                {
                    g.UpdateVisibility();
                }
            }
        }

        public bool IsRefreshingGameList
        {
            get { return _isRefreshingGameList; }
            set
            {
                if (value == _isRefreshingGameList) return;
                _isRefreshingGameList = value;
                OnPropertyChanged("IsRefreshingGameList");
            }
        }

        public bool ShowUninstalledGames
        {
            get { return _showUninstalledGames; }
            set
            {
                if (value.Equals(_showUninstalledGames)) return;
                _showUninstalledGames = value;
                OnPropertyChanged("ShowUninstalledGames");
                Prefs.HideUninstalledGamesInList = _showUninstalledGames == false;
                foreach (var g in HostedGameList.ToArray())
                {
                    g.UpdateVisibility();
                }
            }
        }

        public bool ShowKillGameButton
        {
            get { return _showKillGameButton; }
            set
            {
                if (value.Equals(_showKillGameButton)) return;
                _showKillGameButton = value;
                OnPropertyChanged("ShowKillGameButton");
            }
        }

        public int CountdownUntilRefresh
        {
            get { return _countdownUntilRefresh; }
            set
            {
                if (value == _countdownUntilRefresh) return;
                _countdownUntilRefresh = value;
                OnPropertyChanged("CountdownUntilRefresh");
            }
        }

        private int _countdownUntilRefresh;

        private HostGameSettings hostGameDialog;
        private ConnectOfflineGame connectOfflineGameDialog;

        private readonly GameBroadcastListener broadcastListener;

        private readonly DragDeltaEventHandler dragHandler;
        private bool _spectate;
        private bool _showUninstalledGames;
        private bool _showKillGameButton;
        private ChatRoom _room;
        private bool _isRefreshingGameList;
        private readonly object _gameListLocker = new object();
        private readonly Timer _refreshGameListTimer;
        private bool _visible;

        public CustomGameList()
        {
            InitializeComponent();
            broadcastListener = new GameBroadcastListener();
            broadcastListener.StartListening();
            dragHandler = this.ListViewGameList_OnDragDelta;
            ListViewGameList.AddHandler(Thumb.DragDeltaEvent, dragHandler, true);
            HostedGameList = new ObservableCollection<HostedGameViewModel>();
            Program.LobbyClient.OnLoginComplete += LobbyClient_OnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClient_OnDisconnect;
            //Program.LobbyClient.OnDataReceived += LobbyClient_OnDataReceived;
            Program.LobbyClient.Chatting.OnCreateRoom += ChattingOnOnCreateRoom;

            Spectate = Prefs.SpectateGames;
            ShowKillGameButton = Prefs.IsAdmin;
            ShowUninstalledGames = Prefs.HideUninstalledGamesInList == false;

            _refreshGameListTimer = new Timer(RefreshGamesTask,null,5000,15000);
        }

        private void ChattingOnOnCreateRoom(object sender, ChatRoom room)
        {
            if (room.GroupUser == null || room.GroupUser.UserName != "lobby")
            {
                return;
            }

            _room = room;
        }

        void RefreshGameList(List<GameDetails> games)
        {
            lock (_gameListLocker)
            {
                //Log.Info("Refreshing list...");
                var list = games.Select(x => new HostedGameViewModel(x)).ToList();
                list.AddRange(broadcastListener.Games.Select(x => new HostedGameViewModel(x)));

                //Log.Info("Got hosted games list");
                ShowKillGameButton = Prefs.IsAdmin;

                Dispatcher.Invoke(
                    new Action(
                        () =>
                        {
                            //Log.Info("Refreshing visual list");

                            var removeList = HostedGameList.Where(i => list.All(x => x.Id != i.Id)).ToList();
                            removeList.ForEach(x => HostedGameList.Remove(x));
                            var addList = list.Where(i => this.HostedGameList.All(x => x.Id != i.Id)).ToList();
                            HostedGameList.AddRange(addList);
                            var dbgames = GameManager.Get().Games.ToArray();
                            foreach (var g in HostedGameList)
                            {
                                var li = list.FirstOrDefault(x => x.Id == g.Id);
                                g.Update(li, dbgames);
                            }
                            //Log.Info("Visual list refreshed");

                        }));
            }
        }

        private void RefreshGamesTask(object state)
        {
            _refreshGameListTimer.Change(Timeout.Infinite, Timeout.Infinite);
            var list = new List<GameDetails>();
            try
            {
                IsRefreshingGameList = true;
                CountdownUntilRefresh = 0;
                if (_visible == false)
                    return;
                if (Program.LobbyClient == null || Program.LobbyClient.IsConnected == false)
                    return;
                list = new ApiClient().GetGameList();
            }
            catch (Exception e)
            {
                Log.Warn("RefreshGamesTask Error", e);
            }
            finally
            {
                RefreshGameList(list);
                IsRefreshingGameList = false;
                CountdownUntilRefresh = 0;
                if (_visible)
                {
                    for (CountdownUntilRefresh = 1500; CountdownUntilRefresh > 0; CountdownUntilRefresh--)
                    {
                        if (IsRefreshingGameList)
                            break;
                        Thread.Sleep(10);
                    }
                    _refreshGameListTimer.Change(100, Timeout.Infinite);
                }
                else
                    _refreshGameListTimer.Change(5000, Timeout.Infinite);
            }
        }

        public void VisibleChanged(bool visible)
        {
            _visible = visible;
        }

        private void ShowHostGameDialog()
        {
            hostGameDialog = new HostGameSettings();
            hostGameDialog.Show(DialogPlaceHolder);
            hostGameDialog.OnClose += HostGameSettingsDialogOnClose;
            BorderButtons.IsEnabled = false;
        }

        private void ShowJoinOfflineGameDialog()
        {
            connectOfflineGameDialog = new ConnectOfflineGame();
            connectOfflineGameDialog.Show(DialogPlaceHolder);
            connectOfflineGameDialog.OnClose += ConnectOfflineGameDialogOnClose;
            BorderButtons.IsEnabled = false;
        }

        private void StartJoinGame(HostedGameViewModel hostedGame, DataNew.Entities.Game game, bool spectate)
        {
            if (hostedGame.GameSource == "Online")
            {
                var client = new Octgn.Site.Api.ApiClient();
                if (!client.IsGameServerRunning(Program.LobbyClient.Username, Program.LobbyClient.Password))
                {
                    throw new UserMessageException("The game server is currently down. Please try again later.");
                }
            }
            Log.InfoFormat("Starting to join a game {0} {1}", hostedGame.GameId, hostedGame.Name);
            Program.IsHost = false;
            Program.IsMatchmaking = false;
            var password = "";
            if (hostedGame.HasPassword)
            {
                Dispatcher.Invoke(new Action(() =>
                    {
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
            if (hostAddress == null)
            {
                Log.WarnFormat("Dns Error, couldn't resolve {0}", AppConfig.GameServerPath);
                throw new UserMessageException("There was a problem with your DNS. Please try again.");
            }

            try
            {
                Log.InfoFormat("Creating client for {0}:{1}", hostAddress, hostedGame.Port);
                Program.Client = new ClientSocket(hostAddress, hostedGame.Port);
                Log.InfoFormat("Connecting client for {0}:{1}", hostAddress, hostedGame.Port);
                Program.Client.Connect();
            }
            catch (Exception e)
            {
                Log.Warn("Start join game error ", e);
                throw new UserMessageException("Could not connect. Please try again.");
            }

        }

        private void FinishJoinGame(Task task)
        {
            Log.Info("Finished joining game task");
            BorderButtons.IsEnabled = true;
            if (task.IsFaulted)
            {
                Log.Warn("Couldn't join game");
                var error = "Unknown Error: Please try again";
                if (task.Exception != null)
                {
                    Log.Warn("Finish join game exception", task.Exception);
                    var umException = task.Exception.InnerExceptions.OfType<UserMessageException>().FirstOrDefault();
                    if (umException != null)
                    {
                        error = umException.Message;
                    }
                }
                MessageBox.Show(error, "OCTGN", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Log.Info("Starting to join game");
            //WindowManager.PreGameLobbyWindow = new PreGameLobbyWindow();
            //WindowManager.PreGameLobbyWindow.Setup(false, WindowManager.Main);
            WindowManager.PlayWindow = new PlayWindow();
            WindowManager.PlayWindow.Show();
        }

        #region LobbyEvents
        void LobbyClient_OnDisconnect(object sender, EventArgs e)
        {
            lock (_gameListLocker)
            {
                Log.Info("Disconnected");
                _room = null;
                Dispatcher.Invoke(new Action(() => this.HostedGameList.Clear()));
            }
        }

        void LobbyClient_OnLoginComplete(object sender, LoginResults results)
        {
            lock (_gameListLocker)
            {
                Log.Info("Connected");
            }
        }

        //void LobbyClient_OnDataReceived(object sender, DataRecType type, object data)
        //{
        //    if (type == DataRecType.GameList || type == DataRecType.GamesNeedRefresh)
        //    {
        //        RefreshGameList();
        //        IsRefreshingGameList = false;
        //    }
        //}
        #endregion

        #region UI Events

        private void GameListItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Program.IsInMatchmakingQueue)
            {
                MessageBox.Show(
                    "You are currently matchmaking. Please leave before you join game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            if (WindowManager.PlayWindow != null)
            {
                MessageBox.Show(
                    "You are currently in a game or game lobby. Please leave before you join game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            var hostedgame = ListViewGameList.SelectedItem as HostedGameViewModel;
            if (hostedgame == null) return;
            if (hostedgame.Status == EHostedGame.GameInProgress && hostedgame.Spectator == false)
            {
                TopMostMessageBox.Show(
                        "You can't join a game in progress.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                return;
            }
            if (hostedgame.GameSource == "Online")
            {
                var client = new Octgn.Site.Api.ApiClient();
                if (!client.IsGameServerRunning(Program.LobbyClient.Username, Program.LobbyClient.Password))
                {
                    TopMostMessageBox.Show(
                        "The game server is currently down. Please try again later.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
            }
            var game = GameManager.Get().GetById(hostedgame.GameId);
            if (game == null)
            {
                TopMostMessageBox.Show("You don't currently have that game installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            bool spectate = hostedgame.Status == EHostedGame.GameInProgress && hostedgame.Spectator;
            var task = new Task(() => this.StartJoinGame(hostedgame, game, spectate));
            task.ContinueWith((t) => { this.Dispatcher.Invoke(new Action(() => this.FinishJoinGame(t))); });
            BorderButtons.IsEnabled = false;
            task.Start();
        }

        private void HostGameSettingsDialogOnClose(object o, DialogResult dialogResult)
        {
            BorderButtons.IsEnabled = true;
            if (dialogResult == DialogResult.OK)
            {
                if (hostGameDialog.SuccessfulHost)
                {
                    if (WindowManager.PlayWindow == null)
                    {
                        //WindowManager.PreGameLobbyWindow = new PreGameLobbyWindow();
                        //WindowManager.PreGameLobbyWindow.Setup(hostGameDialog.IsLocalGame, WindowManager.Main);
                        WindowManager.PlayWindow = new PlayWindow();
                        WindowManager.PlayWindow.Show();
                    }
                }
            }
            hostGameDialog.Dispose();
            hostGameDialog = null;
        }

        private void ConnectOfflineGameDialogOnClose(object o, DialogResult dialogResult)
        {
            BorderButtons.IsEnabled = true;
            try
            {
                if (dialogResult == DialogResult.OK)
                {
                    if (connectOfflineGameDialog.Successful)
                    {
                        if (WindowManager.PlayWindow == null)
                        {
                            //WindowManager.PreGameLobbyWindow = new PreGameLobbyWindow();
                            //WindowManager.PreGameLobbyWindow.Setup(true, WindowManager.Main);
                            WindowManager.PlayWindow = new PlayWindow();
                            WindowManager.PlayWindow.Show();
                            return;
                        }
                    }
                }

                try
                {
                    Program.GameEngine.End();
                }
                catch { }

                Program.GameEngine = null;

            }
            finally
            {
                connectOfflineGameDialog.Dispose();
                connectOfflineGameDialog = null;
            }
        }

        private void ListViewGameListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Log.Info("Changed custom game selection");
            if (ListViewGameList == null) return;
            var game = ListViewGameList.SelectedItem as HostedGameViewModel;
            if (game == null) return;
            Log.InfoFormat("Selected game {0} {1}", game.GameId, game.Name);
            this.IsJoinableGameSelected = game.CanPlay;
        }

        private void ButtonHostClick(object sender, RoutedEventArgs e)
        {
            if (Program.IsInMatchmakingQueue)
            {
                MessageBox.Show(
                    "You are currently matchmaking. Please leave before you host a new game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            if (WindowManager.PlayWindow != null)
            {
                MessageBox.Show(
                    "You are currently in a game or game lobby. Please leave before you host a new game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            this.ShowHostGameDialog();
        }

        private void ButtonJoinClick(object sender, RoutedEventArgs e)
        {
            if (Program.IsInMatchmakingQueue)
            {
                MessageBox.Show(
                    "You are currently matchmaking. Please leave before you join game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            if (WindowManager.PlayWindow != null)
            {
                MessageBox.Show(
                    "You are currently in a game or game lobby. Please leave before you join game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            var hostedgame = ListViewGameList.SelectedItem as HostedGameViewModel;
            if (hostedgame == null) return;
            if (hostedgame.Status == EHostedGame.GameInProgress && hostedgame.Spectator == false)
            {
                TopMostMessageBox.Show(
                        "You can't join a game in progress.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                return;
            }
            if (hostedgame.GameSource == "Online")
            {
                var client = new Octgn.Site.Api.ApiClient();
                if (!client.IsGameServerRunning(Program.LobbyClient.Username, Program.LobbyClient.Password))
                {
                    TopMostMessageBox.Show(
                        "The game server is currently down. Please try again later.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
            }
            var game = GameManager.Get().GetById(hostedgame.GameId);
            if (game == null)
            {
                TopMostMessageBox.Show("You don't currently have that game installed.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var spectate = hostedgame.Status == EHostedGame.GameInProgress && hostedgame.Spectator == true;
            var task = new Task(() => this.StartJoinGame(hostedgame, game, spectate));
            task.ContinueWith((t) => { this.Dispatcher.Invoke(new Action(() => this.FinishJoinGame(t))); });
            BorderButtons.IsEnabled = false;
            task.Start();
        }

        private void ButtonJoinOfflineGame(object sender, RoutedEventArgs e)
        {
            if (Program.IsInMatchmakingQueue)
            {
                MessageBox.Show(
                    "You are currently matchmaking. Please leave before you join game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            if (WindowManager.PlayWindow != null)
            {
                MessageBox.Show(
                    "You are currently in a game or game lobby. Please leave before you join game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            ShowJoinOfflineGameDialog();
        }

        private void ButtonKillGame(object sender, RoutedEventArgs e)
        {
            var hostedgame = ListViewGameList.SelectedItem as HostedGameViewModel;
            if (hostedgame == null) return;
            if (Program.LobbyClient != null && Program.LobbyClient.Me != null && Program.LobbyClient.IsConnected)
            {
                Program.LobbyClient.KillGame(hostedgame.Id);
            }
        }
        #endregion

        private void ListViewGameList_OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (e == null || e.OriginalSource == null) return;
            var senderAsThumb = e.OriginalSource as Thumb;
            if (senderAsThumb == null || senderAsThumb.TemplatedParent == null) return;
            var header = senderAsThumb.TemplatedParent as GridViewColumnHeader;
            if (header == null) return;
            if (header.Column.ActualWidth < 20)
                header.Column.Width = 20;
        }

        public void Dispose()
        {
            broadcastListener.StopListening();
            broadcastListener.Dispose();
            ListViewGameList.RemoveHandler(Thumb.DragDeltaEvent, dragHandler);
            Program.LobbyClient.OnLoginComplete -= LobbyClient_OnLoginComplete;
            Program.LobbyClient.OnDisconnect -= LobbyClient_OnDisconnect;
            //Program.LobbyClient.OnDataReceived -= LobbyClient_OnDataReceived;
            Program.LobbyClient.Chatting.OnCreateRoom -= ChattingOnOnCreateRoom;
            _refreshGameListTimer.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
