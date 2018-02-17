using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Octgn.DataNew.Entities;

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
            if (!(double.IsNaN(cardsCtrl.Width) || double.IsNaN(cardsCtrl.Height)))
            {
                grid.ColumnDefinitions[0].Width = new GridLength(cardsCtrl.Width);
            }
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
                                                 capturedPile.ViewState = GroupViewState.Collapsed;
                                             };
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
        }

        private void ExpandClicked(object sender, RoutedEventArgs e)
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
                capturedPile.ViewState = GroupViewState.Expanded;
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
			for(var i = 0;i<e.Cards.Length;i++)
            {
                e.CardSizes[i] = new Size(cardsCtrl.ActualWidth, cardsCtrl.ActualHeight);
                if (cardsCtrl.ActualWidth > cardsCtrl.ActualHeight)
                {
                    e.CardSizes[i] = new Size(e.Cards[i].RealWidth*cardsCtrl.ActualHeight/e.Cards[i].RealHeight, cardsCtrl.ActualHeight);
                }
                else
                {
                    e.CardSizes[i] = new Size(cardsCtrl.ActualWidth, e.Cards[i].RealHeight * cardsCtrl.ActualWidth / e.Cards[i].RealWidth);
                }
            }

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
            //if (group.TryToManipulate())
            //    foreach (Card c in e.Cards)
            //        c.MoveTo(group, e.FaceUp != null && e.FaceUp.Value, 0,false);
            if (group.TryToManipulate())
            {
                var cards = e.Cards.ToArray();
                Card.MoveCardsTo(group, cards, 
                    Enumerable.Repeat(e.FaceUp ?? false,cards.Length).ToArray()
                    ,Enumerable.Repeat(0,cards.Length).ToArray(),false);
            }
        }

        private void OnCardDroppedBottom(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            //if (group.TryToManipulate())
            //    foreach (Card c in e.Cards)
            //        c.MoveTo(group, e.FaceUp != null && e.FaceUp.Value, group.Count,false);
            if (group.TryToManipulate())
            {
                var cards = e.Cards.ToArray();
                Card.MoveCardsTo(group, cards, 
                    Enumerable.Repeat(e.FaceUp ?? false,cards.Length).ToArray()
                    ,Enumerable.Range(group.Count,cards.Length).ToArray(),false);
            }
        }

        private void OnCardOverBottom(object sender, CardsEventArgs e)
        {
            OnCardOver(sender, e);
            Anim.To = 0.8;
            bottomZone.BeginAnimation(OpacityProperty, Anim);
        }

        #endregion

        private void cardsCtrl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Hack: animate the first card into a pile, 
            // otherwise the CardControl sometimes has issues displaying anything.
            // for some reason...
            if (e.OldValue == null)
            {
                var anim = new DoubleAnimation(1.1, 1, new Duration(TimeSpan.FromMilliseconds(150)), FillBehavior.Stop);
                cardsCtrl.turn.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                cardsCtrl.turn.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            }
        }
    }
}