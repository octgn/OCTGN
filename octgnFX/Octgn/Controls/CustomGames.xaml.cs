using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Octgn.Extentions;
using Skylabs.Lobby;

namespace Octgn.Controls
{
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Windows.Controls.Primitives;
    using System.Windows.Forms;

    using Microsoft.Scripting.Utils;

    using Octgn.Core.DataManagers;
    using Octgn.Library.Exceptions;
    using Octgn.Networking;
    using Octgn.Scripting.Controls;
    using Octgn.ViewModels;
    using Octgn.Windows;

    using log4net;

    using Timer = System.Timers.Timer;

    /// <summary>
    /// Interaction logic for CustomGames.xaml
    /// </summary>
    public partial class CustomGameList:IDisposable
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

        private readonly Timer timer;
        private bool isConnected;
        private HostGameSettings hostGameDialog;
        private ConnectOfflineGame connectOfflineGameDialog;

        private readonly DragDeltaEventHandler dragHandler;

        public CustomGameList()
        {
            InitializeComponent();
            dragHandler = this.ListViewGameList_OnDragDelta;
            ListViewGameList.AddHandler(Thumb.DragDeltaEvent, dragHandler, true);
            HostedGameList = new ObservableCollection<HostedGameViewModel>();
	        HideUninstalledGames.IsChecked = Prefs.HideUninstalledGamesInList;
            Program.LobbyClient.OnLoginComplete += LobbyClient_OnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClient_OnDisconnect;
            Program.LobbyClient.OnDataReceived += LobbyClient_OnDataReceived;

            timer = new Timer(10000);
            timer.Start();
            timer.Elapsed += this.TimerElapsed;
        }

        void RefreshGameList()
        {
            Log.Info("Refreshing list...");
            var list = Program.LobbyClient.GetHostedGames().Select(x => new HostedGameViewModel(x)).ToList();
            Log.Info("Got hosted games list");

	        Dispatcher.Invoke(new Action(() =>
                                             {
                                                 Log.Info("Refreshing visual list");

												 var hideGames = HideUninstalledGames.IsChecked ?? false;
												 if (hideGames)
												 {
													 list = list.Where(game => game.GameName != "{Unknown Game}").ToList();
												 }

                                                 var removeList = HostedGameList.Where(i => list.All(x => x.Port != i.Port)).ToList();
                                                 removeList.ForEach(x => HostedGameList.Remove(x));
                                                 var addList = list.Where(i => this.HostedGameList.All(x => x.Port != i.Port)).ToList();
                                                 HostedGameList.AddRange(addList);
                                                 foreach (var g in HostedGameList)
                                                     g.Update();
                                                 Log.Info("Visual list refreshed");

                                             }));
        }

        private void ShowHostGameDialog()
        {
            hostGameDialog = new HostGameSettings();
            hostGameDialog.Show(DialogPlaceHolder);
            hostGameDialog.OnClose += HostGameSettingsDialogOnClose;
            BorderButtons.IsEnabled = false;
        }

        private void HideHostGameDialog()
        {
            hostGameDialog.Close();
        }

        private void ShowJoinOfflineGameDialog()
        {
            connectOfflineGameDialog = new ConnectOfflineGame();
            connectOfflineGameDialog.Show(DialogPlaceHolder);
            connectOfflineGameDialog.OnClose += ConnectOfflineGameDialogOnClose;
            BorderButtons.IsEnabled = false;
        }

        private void HideJoinOfflineGameDialog()
        {
            connectOfflineGameDialog.Close();
        }

