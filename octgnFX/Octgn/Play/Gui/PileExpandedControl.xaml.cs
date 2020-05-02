using Octgn.DataNew.Entities;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Octgn.Play.Gui
{
    partial class PileExpandedControl
    {
        private FanPanel _fanPanel;

        public PileExpandedControl()
        {
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            InitializeComponent();
        }

        internal override ItemContainerGenerator GetItemContainerGenerator()
        {
            return list.ItemContainerGenerator;
        }

        protected override void GroupChanged()
        {
            base.GroupChanged();
            var pile = (Pile)group;
            if (!pile.AnimateInsertion) return;
            pile.AnimateInsertion = false;
            var doubleAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
            { EasingFunction = new ExponentialEase(), FillBehavior = FillBehavior.Stop };
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
        }


        private void CollapseClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            IsHitTestVisible = false;

            // Fix: capture the pile, it may sometimes be null when Completed executes.
            var capturedPile = (Pile)group;
            var doubleAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300))
            { EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseIn } };
            doubleAnimation.Completed += delegate
            {
                capturedPile.AnimateInsertion = true;
                capturedPile.ViewState = GroupViewState.Pile;
            };
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
        }

        protected override void OnCardOver(object sender, CardsEventArgs e)
        {
            base.OnCardOver(sender, e);
            for(var i = 0;i<e.Cards.Length;i++)
            {
                e.CardSizes[i] = new Size(e.Cards[i].RealWidth * 100 / e.Cards[i].RealHeight, 100);
            }
            //e.CardSize = new Size(100 * Program.GameEngine.Definition.DefaultSize.Width / Program.GameEngine.Definition.DefaultSize.Height, 100);
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
            var cards = e.Cards.ToArray();

            Card.MoveCardsTo(@group, cards, (args) =>
            {
                var c = args.Card;
                args.Index = idx;
                bool doNotIncrement = (c.Group == @group && @group.GetCardIndex(c) < idx);
                c.MoveTo(@group, e.FaceUp != null && e.FaceUp.Value, idx, false);
                // Fix: some cards (notably copies like token) may be deleted when they change group
                // in those case we should increment idx, otherwise an IndexOutOfRange exception may occur
                if (c.Group != @group)
                    doNotIncrement = true;
                if (!doNotIncrement) idx++;

            }, false);
        }

        private void SaveFanPanel(object sender, RoutedEventArgs e)
        {
            _fanPanel = (FanPanel)sender;
            DensitySlider.Value = _fanPanel.HandDensity * 100;
        }

        private void DensitySlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _fanPanel.HandDensity = (sender as Slider).Value / 100;
            _fanPanel.InvalidateMeasure();
            this.InvalidateArrange();
        }
    }
}
