using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Octgn.Play.Gui
{
    partial class PileControl
    {
        private static readonly DoubleAnimation Anim = new DoubleAnimation
                                                           {
                                                               Duration = new Duration(TimeSpan.FromMilliseconds(150)),
                                                               AccelerationRatio = 0.3,
                                                               DecelerationRatio = 0.3,
                                                               FillBehavior = FillBehavior.HoldEnd
                                                           };

        public PileControl()
        {
            InitializeComponent();
            bottomZone.AddHandler(CardControl.CardOverEvent, new CardsEventHandler(OnCardOverBottom));
            bottomZone.AddHandler(CardControl.CardDroppedEvent, new CardsEventHandler(OnCardDroppedBottom));
        }

        protected override void GroupChanged()
        {
            base.GroupChanged();
            grid.ColumnDefinitions[0].Width = new GridLength(100*group.Def.Width/group.Def.Height);
            var pile = (Pile) group;
            if (!pile.AnimateInsertion) return;
            pile.AnimateInsertion = false;
            var doubleAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
                                      {EasingFunction = new ExponentialEase(), FillBehavior = FillBehavior.Stop};
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
        }

        private void CollapseClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            IsHitTestVisible = false;

            // Fix: capture the pile, it may sometimes be null when Completed executes.
            var capturedPile = (Pile) group;
            var doubleAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300))
                                      {EasingFunction = new ExponentialEase {EasingMode = EasingMode.EaseIn}};
            doubleAnimation.Completed += delegate
                                             {
                                                 capturedPile.AnimateInsertion = true;
                                                 capturedPile.Collapsed = true;
                                             };
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
        }

        public override void ExecuteDefaultAction(Card card)
        {
            if (!ExecuteDefaultGroupAction()) ExecuteDefaultCardAction(card);
        }

        #region Card DnD

        protected override void OnCardOver(object sender, CardsEventArgs e)
        {
            base.OnCardOver(sender, e);
            e.CardSize = new Size(cardsCtrl.ActualWidth, cardsCtrl.ActualHeight);

            if (bottomZone.Visibility != Visibility.Visible)
            {
                bottomZone.Visibility = Visibility.Visible;
                Anim.From = 0;
                Anim.To = 0.4;
                bottomZone.BeginAnimation(OpacityProperty, Anim);
                Anim.From = null;
            }
            else
            {
                Anim.To = 0.4;
                bottomZone.BeginAnimation(OpacityProperty, Anim);
            }
        }

        protected override void OnCardOut(object sender, CardsEventArgs e)
        {
            bottomZone.Visibility = Visibility.Collapsed;
        }

        protected override void OnCardDropped(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            if (group.TryToManipulate())
                foreach (Card c in e.Cards)
                    c.MoveTo(group, e.FaceUp != null && e.FaceUp.Value, 0,false);
        }

        private void OnCardDroppedBottom(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            if (group.TryToManipulate())
                foreach (Card c in e.Cards)
                    c.MoveTo(group, e.FaceUp != null && e.FaceUp.Value, group.Count,false);
        }

        private void OnCardOverBottom(object sender, CardsEventArgs e)
        {
            OnCardOver(sender, e);
            Anim.To = 0.8;
            bottomZone.BeginAnimation(OpacityProperty, Anim);
        }

        #endregion
    }
}