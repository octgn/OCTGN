using System.Windows;
using System.Windows.Controls;
using Octgn.Controls;
using Octgn.Script;

namespace Octgn.Play.Gui
{
    public class PileBaseControl : GroupControl
    {
        private MenuItem _lookAtCardsMenuItem;

        private void BuildLookAtCardsMenuItem()
        {
            _lookAtCardsMenuItem = new MenuItem {Header = "Look at"};

            var top = new MenuItem {Header = "Top X cards..."};
            top.Click += ViewTopCards;
            _lookAtCardsMenuItem.Items.Add(top);

            var all = new MenuItem {Header = "All cards"};
            all.Click += ViewAllCards;
            _lookAtCardsMenuItem.Items.Add(all);

            var bottom = new MenuItem {Header = "Bottom X cards..."};
            bottom.Click += ViewBottomCards;
            _lookAtCardsMenuItem.Items.Add(bottom);
        }

        protected override MenuItem CreateLookAtCardsMenuItem()
        {
            if (_lookAtCardsMenuItem == null) BuildLookAtCardsMenuItem();
            return _lookAtCardsMenuItem;
        }

        protected void ViewAllCards(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            var playWindow = (PlayWindow) Window.GetWindow(this);
            if (playWindow == null) return;
            ChildWindowManager manager = playWindow.wndManager;
            manager.Show(new GroupWindow(@group, PilePosition.All, 0));
        }

        protected void ViewTopCards(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            int count = OCTGN.InputPositiveInt("View top cards", "How many cards do you want to see?", 1);
            var playWindow = (PlayWindow) Window.GetWindow(this);
            if (playWindow == null) return;
            ChildWindowManager manager = playWindow.wndManager;
            manager.Show(new GroupWindow(@group, PilePosition.Top, count));
        }

        protected void ViewBottomCards(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            int count = OCTGN.InputPositiveInt("View bottom cards", "How many cards do you want to see?", 1);
            var playWindow = (PlayWindow) Window.GetWindow(this);
            if (playWindow == null) return;
            ChildWindowManager manager = playWindow.wndManager;
            manager.Show(new GroupWindow(@group, PilePosition.Bottom, count));
        }
    }
}