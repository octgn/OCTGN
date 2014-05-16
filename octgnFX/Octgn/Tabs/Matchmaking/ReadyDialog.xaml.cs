using System.Windows;

namespace Octgn.Tabs.Matchmaking
{
    public partial class ReadyDialog 
    {
        public ReadyDialog()
        {
            InitializeComponent();
        }


        private void OnReadyClick(object sender, RoutedEventArgs e)
        {
            var dx = DataContext as MatchmakingTabViewModel;
			dx.SignalReady();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            var dx = DataContext as MatchmakingTabViewModel;
			dx.HideReadyDialog();
        }
    }
}
