using System.Windows.Controls;
using System.Windows.Input;

namespace Octgn.Tabs.Matchmaking
{
    /// <summary>
    /// Interaction logic for InQueueView.xaml
    /// </summary>
    public partial class InQueueView
    {
        public InQueueView()
        {
            InitializeComponent();
        }

        private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            var dc = DataContext as MatchmakingTabViewModel;
            dc.LeaveQueue();
        }
    }
}
