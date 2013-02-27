using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Skylabs.Lobby;

namespace Octgn.Controls
{
    using System.Collections.ObjectModel;
    using System.Net;
    using System.Threading.Tasks;
    using System.Windows.Controls.Primitives;
    using System.Windows.Forms;

    using Microsoft.Scripting.Utils;

    using Octgn.Definitions;
    using Octgn.Library.Exceptions;
    using Octgn.Networking;
    using Octgn.ViewModels;
    using Octgn.Windows;

    using KeyEventArgs = System.Windows.Input.KeyEventArgs;
    using Timer = System.Timers.Timer;

    /// <summary>
    /// Interaction logic for CustomGames.xaml
    /// </summary>
    public partial class CustomGameList
    {
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
                SetValue(IsJoinableGameSelectedProperty,value);
            }
        }

        private readonly Timer timer;
        private bool isConnected;
        private bool waitingForGames;
        private HostGameSettings hostGameDialog;
        private ConnectOfflineGame connectOfflineGameDialog;

        public CustomGameList()
        {
            InitializeComponent();
            ListViewGameList.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(ListViewGameList_OnDragDelta), true);
            HostedGameList = new ObservableCollection<HostedGameViewModel>();
            Program.LobbyClient.OnLoginComplete += LobbyClient_OnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClient_OnDisconnect;
            Program.LobbyClient.OnDataReceived += LobbyClient_OnDataReceived;

            timer = new Timer(10000);
            timer.Start();
            timer.Elapsed += this.TimerElapsed;
        }

        void RefreshGameList()
        {
            Trace.WriteLine("Refreshing list...");
            var list = Program.LobbyClient.GetHostedGames().Select(x => new HostedGameViewModel(x)).ToList();
            Dispatcher.Invoke(new Action(() =>
            {
                var removeList = HostedGameList.Where(i => !list.Any(x => x.Port == i.Port)).ToList();
                removeList.ForEach(x => HostedGameList.Remove(x));
                var addList = list.Where(i => !HostedGameList.Any(x => x.Port == i.Port)).ToList();
                HostedGameList.AddRange(addList);
                foreach(var g in HostedGameList)
                    g.Update();
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

        private void StartJoinGame(HostedGameViewModel hostedGame, Data.Game game)
        {
            Program.IsHost = false;
            Program.Game = new Game(GameDef.FromO8G(game.FullPath));
            Program.CurrentOnlineGameName = hostedGame.Name;
            IPAddress hostAddress =  Dns.GetHostAddresses(Program.GameServerPath).FirstOrDefault();
            if(hostAddress == null)
                throw new UserMessageException("There was a problem with your DNS. Please try again.");

            try
            {
                Program.Client = new Client(hostAddress,hostedGame.Port);
                Program.Client.Connect();
            }
            catch (Exception)
            {
                throw new UserMessageException("Could not connect. Please try again.");
            }
            
        }

        private void FinishJoinGame(Task task)
        {
            BorderButtons.IsEnabled = true;
            if (task.IsFaulted)
            {
                var error = "Unknown Error: Please try again";
                if (task.Exception != null)
                {
                    var umException = task.Exception.InnerExceptions.OfType<UserMessageException>().FirstOrDefault();
                    if (umException != null)
                    {
                        error = umException.Message;
                    }
                }
                MessageBox.Show(error, "OCTGN", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Program.PreGameLobbyWindow = new PreGameLobbyWindow();
            Program.PreGameLobbyWindow.Setup(false,Program.MainWindowNew);
        }

        #region LobbyEvents
        void LobbyClient_OnDisconnect(object sender, EventArgs e)
        {
            isConnected = false;
        }

        void LobbyClient_OnLoginComplete(object sender, LoginResults results)
        {
            isConnected = true;
        }

        void LobbyClient_OnDataReceived(object sender, DataRecType type, object data) 
        {
            if (type == DataRecType.GameList || type == DataRecType.GamesNeedRefresh)
            {
                Trace.WriteLine("Games Received");
                RefreshGameList();
                waitingForGames = false;
            }
        }
        #endregion

        #region UI Events

        private void HostGameSettingsDialogOnClose(object o, DialogResult dialogResult)
        {
            BorderButtons.IsEnabled = true;
            if (dialogResult == DialogResult.OK)
            {
                if (hostGameDialog.SuccessfulHost)
                {
                    if (Program.PreGameLobbyWindow == null)
                    {
                        Program.PreGameLobbyWindow = new PreGameLobbyWindow();
                        Program.PreGameLobbyWindow.Setup(hostGameDialog.IsLocalGame,Program.MainWindowNew);
                    }
                }
            }
        }

        private void ConnectOfflineGameDialogOnClose(object o, DialogResult dialogResult)
        {
            BorderButtons.IsEnabled = true;
            if (dialogResult == DialogResult.OK)
            {
                if (connectOfflineGameDialog.Successful)
                {
                    if (Program.PreGameLobbyWindow == null)
                    {
                        Program.IsHost = false;
                        Program.Game = new Octgn.Game(GameDef.FromO8G(connectOfflineGameDialog.Game.FullPath), true);

                        Program.PreGameLobbyWindow = new PreGameLobbyWindow();
                        Program.PreGameLobbyWindow.Setup(true,Program.MainWindowNew);
                    }
                }
            }
        }

        void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Trace.WriteLine("Timer ticks");
            if (!isConnected || waitingForGames) return;
            Trace.WriteLine("Begin refresh games.");
            waitingForGames = true;
            Program.LobbyClient.BeginGetGameList();
        }

        private void ListViewGameListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var game = ListViewGameList.SelectedItem as HostedGameViewModel;
            this.IsJoinableGameSelected = game != null && game.CanPlay;
        }

        private void ButtonHostClick(object sender, RoutedEventArgs e)
        {
            if (Program.PreGameLobbyWindow != null || Program.PlayWindow != null)
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
            if (Program.PreGameLobbyWindow != null || Program.PlayWindow != null)
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
            var game = Program.GamesRepository.Games.FirstOrDefault(x => x.Id == hostedgame.GameId);
            var task = new Task(() => this.StartJoinGame(hostedgame,game));
            task.ContinueWith((t) =>{ this.Dispatcher.Invoke(new Action(() => this.FinishJoinGame(t)));});
            BorderButtons.IsEnabled = false;
            task.Start();

            //this.NavigateForward();
        }

        private void ButtonJoinOfflineGame(object sender, RoutedEventArgs e)
        {
            if (Program.PreGameLobbyWindow != null || Program.PlayWindow != null)
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
            if(e == null || e.OriginalSource == null)return;
            var senderAsThumb = e.OriginalSource as Thumb;
            if(senderAsThumb == null || senderAsThumb.TemplatedParent == null)return;
            var header = senderAsThumb.TemplatedParent as GridViewColumnHeader;
            if(header == null)return;
            if (header.Column.ActualWidth < 20)
                header.Column.Width = 20;
            //if (header.Column.ActualWidth > 100)
            //    header.Column.Width = 100;
        }
    }
}
