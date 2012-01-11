using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Octgn.Play.Actions;
using System.Windows.Documents;

namespace Octgn.Play.Gui
{
    public class TableCanvas : Canvas
    {
        public TableCanvas()
        {
            MoveCard.Doing += CardMoving;
            Target.CreatingArrow += Targetting;
            Target.DeletingArrows += Untargetting;
            Unloaded += delegate 
            { 
                MoveCard.Doing -= CardMoving;
                Target.CreatingArrow -= Targetting;
                Target.DeletingArrows -= Untargetting;
            };   
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (visualAdded != null)
            {
                var child = (ContentPresenter)visualAdded;
                if (((Card)child.DataContext).Controller != Player.LocalPlayer)
                {
                    var scale = new ScaleTransform();
                    child.RenderTransformOrigin = new Point(0.5, 0.5);
                    child.RenderTransform = scale;                    
                    var anim = new DoubleAnimation()
                    {
                        Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                        AutoReverse = true,
                        RepeatBehavior = new RepeatBehavior(2.166),
                        AccelerationRatio = 0.2,
                        DecelerationRatio = 0.7,
                        To = 1.2, From = 0.9,
                        FillBehavior = FillBehavior.Stop
                    };
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
                }
            }
        }

        private void CardMoving(object sender, EventArgs e)
        {
            MoveCard action = (MoveCard)sender;
            var table = Program.Game.Table;
            if (action.who == Player.LocalPlayer || action.to != table || action.from != table)
                return;

            AnimateMove(action.card, action.x, action.y);
        }

        private void AnimateMove(Card card, double x, double y)
        {
            foreach (ContentPresenter child in Children)
                if (child.DataContext == card)
                {
                    var duration = new Duration(TimeSpan.FromMilliseconds(500));
                    var anim = new DoubleAnimation()
                    {
                        AccelerationRatio = 0.2, DecelerationRatio = 0.7, Duration = duration,
                        From = Canvas.GetLeft(child), To = x, FillBehavior = FillBehavior.Stop
                    };                    
                    var anim2 = new DoubleAnimation()
                    {
                       AccelerationRatio = 0.2, DecelerationRatio = 0.7, Duration = duration,
                       From = Canvas.GetTop(child), To = y, FillBehavior = FillBehavior.Stop
                    };
                    child.BeginAnimation(Canvas.LeftProperty, anim, HandoffBehavior.SnapshotAndReplace);
                    child.BeginAnimation(Canvas.TopProperty, anim2, HandoffBehavior.SnapshotAndReplace);
                    break;
                }
        }

        private void Targetting(object sender, EventArgs e)
        {
            var targetAction = (Target)sender;
            CardControl fromCard = null, toCard = null;

            foreach (ContentPresenter child in Children)
            {
                if (targetAction.fromCard == child.DataContext)
                {
                    fromCard = VisualTreeHelper.GetChild(child, 0) as CardControl;
                    if (toCard != null) break;
                }
                else if (targetAction.toCard == child.DataContext)
                {
                    toCard = VisualTreeHelper.GetChild(child, 0) as CardControl;
                    if (fromCard != null) break;
                }
            }

            if (fromCard != null && toCard != null)     // Opponent may have moved the card out of the table concurently
            {                
                fromCard.CreateArrowTo(targetAction.who, toCard);
                targetAction.fromCard.TargetsOtherCards = true;
            }
        }

        private void Untargetting(object sender, EventArgs e)
        {
            var targetAction = (Target)sender;
            CardControl card = null;
            foreach (ContentPresenter child in Children)
                if (child.DataContext == targetAction.fromCard)
                {
                    card = VisualTreeHelper.GetChild(child, 0) as CardControl;
                    break;
                }
            if (card == null) return;   // Opponent moved the card out of the table concurently

            var layer = AdornerLayer.GetAdornerLayer(card);
            var adorners = layer.GetAdorners(card);
            if (adorners == null) return;   // Opponent removed the target card out of the table concurently
            foreach (Adorner adorner in adorners)
            {
                var arrow = adorner as ArrowAdorner;
                if (arrow != null) layer.Remove(arrow);
            }

            targetAction.fromCard.TargetsOtherCards = false;
        }
    }
}
