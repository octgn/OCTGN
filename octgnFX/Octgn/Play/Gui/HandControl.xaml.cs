using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Octgn.Play.Gui
{
    internal partial class HandControl
    {
        private FanPanel fanPanel;

        public HandControl()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            InitializeComponent();
        }

        internal override ItemContainerGenerator GetItemContainerGenerator()
        {
            return list.ItemContainerGenerator;
        }

        protected override void OnCardOver(object sender, CardsEventArgs e)
        {
            base.OnCardOver(sender, e);
            e.CardSize =
                new Size(
                    100*Program.Game.Definition.CardDefinition.Width/Program.Game.Definition.CardDefinition.Height, 100);
            fanPanel.DisplayInsertIndicator(e.ClickedCard, fanPanel.GetIndexFromPoint(Mouse.GetPosition(fanPanel)));
        }

        protected override void OnCardOut(object sender, CardsEventArgs e)
        {
            e.Handled = true;
            fanPanel.HideInsertIndicator();
        }

        protected override void OnCardDropped(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            if (!@group.TryToManipulate()) return;
            int idx = fanPanel.GetIndexFromPoint(Mouse.GetPosition(fanPanel));
            foreach (Card c in e.Cards)
            {
                bool doNotIncrement = (c.Group == @group && @group.GetCardIndex(c) < idx);
                c.MoveTo(@group, e.FaceUp != null && e.FaceUp.Value, idx);
                // Fix: some cards (notably copies like token) may be deleted when they change group
                // in those case we should increment idx, otherwise an IndexOutOfRange exception may occur
                if (c.Group != @group)
                    doNotIncrement = true;
                if (!doNotIncrement) idx++;
            }
        }

        private void SaveFanPanel(object sender, RoutedEventArgs e)
        {
            fanPanel = (FanPanel) sender;
        }
    }
}