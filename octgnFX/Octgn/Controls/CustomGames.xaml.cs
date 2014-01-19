using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Octgn.Library.Networking;
using Skylabs.Lobby;

namespace Octgn.Controls
{
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
    using Octgn.Windows;

    using log4net;

    using Timer = System.Timers.Timer;

    /// <summary>
    /// Interaction logic for CustomGames.xaml
    /// </summary>
    public partial class CustomGameList:INotifyPropertyChanged,IDisposable
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

        public bool ShowRunningGames
        {
            get { return _showRunningGames; }
            set
            {
                if (value.Equals(_showRunningGames)) return;
                _showRunningGames = value;
                OnPropertyChanged("ShowRunningGames");
                Prefs.ShowRunningGames = _showRunningGames;
                foreach (var g in HostedGameList.ToArray())
                {
                    g.UpdateVisibility();
                }
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

        private readonly Timer timer;

        private readonly Timer refreshVisualListTimer;
        private bool isConnected;
        private HostGameSettings hostGameDialog;
        private ConnectOfflineGame connectOfflineGameDialog;

        private readonly GameBroadcastListener broadcastListener;

        private readonly DragDeltaEventHandler dragHandler;
        private bool _showRunningGames;
        private bool _showUninstalledGames;
        private bool _showKillGameButton;
        private ChatRoom _room;

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
            Program.LobbyClient.OnDataReceived += LobbyClient_OnDataReceived;
            Program.LobbyClient.Chatting.OnCreateRoom += ChattingOnOnCreateRoom;

            timer = new Timer(10000);
            //timer.Start();
            timer.Elapsed += this.TimerElapsed;
            refreshVisualListTimer = new Timer(10000);
            refreshVisualListTimer.Start();
            refreshVisualListTimer.Elapsed += RefreshGameList;
            ShowRunningGames = Prefs.ShowRunningGames;
            ShowUninstalledGames = Prefs.HideUninstalledGamesInList == false;
        }

        private void ChattingOnOnCreateRoom(object sender, ChatRoom room)
        {
            if (room.GroupUser == null || room.GroupUser.UserName != "lobby")
            {
                return;
            }

            _room = room;
        }

        void RefreshGameList(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            lock (timer)
            {
                //Log.Info("Refreshing list...");
                var list = Program.LobbyClient.GetHostedGames().Select(x => new HostedGameViewModel(x)).ToList();
				list.AddRange(broadcastListener.Games.Select(x => new HostedGameViewModel(x)));
				
                //Log.Info("Got hosted games list");

                this.ShowKillGameButton = false;
                if (_room != null && Program.LobbyClient != null 
                    && Program.LobbyClient.Me != null 
                    && Program.LobbyClient.Me.ApiUser != null 
                    && Program.LobbyClient.IsConnected)
                {
                    var allowed = new List<User>();
                    allowed.AddRange(_room.AdminList);
                    allowed.AddRange(_room.ModeratorList);
                    allowed.AddRange(_room.OwnerList);
                    if (allowed.Any(x => x.ApiUser != null 
                        && x.ApiUser.Id == Program.LobbyClient.Me.ApiUser.Id))
                    {
                        this.ShowKillGameButton = true;
                    }
                }

                Dispatcher.Invoke(
                    new Action(
                        () =>
                            {
                                //Log.Info("Refreshing visual list");

                                var removeList = HostedGameList.Where(i => list.All(x => x.Id != i.Id)).ToList();
                                removeList.ForEach(x => HostedGameList.Remove(x));
                                var addList = list.Where(i => this.HostedGameList.All(x => x.Id != i.Id)).ToList();
                                HostedGameList.AddRange(addList);
                                var games = GameManager.Get().Games.ToArray();
                                foreach (var g in HostedGameList)
                                {
                                    var li = list.FirstOrDefault(x => x.Id == g.Id);
                                    g.Update(li, games);
                                }
                                //Log.Info("Visual list refreshed");

                            }));
            }
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

        private void StartJoinGame(HostedGameViewModel hostedGame, DataNew.Entities.Game game)
        {
            if (hostedGame.Data.Source == HostedGameSource.Online)
            {
                var client = new Octgn.Site.Api.ApiClient();
                if (!client.IsGameServerRunning(Program.LobbyClient.Username, Program.LobbyClient.Password))
                {
                    throw new UserMessageException("The game server is currently down. Please try again later.");
                }
            }
            Log.InfoFormat("Starting to join a game {0} {1}", hostedGame.GameId, hostedGame.Name);
            Program.IsHost = false;
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
            Program.GameEngine = new GameEngine(game, username,password);
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
            lock (timer)
            {
                Log.Info("Disconnected");
                isConnected = false;
                _room = null;
                Dispatcher.Invoke(new Action(()=>this.HostedGameList.Clear()));
            }
        }

        void LobbyClient_OnLoginComplete(object sender, LoginResults results)
        {
            lock (timer)
            {
                Log.Info("Connected");
                isConnected = true;
            }
        }

        void LobbyClient_OnDataReceived(object sender, DataRecType type, object data)
        {
            if (type == DataRecType.GameList || type == DataRecType.GamesNeedRefresh)
            {

                //RefreshGameList(null,null);
            }
        }
        #endregion

        #region UI Events

        private void GameListItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
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
            if (hostedgame.Status == EHostedGame.GameInProgress)
            {
                TopMostMessageBox.Show(
                        "You can't join a game in progress.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                return;
            }
            if (hostedgame.Data.Source == HostedGameSource.Online)
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
            var task = new Task(() => this.StartJoinGame(hostedgame, game));
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
                catch{}
                
                Program.GameEngine = null;

            }
            finally
            {
                connectOfflineGameDialog.Dispose();
                connectOfflineGameDialog = null;
            } 
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (timer)
            {
                try
                {
                    if (Program.LobbyClient.IsConnected)
                    {
                        Program.LobbyClient.BeginGetGameList();
                    }
                }
                catch (Exception ex)
                {
                    Log.Warn("Get Custom games timer tick error", ex);
                }
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
            if (hostedgame.Status == EHostedGame.GameInProgress)
            {
                TopMostMessageBox.Show(
                        "You can't join a game in progress.",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                return;
            }
            if (hostedgame.Data.Source == HostedGameSource.Online)
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
            var task = new Task(() => this.StartJoinGame(hostedgame, game));
            task.ContinueWith((t) => { this.Dispatcher.Invoke(new Action(() => this.FinishJoinGame(t))); });
            BorderButtons.IsEnabled = false;
            task.Start();
        }

        private void ButtonJoinOfflineGame(object sender, RoutedEventArgs e)
        {
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

        #region Implementation of IDisposable

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            broadcastListener.StopListening();
			broadcastListener.Dispose();
            ListViewGameList.RemoveHandler(Thumb.DragDeltaEvent, dragHandler);
            Program.LobbyClient.OnLoginComplete -= LobbyClient_OnLoginComplete;
            Program.LobbyClient.OnDisconnect -= LobbyClient_OnDisconnect;
            Program.LobbyClient.OnDataReceived -= LobbyClient_OnDataReceived;
            Program.LobbyClient.Chatting.OnCreateRoom -= ChattingOnOnCreateRoom;

            timer.Elapsed -= this.TimerElapsed;
            timer.Stop();
            timer.Dispose();
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ButtonRefreshClick(object sender, RoutedEventArgs e)
        {
            ButtonRefresh.IsEnabled = false;
            Task.Factory.StartNew(RefreshGamesTask);
        }

        private void RefreshGamesTask()
        {
            try
            {
                if (Program.LobbyClient != null && Program.LobbyClient.IsConnected) 
	                Program.LobbyClient.BeginGetGameList();
                Thread.Sleep(15000);
                Dispatcher.BeginInvoke(new Action(() => this.ButtonRefresh.IsEnabled = true));

            }
            catch (Exception e)
            {
                Log.Warn("RefreshGamesTask Error", e);
            }
        }
    }
}
