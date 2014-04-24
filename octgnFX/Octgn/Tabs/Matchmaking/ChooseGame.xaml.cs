using System;
using System.Collections.ObjectModel;
using System.Linq;
using Octgn.Core.DataManagers;
using Octgn.DataNew.Entities;

namespace Octgn.Tabs.Matchmaking
{
    public partial class ChooseGame
    {
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
    }
}
