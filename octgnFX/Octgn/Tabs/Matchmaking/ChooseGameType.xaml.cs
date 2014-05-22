using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using log4net;

namespace Octgn.Tabs.Matchmaking
{
    public partial class ChooseGameType
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public List<GameType> GameTypeList { get; set; }

        public ChooseGameType()
        {
            InitializeComponent();
        }

        private void OnGameTypeMouseUp(object sender, MouseButtonEventArgs e)
        {
			Log.Info("Clicked on game type");
            var fe = sender as FrameworkElement;
            if (fe == null)
                return;
            var gameType = fe.DataContext as GameType;
            if (gameType == null)
                return;
            var vm = DataContext as MatchmakingTabViewModel;
			vm.PickGameType(gameType);
        }
    }
}
