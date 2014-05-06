/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows.Media.Animation;
using Octgn.DataNew.Entities;

namespace Octgn.Tabs.Matchmaking
{
    public partial class MatchmakingTab 
    {
        public MatchmakingTab()
        {
            InitializeComponent();
        }

        private void ChooseGame_OnChooseGame(Game obj)
        {
            ChooseGameModeView.SelectedGame = obj;
            ChooseGameModeView.IsHitTestVisible = true;
            ChooseGameView.IsHitTestVisible = false;
            var sl = this.FindResource("SlideLeftHide") as Storyboard;
			Storyboard.SetTarget(sl,ChooseGameView);
			sl.Begin();

            sl = this.FindResource("SlideLeftShow") as Storyboard;
			Storyboard.SetTarget(sl,ChooseGameModeView);
			sl.Begin();
        }

        private void ChooseGame_OnChooseGameMode(GameMode obj)
        {
            ChooseGameView.IsHitTestVisible = false;

            var sl = this.FindResource("SlideLeftHide") as Storyboard;
            Storyboard.SetTarget(sl, ChooseGameModeView);
			sl.Begin();
        }
    }
}
