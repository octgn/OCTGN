/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using log4net;
using Octgn.Core.DataManagers;
using Octgn.DataNew.Entities;

namespace Octgn.Tabs.Matchmaking
{
    public partial class ChooseGame
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ObservableCollection<Game> Games { get; set; }

        public ChooseGame()
        {
			Games = new ObservableCollection<Game>();
            InitializeComponent();
			GameManager.Get().GameListChanged += OnGameListChanged;
            rebuildList();
        }

        private void OnGameListChanged(object sender, EventArgs eventArgs)
        {
			Log.Info("OnGameListChanged");
            rebuildList();
        }

        private void rebuildList()
        {
			if(Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke(new Action(rebuildList));
                return;
            }
			Log.Info("Rebuilding game list");
            var installedGames = GameManager.Get().Games.ToArray();
            foreach (var g in installedGames)
            {
                if (Games.Any(x=>x.Id == g.Id))
                    continue;
				Games.Add(g);
            }
            foreach (var g in Games.ToArray())
            {
                if (installedGames.Any(x => x.Id == g.Id) == false)
                {
                    Games.Remove(g);
                }
            }
        }

        private void OnGameMouseUp(object sender, MouseButtonEventArgs e)
        {
			Log.Info("Clicked on game");
            var fe = sender as FrameworkElement;
            if (fe == null)
                return;
            var game = fe.DataContext as Game;
            if (game == null)
                return;
            var vm = DataContext as MatchmakingTabViewModel;
			vm.PickGame(game);
        }
    }
}
