using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using Octgn.DataNew.Entities;

namespace Octgn.Play.Actions
{
    internal sealed class Shake : ActionBase
    {
        private readonly Card _card;
        private readonly Player _who;

        public Shake(Player who, Card card)
        {
            _who = who;
            _card = card;
        }

        public override void Do()
        {
            base.Do();

            // If card is in a pile or hand, focus the player's tab BEFORE the animation starts
            var did_focus = false;
            if (_card.Group != null && _card.Group != Program.GameEngine.Table)
            {
                did_focus = FocusPlayerTab(_card.Group.Owner);
            }

            if (!did_focus) {
                _card.DoShake();
            } else {
                Program.Dispatcher.InvokeAsync(async () => {
                    await Task.Delay(100);
                    _card.DoShake();
                });
            }

            // Generate appropriate message based on card visibility and group type
            var (message, args) = GetShakeMessage();
            Program.GameMess.PlayerEvent(_who, message, args);
        }
        
        private bool FocusPlayerTab(Player player)
        {
            try
            {
                var windowManager = WindowManager.PlayWindow;
                if (windowManager == null) return false;
                
                // Check if the "Switch Tabs on Shake" setting is enabled
                if (!windowManager.SwitchTabsOnShake) return false;
                
                // Use reflection to access the private playerTabs field
                var playerTabsField = windowManager.GetType().GetField("playerTabs", BindingFlags.NonPublic | BindingFlags.Instance);
                if (playerTabsField?.GetValue(windowManager) is TabControl playerTabs)
                {
                    // Find the tab for this player
                    foreach (Player tab_player in playerTabs.Items)
                    {
                        if (tab_player.Id == player.Id)
                        {
                            if (playerTabs.SelectedItem != tab_player) {
                                playerTabs.SelectedItem = tab_player;
                                return true;
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                // Log the error but don't crash the game
                Debug.WriteLine($"Error focusing player tab: {ex.Message}");
            }

            return false;
        }

        private (string message, object[] args) GetShakeMessage()
        {
            // If card is on the table, use the standard message regardless of visibility
            if (_card.Group == null || _card.Group == Program.GameEngine.Table)
            {
                return ("shakes '{0}'", new object[] { _card });
            }

            var groupOwnerName = _card.Group.Owner?.Name ?? "Unknown";
            var groupName = _card.Group.Name;
            
            // Card is not on the table, always specify the group
            // If card is visible, show the card name in the group
            if (_card.Name != "Card")
            {
                return ("shakes '{0}' in {1}'s {2}", new object[] { _card.Name, groupOwnerName, groupName });
            }

            // Card is not visible, determine appropriate message based on group type
            // Check if the group is a pile
            if (_card.Group is Pile pile)
            {
                // Check if it's collapsed (PileCollapsedControl)
                if (pile.ViewState == GroupViewState.Collapsed)
                {
                    return ("shook {0}'s {1}", new object[] { groupOwnerName, groupName });
                }
                
                // Check if it's a pile without fanning (PileControl without fanning)
                // For GroupViewState.Pile, we assume it's not fanned
                if (pile.ViewState == GroupViewState.Pile)
                {
                    return ("shook {0}'s {1}", new object[] { groupOwnerName, groupName });
                }
            }

            // For other cases (expanded piles, hands, etc.), use "in <groupname>" format
            return ("shook a card in {0}'s {1}", new object[] { groupOwnerName, groupName });
        }
    }
}