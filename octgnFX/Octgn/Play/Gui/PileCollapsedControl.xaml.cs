using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Octgn.Play.Gui
{
    public partial class PileCollapsedControl
    {
        public PileCollapsedControl()
        {
            InitializeComponent();
        }

        protected override void GroupChanged()
        {
            base.GroupChanged();
            var pile = (Pile) group;
            if (!pile.AnimateInsertion) return;
            pile.AnimateInsertion = false;
            var anim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300))
                           {EasingFunction = new ExponentialEase(), FillBehavior = FillBehavior.Stop};
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
            IsHitTestVisible = false;

            // Fix: capture the group, because it's sometimes null inside the Completed event.
            var oldPile = (Pile) group;
            // Fix: group may even be null at this point (?!). 
            // Apparently when clicking on a pile just after another one has been removed. That would be just before the GroupChanged happens?
            if (oldPile == null) return;

            var anim = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300))
                           {EasingFunction = new ExponentialEase {EasingMode = EasingMode.EaseIn}};
            anim.Completed += delegate
                                  {
                                      oldPile.AnimateInsertion = true;
                                      oldPile.Collapsed = false;
                                  };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        protected override void OnCardOver(object sender, CardsEventArgs e)
        {
            base.OnCardOver(sender, e);
            e.CardSize = new Size(30*group.Def.Width/group.Def.Height, 30);
        }

        protected override void OnCardDropped(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            if (group.TryToManipulate())
                foreach (Card c in e.Cards)
                    c.MoveTo(group, e.FaceUp != null && e.FaceUp.Value, 0,false);
        }
    }
}