using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Octgn.Utils;
using Skylabs.Lobby;

namespace Octgn.Controls
{
    using System.Collections.ObjectModel;

    using DriveSync;

    using Microsoft.Scripting.Utils;

    using Octgn.Launcher;
    using Octgn.ViewModels;

    /// <summary>
    /// Interaction logic for CustomGames.xaml
    /// </summary>
    public partial class CustomGameList : SliderPage
    {
        public static DependencyProperty IsJoinableGameSelectedProperty = DependencyProperty.Register(
            "IsJoinableGameSelected", typeof(bool), typeof(CustomGameList));

        public ObservableCollection<HostedGameViewModel> HostedGameList { get; set; }
        public ObservableCollection<DataGameViewModel> Games { get; set; }

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

        private Timer timer;
        private bool isConnected;
        private bool waitingForGames;

        public CustomGameList()
        {
            InitializeComponent();
            HostedGameList = new ObservableCollection<HostedGameViewModel>();
            Program.LobbyClient.OnLoginComplete += LobbyClient_OnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClient_OnDisconnect;
            Program.LobbyClient.OnDataReceived += LobbyClient_OnDataReceived;

            BorderHostGame.Visibility = Visibility.Hidden;

            this.RefreshInstalledGameList();

            this.PreviewKeyUp += OnPreviewKeyUp;

            BorderHostGame.IsVisibleChanged += (sender, args) => this.RefreshInstalledGameList();

            timer = new Timer(10000);
            timer.Start();
            timer.Elapsed += timer_Elapsed;
        }

        void RefreshInstalledGameList()
        {
            if(Games == null)
                Games = new ObservableCollection<DataGameViewModel>();
            var list = Program.GamesRepository.Games.Select(x => new DataGameViewModel(x)).ToList();
            Games.Clear();
            foreach(var l in list)
                Games.Add(l);
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

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Trace.WriteLine("Timer ticks");
            if (!isConnected || waitingForGames) return;
            Trace.WriteLine("Begin refresh games.");
            waitingForGames = true;
            Program.LobbyClient.BeginGetGameList();
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

        private void ListViewGameListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var game = ListViewGameList.SelectedItem as HostedGameViewModel;
            this.IsJoinableGameSelected = game != null && game.CanPlay;
        }

        private void ButtonHostClick(object sender, RoutedEventArgs e)
        {
            this.ShowHostGameDialog();
        }

        private void ButtonJoinClick(object sender, RoutedEventArgs e)
        {
            this.NavigateForward();
        }

        private void ButtonCancelClick(object sender, RoutedEventArgs e)
        {
            this.HideHostGameDialog();
        }

        private void ButtonHostGameStartClick(object sender, RoutedEventArgs e)
        {
            this.HideHostGameDialog();
            this.NavigateForward();
        }

        private void ShowHostGameDialog()
        {
            BorderHostGame.Visibility = Visibility.Visible;
            BorderButtons.IsEnabled = false;
        }

        private void HideHostGameDialog()
        {
            BorderHostGame.Visibility = Visibility.Hidden;
            BorderButtons.IsEnabled = true;
        }

        private void ToggleHostGameDialog()
        {
            if (BorderHostGame.Visibility == Visibility.Visible)
            {
                this.HideHostGameDialog();
            }
            else
            {
                this.ShowHostGameDialog();
            }
        }
    }
}
