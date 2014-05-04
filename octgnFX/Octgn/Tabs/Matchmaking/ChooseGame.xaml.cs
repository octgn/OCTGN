/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Octgn.Core.DataManagers;
using Octgn.DataNew.Entities;

namespace Octgn.Tabs.Matchmaking
{
    public partial class ChooseGame
    {
        public ObservableCollection<Game> Games { get; set; }

        public event Action<Game> OnChooseGame;

        public ChooseGame()
        {
			Games = new ObservableCollection<Game>();
            InitializeComponent();
			GameManager.Get().GameListChanged += OnGameListChanged;
            rebuildList();
        }

        private void OnGameListChanged(object sender, EventArgs eventArgs)
        {
            rebuildList();
        }

        private void rebuildList()
        {
			if(Dispatcher.CheckAccess() == false)
            {
                Dispatcher.Invoke(new Action(rebuildList));
                return;
            }
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
            var fe = sender as FrameworkElement;
            if (fe == null)
                return;
            var game = fe.DataContext as Game;
            if (game == null)
                return;
			if(OnChooseGame != null)
				OnChooseGame(game);
        }
    }
}