        private void StartJoinGame(HostedGameViewModel hostedGame, DataNew.Entities.Game game)
        {
            var client = new Octgn.Site.Api.ApiClient();
            if (!client.IsGameServerRunning(Program.LobbyClient.Username, Program.LobbyClient.Password))
            {
                throw new UserMessageException("The game server is currently down. Please try again later.");
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
            Program.GameEngine = new GameEngine(game, Program.LobbyClient.Me.UserName,password);
            Program.CurrentOnlineGameName = hostedGame.Name;
            IPAddress hostAddress = Dns.GetHostAddresses(AppConfig.GameServerPath).FirstOrDefault();
            if (hostAddress == null)
            {
                Log.WarnFormat("Dns Error, couldn't resolve {0}", AppConfig.GameServerPath);
                throw new UserMessageException("There was a problem with your DNS. Please try again.");
            }

            try
            {
                Log.InfoFormat("Creating client for {0}:{1}", hostAddress, hostedGame.Port);
                Program.Client = new Client(hostAddress, hostedGame.Port);
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
            WindowManager.PreGameLobbyWindow = new PreGameLobbyWindow();
            WindowManager.PreGameLobbyWindow.Setup(false, WindowManager.Main);
        }

        #region LobbyEvents
        void LobbyClient_OnDisconnect(object sender, EventArgs e)
        {
            Log.Info("Disconnected");
            isConnected = false;
        }

        void LobbyClient_OnLoginComplete(object sender, LoginResults results)
        {
            Log.Info("Connected");
            isConnected = true;
        }

        void LobbyClient_OnDataReceived(object sender, DataRecType type, object data)
        {
            if (type == DataRecType.GameList || type == DataRecType.GamesNeedRefresh)
            {
                Log.Info("Games List Received");
                RefreshGameList();
            }
        }
        #endregion

        #region UI Events

        private void GameListItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (WindowManager.PreGameLobbyWindow != null || WindowManager.PlayWindow != null)
            {
                MessageBox.Show(
                    "You are currently in a game or game lobby. Please leave before you join game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            var client = new Octgn.Site.Api.ApiClient();
            if (!client.IsGameServerRunning(Program.LobbyClient.Username, Program.LobbyClient.Password))
            {
                TopMostMessageBox.Show("The game server is currently down. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var hostedgame = ListViewGameList.SelectedItem as HostedGameViewModel;
            if (hostedgame == null) return;
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
                    if (WindowManager.PreGameLobbyWindow == null)
                    {
                        WindowManager.PreGameLobbyWindow = new PreGameLobbyWindow();
                        WindowManager.PreGameLobbyWindow.Setup(hostGameDialog.IsLocalGame, WindowManager.Main);
                    }
                }
            }
            hostGameDialog.Dispose();
            hostGameDialog = null;
        }

        private void ConnectOfflineGameDialogOnClose(object o, DialogResult dialogResult)
        {
            BorderButtons.IsEnabled = true;
            if (dialogResult == DialogResult.OK)
            {
                if (connectOfflineGameDialog.Successful)
                {
                    if (WindowManager.PreGameLobbyWindow == null)
                    {
                        Program.IsHost = false;
                        Program.GameEngine = new Octgn.GameEngine(connectOfflineGameDialog.Game, null,connectOfflineGameDialog.Password, true);

                        WindowManager.PreGameLobbyWindow = new PreGameLobbyWindow();
                        WindowManager.PreGameLobbyWindow.Setup(true, WindowManager.Main);
                    }
                }
            }
            connectOfflineGameDialog.Dispose();
            connectOfflineGameDialog = null;
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Program.LobbyClient.IsConnected)
                {
                    Log.Info("Refresh game list timer ticks");
                    Program.LobbyClient.BeginGetGameList();
                }
            }
            catch (Exception ex)
            {
                Log.Warn("Get Custom games timer tick error", ex);
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
            if (WindowManager.PreGameLobbyWindow != null || WindowManager.PlayWindow != null)
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
            if (WindowManager.PreGameLobbyWindow != null || WindowManager.PlayWindow != null)
            {
                MessageBox.Show(
                    "You are currently in a game or game lobby. Please leave before you join game.",
                    "OCTGN",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            var client = new Octgn.Site.Api.ApiClient();
            if (!client.IsGameServerRunning(Program.LobbyClient.Username, Program.LobbyClient.Password))
            {
                TopMostMessageBox.Show("The game server is currently down. Please try again later.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var hostedgame = ListViewGameList.SelectedItem as HostedGameViewModel;
            if (hostedgame == null) return;
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
            if (WindowManager.PreGameLobbyWindow != null || WindowManager.PlayWindow != null)
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
            ListViewGameList.RemoveHandler(Thumb.DragDeltaEvent, dragHandler);
            Program.LobbyClient.OnLoginComplete -= LobbyClient_OnLoginComplete;
            Program.LobbyClient.OnDisconnect -= LobbyClient_OnDisconnect;
            Program.LobbyClient.OnDataReceived -= LobbyClient_OnDataReceived;

            timer.Elapsed -= this.TimerElapsed;
            timer.Stop();
            timer.Dispose();
        }

        #endregion

	    private void HideUninstalledGames_OnClick(object sender, RoutedEventArgs e)
	    {
		    Prefs.HideUninstalledGamesInList = HideUninstalledGames.IsChecked.ToBool();
			RefreshGameList();
	    }
    }
}
