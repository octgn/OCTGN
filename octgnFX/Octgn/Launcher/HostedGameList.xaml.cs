using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Octgn.Controls;
using Skylabs.Lobby;

namespace Octgn.Launcher
{
    /// <summary>
    /// Interaction logic for GameList.xaml
    /// </summary>
    public partial class HostedGameList : Page
    {
        public HostedGameList()
        {
            InitializeComponent();
            Program.lobbyClient.OnGameHostEvent += lobbyClient_OnGameHostEvent;
        }

        public event EventHandler OnGameClick;

        private void lobbyClient_OnGameHostEvent(HostedGame g)
        {
            Reload_Game_List();
        }

        private void Reload_Game_List()
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 stackPanel1.Children.Clear();
                                                 var gids = new Guid[Program.GamesRepository.Games.Count];
                                                 int count = 0;
                                                 foreach (Data.Game game in Program.GamesRepository.Games)
                                                 {
                                                     gids[count] = game.Id;
                                                     count++;
                                                 }
                                                 HostedGame[] gl =
                                                     Program.lobbyClient.GetHostedGames().OrderByDescending(
                                                         item => item.TimeStarted).ToArray();
                                                 foreach (HostedGame g in gl)
                                                 {
                                                     if (gids.Contains(g.GameGuid) &&
                                                         g.GameStatus == HostedGame.eHostedGame.StartedHosting
                                                         && g.UserHosting.Status != UserStatus.Offline &&
                                                         g.UserHosting.Status != UserStatus.Unknown)
                                                     {
                                                         var gs = new HostedGameListItem(g);
                                                         if (g.GameStatus == HostedGame.eHostedGame.StartedHosting)
                                                             gs.MouseUp += gs_MouseUp;
                                                         stackPanel1.Children.Add(gs);
                                                     }
                                                 }
                                             }));
        }

        private void gs_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var gs = (HostedGameListItem) sender;
            if (OnGameClick != null)
            {
                OnGameClick.Invoke(gs.Game, new EventArgs());
            }
        }

        private void GamesRepository_GameInstalled(object sender, EventArgs e)
        {
            Reload_Game_List();
        }

        private void gs_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var gs = (HostedGameListItem) sender;
            if (OnGameClick != null)
            {
                OnGameClick.Invoke(gs.Game, new EventArgs());
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Reload_Game_List();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Program.lobbyClient.OnGameHostEvent -= lobbyClient_OnGameHostEvent;
        }
    }
}