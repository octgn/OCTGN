using System.Windows;
using System.Windows.Controls;
using Octgn.Controls;
using Octgn.DataNew.Entities;
using Octgn.Scripting.Controls;
using Octgn.Networking;

namespace Octgn.Play.Gui
{
    public class PileBaseControl : GroupControl
    {
        private MenuItem _lookAtCardsMenuItem;

        static PileBaseControl()
        {
            // Subscribe to the permission granted event
            Handler.PileViewPermissionGranted += OnPileViewPermissionGranted;
        }

        private static void OnPileViewPermissionGranted(Group pile)
        {
            // When permission is granted, automatically show the pile view
            Program.Dispatcher.BeginInvoke(new System.Action(() =>
            {
                try
                {
                    var playWindow = WindowManager.PlayWindow;
                    if (playWindow != null)
                    {
                        // Find the window manager and show the group window
                        var manager = playWindow.GetValue(System.Windows.FrameworkElement.TagProperty) as ChildWindowManager;
                        if (manager == null)
                        {
                            // Try to find the window manager through other means
                            if (System.Windows.Application.Current?.MainWindow is PlayWindow mainWindow)
                            {
                                // Use reflection to access the wndManager field
                                var field = typeof(PlayWindow).GetField("wndManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                if (field != null)
                                {
                                    manager = field.GetValue(mainWindow) as ChildWindowManager;
                                }
                            }
                        }
                        
                        if (manager != null)
                        {
                            manager.Show(new GroupWindow(pile, PilePosition.All, 0));
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    // Log error but don't show to user
                    System.Diagnostics.Debug.WriteLine($"Error showing pile view: {ex.Message}");
                }
            }));
        }

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
                        Program.Client.Rpc.RequestPileViewPermission(Player.LocalPlayer, pile, pile.Owner);
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