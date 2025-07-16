using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;

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

            Program.GameMess.PlayerEvent(_who, "shakes '{0}'", _card);
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
    }
}