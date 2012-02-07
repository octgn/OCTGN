using System.Windows;
using System.Windows.Controls;
using Octgn.Controls;
using Octgn.Script;

namespace Octgn.Play.Gui
{
    public class PileBaseControl : GroupControl
    {
        private MenuItem LookAtCardsMenuItem;

        private void BuildLookAtCardsMenuItem()
        {
            LookAtCardsMenuItem = new MenuItem {Header = "Look at"};

            var top = new MenuItem {Header = "Top X cards..."};
            top.Click += ViewTopCards;
            LookAtCardsMenuItem.Items.Add(top);

            var all = new MenuItem {Header = "All cards"};
            all.Click += ViewAllCards;
            LookAtCardsMenuItem.Items.Add(all);

            var bottom = new MenuItem {Header = "Bottom X cards..."};
            bottom.Click += ViewBottomCards;
            LookAtCardsMenuItem.Items.Add(bottom);
        }

        protected override MenuItem CreateLookAtCardsMenuItem()
        {
            if (LookAtCardsMenuItem == null) BuildLookAtCardsMenuItem();
            return LookAtCardsMenuItem;
        }

        protected void ViewAllCards(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            ChildWindowManager manager = ((PlayWindow) Window.GetWindow(this)).wndManager;
            manager.Show(new GroupWindow(group, PilePosition.All, 0));
        }

        protected void ViewTopCards(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            int count = OCTGN.InputPositiveInt("View top cards", "How many cards do you want to see?", 1);
            ChildWindowManager manager = ((PlayWindow) Window.GetWindow(this)).wndManager;
            manager.Show(new GroupWindow(group, PilePosition.Top, count));
        }

        protected void ViewBottomCards(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            int count = OCTGN.InputPositiveInt("View bottom cards", "How many cards do you want to see?", 1);
            ChildWindowManager manager = ((PlayWindow) Window.GetWindow(this)).wndManager;
            manager.Show(new GroupWindow(group, PilePosition.Bottom, count));
        }
    }
}