using System.Windows;
using System.Windows.Controls;
using Octgn.Controls;
using Octgn.DataNew.Entities;
using Octgn.Scripting.Controls;

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
            
            var pile = @group as Pile;
            if (pile != null && !CanViewPile(pile))
                return;
                
            var playWindow = (PlayWindow) Window.GetWindow(this);
            if (playWindow == null) return;
            ChildWindowManager manager = playWindow.wndManager;
            manager.Show(new GroupWindow(@group, PilePosition.All, 0));
        }

        protected void ViewTopCards(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            
            var pile = @group as Pile;
            if (pile != null && !CanViewPile(pile))
                return;
                
            int count = Dialog.InputPositiveInt("View top cards", "How many cards do you want to see?", 1);
            var playWindow = (PlayWindow) Window.GetWindow(this);
            if (playWindow == null) return;
            ChildWindowManager manager = playWindow.wndManager;
            manager.Show(new GroupWindow(@group, PilePosition.Top, count));
        }

        protected void ViewBottomCards(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            
            var pile = @group as Pile;
            if (pile != null && !CanViewPile(pile))
                return;
                
            int count = Dialog.InputPositiveInt("View bottom cards", "How many cards do you want to see?", 1);
            var playWindow = (PlayWindow) Window.GetWindow(this);
            if (playWindow == null) return;
            ChildWindowManager manager = playWindow.wndManager;
            manager.Show(new GroupWindow(@group, PilePosition.Bottom, count));
        }

        private bool CanViewPile(Pile pile)
        {
            // If the player owns the pile, they can always view it
            if (pile.Owner == Player.LocalPlayer)
                return true;
                
            switch (pile.ProtectionState)
            {
                case GroupProtectionState.False:
                    return true;
                    
                case GroupProtectionState.True:
                    Program.GameMess.Warning($"Cannot view {pile.FullName} - it is protected.");
                    return false;
                    
                case GroupProtectionState.Ask:
                    // Check if we have temporary permission
                    if (pile.TemporaryViewPermissions.Contains(Player.LocalPlayer))
                    {
                        return true;
                    }
                    
                    // Send network request for permission if client is available
                    if (Program.Client?.Rpc != null)
                    {
                        Program.Client.Rpc.RequestPileViewPermission(pile, Player.LocalPlayer);
                        Program.GameMess.Warning($"Permission requested to view {pile.FullName}. Waiting for {pile.Owner.Name} to grant access.");
                    }
                    else
                    {
                        Program.GameMess.Warning($"Cannot request permission to view {pile.FullName} - not connected to game.");
                    }
                    return false;
                    
                default:
                    return true;
            }
        }
    }
}