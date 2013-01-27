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
    using System.Windows.Forms;

    using Microsoft.Scripting.Utils;

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

        public CustomGameList()
        {
            InitializeComponent();
            HostedGameList = new ObservableCollection<HostedGameViewModel>();
            Program.LobbyClient.OnLoginComplete += LobbyClient_OnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClient_OnDisconnect;
            Program.LobbyClient.OnDataReceived += LobbyClient_OnDataReceived;

            this.PreviewKeyUp += OnPreviewKeyUp;

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

        private void ToggleHostGameDialog()
        {
            if (hostGameDialog != null)
            {
                this.HideHostGameDialog();
            }
            else
            {
                this.ShowHostGameDialog();
            }
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
                        Program.PreGameLobbyWindow.Setup();
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

        private void OnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.Escape)
            {
                this.HideHostGameDialog();
            }
            else if (keyEventArgs.Key == Key.H)
            {
                this.ToggleHostGameDialog();
            }
        }

        private void ButtonHostClick(object sender, RoutedEventArgs e)
        {
            this.ShowHostGameDialog();
        }

        private void ButtonJoinClick(object sender, RoutedEventArgs e)
        {
            //this.NavigateForward();
        }

        #endregion

    }
}
