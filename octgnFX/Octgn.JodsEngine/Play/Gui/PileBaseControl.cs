using System.Windows;
using System.Windows.Controls;
using Octgn.Controls;
using Octgn.DataNew.Entities;
using Octgn.Scripting.Controls;
using Octgn.Networking;
using System.Collections.Generic;

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

        protected override List<Control> CreateGroupMenuItems(DataNew.Entities.Group def)
        {
            var items = base.CreateGroupMenuItems(def);
            
            // Add protection menu item for piles
            if (group is Pile && group.CanManipulate())
            {
                var protectionItem = CreateProtectionItem();
                if (protectionItem != null)
                {
                    // Find the position to insert the protection item
                    // We want it after the visibility item but before the look at cards item
                    var insertIndex = items.Count;
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i] is MenuItem menuItem && menuItem.Header?.ToString() == "Look at")
                        {
                            insertIndex = i;
                            break;
                        }
                    }
                    items.Insert(insertIndex, protectionItem);
                }
            }
            
            return items;
        }

        private MenuItem CreateProtectionItem()
        {
            var pile = group as Pile;
            if (pile == null) return null;
            
            var item = new MenuItem { Header = "Protection" };
            
            var allowItem = new MenuItem { Header = "Allow viewing", IsCheckable = true };
            allowItem.Click += delegate { pile.SetProtectionState(GroupProtectionState.False, true); };
            item.Items.Add(allowItem);
            
            var blockItem = new MenuItem { Header = "Block viewing", IsCheckable = true };
            blockItem.Click += delegate { pile.SetProtectionState(GroupProtectionState.True, true); };
            item.Items.Add(blockItem);
            
            var askItem = new MenuItem { Header = "Ask permission", IsCheckable = true };
            askItem.Click += delegate { pile.SetProtectionState(GroupProtectionState.Ask, true); };
            item.Items.Add(askItem);
            
            item.SubmenuOpened += delegate
            {
                allowItem.IsChecked = pile.ProtectionState == GroupProtectionState.False;
                blockItem.IsChecked = pile.ProtectionState == GroupProtectionState.True;
                askItem.IsChecked = pile.ProtectionState == GroupProtectionState.Ask;
            };
            
            return item;
        }

        protected void ViewAllCards(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            
            var pile = @group as Pile;
            if (pile != null && !CanViewPile(pile, "all", 0))
                return;
                
            var playWindow = (PlayWindow) Window.GetWindow(this);
            if (playWindow == null) return;
            ChildWindowManager manager = playWindow.wndManager;
            manager.Show(new GroupWindow(@group, PilePosition.All, 0));
        }

        protected void ViewTopCards(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            
            int count = Dialog.InputPositiveInt("View top cards", "How many cards do you want to see?", 1);
            if (count <= 0) return; // User cancelled or entered invalid input
            
            var pile = @group as Pile;
            if (pile != null && !CanViewPile(pile, "top", count))
                return;
            
            var playWindow = (PlayWindow) Window.GetWindow(this);
            if (playWindow == null) return;
            ChildWindowManager manager = playWindow.wndManager;
            manager.Show(new GroupWindow(@group, PilePosition.Top, count));
        }

        protected void ViewBottomCards(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            
            int count = Dialog.InputPositiveInt("View bottom cards", "How many cards do you want to see?", 1);
            if (count <= 0) return; // User cancelled or entered invalid input
            
            var pile = @group as Pile;
            if (pile != null && !CanViewPile(pile, "bottom", count))
                return;
            
            var playWindow = (PlayWindow) Window.GetWindow(this);
            if (playWindow == null) return;
            ChildWindowManager manager = playWindow.wndManager;
            manager.Show(new GroupWindow(@group, PilePosition.Bottom, count));
        }

        private bool CanViewPile(Pile pile, string viewType, int cardCount)
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
                    
                    // Send network request for permission with view details
                    if (Program.Client?.Rpc != null)
                    {
                        Program.Client.Rpc.RequestPileViewPermission(Player.LocalPlayer, pile, pile.Owner, viewType, cardCount);
                        
                        // Create a user-friendly message based on view type
                        string viewDescription;
                        switch (viewType)
                        {
                            case "all":
                                viewDescription = "all cards";
                                break;
                            case "top":
                                viewDescription = $"top {cardCount} card{(cardCount != 1 ? "s" : "")}";
                                break;
                            case "bottom":
                                viewDescription = $"bottom {cardCount} card{(cardCount != 1 ? "s" : "")}";
                                break;
                            default:
                                viewDescription = "cards";
                                break;
                        }
                        
                        Program.GameMess.Warning($"Permission requested to view {viewDescription} in {pile.FullName}. Waiting for {pile.Owner.Name} to grant access.");
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