using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Octgn.Play.Gui
{
    partial class HandControl
    {
        private FanPanel _fanPanel;

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
                    100*Program.GameEngine.Definition.CardWidth/Program.GameEngine.Definition.CardHeight, 100);
            _fanPanel.DisplayInsertIndicator(e.ClickedCard, _fanPanel.GetIndexFromPoint(Mouse.GetPosition(_fanPanel)));
        }

        protected override void OnCardOut(object sender, CardsEventArgs e)
        {
            e.Handled = true;
            _fanPanel.HideInsertIndicator();
        }

        protected override void OnCardDropped(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            if (!@group.TryToManipulate()) return;
            int idx = _fanPanel.GetIndexFromPoint(Mouse.GetPosition(_fanPanel));
            foreach (Card c in e.Cards)
            {
                bool doNotIncrement = (c.Group == @group && @group.GetCardIndex(c) < idx);
                c.MoveTo(@group, e.FaceUp != null && e.FaceUp.Value, idx,false);
                // Fix: some cards (notably copies like token) may be deleted when they change group
                // in those case we should increment idx, otherwise an IndexOutOfRange exception may occur
                if (c.Group != @group)
                    doNotIncrement = true;
                if (!doNotIncrement) idx++;
            }
        }

        private void SaveFanPanel(object sender, RoutedEventArgs e)
        {
            _fanPanel = (FanPanel) sender;
        }
    }
}