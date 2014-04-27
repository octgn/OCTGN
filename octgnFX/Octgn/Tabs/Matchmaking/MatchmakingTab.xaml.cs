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
