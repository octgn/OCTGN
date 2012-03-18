using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Octgn.Controls;
using Skylabs.Lobby;

namespace Octgn.Launcher
{
    /// <summary>
    ///   Interaction logic for GameList.xaml
    /// </summary>
    public partial class HostedGameList
    {
        public HostedGameList()
        {
            InitializeComponent();
            Program.LClient.BeginGetGameList();
            Program.LClient.OnDataRecieved += LClientOnOnDataRecieved;
        }

        private void LClientOnOnDataRecieved(object sender , Client.DataRecType type , object data)
        {
            if(type == Client.DataRecType.GameList)
                ReloadGameList();
            else if(type == Client.DataRecType.GamesNeedRefresh)
                Program.LClient.BeginGetGameList();
        }

        public event EventHandler OnGameClick;

        private void ReloadGameList()
        {
            Dispatcher.Invoke(new Action(() =>
                                             {
                                                 stackPanel1.Children.Clear();
                                                 var gids = new Guid[Program.GamesRepository.Games.Count];
                                                 int count = 0;
                                                 var filterList = new Guid[Program.GamesRepository.Games.Count];
                                                 foreach (Data.Game game in Program.GamesRepository.Games)
                                                 {
                                                     gids[count] = game.Id;
                                                     if (SimpleConfig.ReadValue("FilterGames_"+game.Name) == "False")
                                                        filterList[count] = game.Id;
                                                     count++;
                                                 }

                                                 HostedGameData[] gl =
                                                     Program.LClient.GetHostedGames().OrderByDescending(
                                                         item => item.TimeStarted).ToArray();
                                                 foreach (HostedGameData g in gl)
                                                 {
                                                     if (!gids.Contains(g.GameGuid) ||
                                                         g.GameStatus != EHostedGame.StartedHosting) continue;
                                                     var gs = new HostedGameListItem(g);
                                                     if (g.GameStatus == EHostedGame.StartedHosting)
                                                         gs.MouseUp += GsMouseUp;
                                                     stackPanel1.Children.Add(gs);
                                                     if (filterList.Contains(g.GameGuid))
                                                        gs.Visibility= Visibility.Collapsed;
                                                 }
                                             }));
        }

        private void GsMouseUp(object sender, MouseButtonEventArgs e)
        {
            var gs = (HostedGameListItem) sender;
            if (OnGameClick != null)
            {
                OnGameClick.Invoke(gs.Game, new EventArgs());
            }
        }

        private void GamesRepositoryGameInstalled(object sender, EventArgs e)
        {
            ReloadGameList();
        }

        private void GsMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var gs = (HostedGameListItem) sender;
            if (OnGameClick != null)
            {
                OnGameClick.Invoke(gs.Game, new EventArgs());
            }
        }

        private void PageLoaded(object sender, RoutedEventArgs e)
        {
            ReloadGameList();
        }

        private void PageUnloaded(object sender, RoutedEventArgs e)
        {
            Program.LClient.OnDataRecieved -= LClientOnOnDataRecieved;
        }

        public void FilterGames(Guid g, Boolean show)
        {
            //let's cycle through the game list and hide/show them from the StackPanel
            foreach (HostedGameListItem hg in stackPanel1.Children.Cast<HostedGameListItem>().Where(hg => hg.Game.GameGuid == g))
            {
                hg.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}