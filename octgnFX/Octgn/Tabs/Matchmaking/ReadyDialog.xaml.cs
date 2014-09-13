using System.Reflection;
using System.Windows;
using log4net;

namespace Octgn.Tabs.Matchmaking
{
    public partial class ReadyDialog 
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ReadyDialog()
        {
            InitializeComponent();
        }


        private void OnReadyClick(object sender, RoutedEventArgs e)
        {
			Log.Info("Ready Clicked");
            var dx = DataContext as MatchmakingTabViewModel;
			dx.SignalReady();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
			Log.Info("Cancel Clicked");
            var dx = DataContext as MatchmakingTabViewModel;
			dx.HideReadyDialog();
        }
    }
}
