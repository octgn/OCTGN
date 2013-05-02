using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Octgn.Play.Actions;
using Octgn.Play.Gui.Adorners;

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
            if (visualAdded == null) return;
            var child = (ContentPresenter) visualAdded;
            if (((Card) child.DataContext).Controller == Player.LocalPlayer) return;
            var scale = new ScaleTransform();
            child.RenderTransformOrigin = new Point(0.5, 0.5);
            child.RenderTransform = scale;
            var anim = new DoubleAnimation
                           {
                               Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                               AutoReverse = true,
                               RepeatBehavior = new RepeatBehavior(2.166),
                               AccelerationRatio = 0.2,
                               DecelerationRatio = 0.7,
                               To = 1.2,
                               From = 0.9,
                               FillBehavior = FillBehavior.Stop
                           };
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
        }

        private void CardMoving(object sender, EventArgs e)
        {
            var action = (MoveCard) sender;
            Table table = Program.GameEngine.Table;
            if (action.Who == Player.LocalPlayer || action.To != table || action.From != table)
                return;

            AnimateMove(action.Card, action.X, action.Y);
        }

        private void AnimateMove(Card card, double x, double y)
        {
            foreach (ContentPresenter child in Children)
                if (child.DataContext == card)
                {
                    var duration = new Duration(TimeSpan.FromMilliseconds(500));
                    var anim = new DoubleAnimation
                                   {
                                       AccelerationRatio = 0.2,
                                       DecelerationRatio = 0.7,
                                       Duration = duration,
                                       From = GetLeft(child),
                                       To = x,
                                       FillBehavior = FillBehavior.Stop
                                   };
                    var anim2 = new DoubleAnimation
                                    {
                                        AccelerationRatio = 0.2,
                                        DecelerationRatio = 0.7,
                                        Duration = duration,
                                        From = GetTop(child),
                                        To = y,
                                        FillBehavior = FillBehavior.Stop
                                    };
                    child.BeginAnimation(LeftProperty, anim, HandoffBehavior.SnapshotAndReplace);
                    child.BeginAnimation(TopProperty, anim2, HandoffBehavior.SnapshotAndReplace);
                    break;
                }
        }

        private void Targetting(object sender, EventArgs e)
        {
            var targetAction = (Target) sender;
            CardControl fromCard = null, toCard = null;

            foreach (ContentPresenter child in Children)
            {
                if (targetAction.FromCard == child.DataContext)
                {
                    fromCard = VisualTreeHelper.GetChild(child, 0) as CardControl;
                    if (toCard != null) break;
                }
                else if (targetAction.ToCard == child.DataContext)
                {
                    toCard = VisualTreeHelper.GetChild(child, 0) as CardControl;
                    if (fromCard != null) break;
                }
            }

            if (fromCard == null || toCard == null) return;
            fromCard.CreateArrowTo(targetAction.Who, toCard);
            targetAction.FromCard.TargetsOtherCards = true;
        }

        private void Untargetting(object sender, EventArgs e)
        {
            var targetAction = (Target) sender;
            CardControl card = (from ContentPresenter child in Children
                                where child.DataContext == targetAction.FromCard
                                select VisualTreeHelper.GetChild(child, 0) as CardControl).FirstOrDefault();
            if (card == null) return; // Opponent moved the card out of the table concurently

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(card);
            Adorner[] adorners = layer.GetAdorners(card);
            if (adorners == null) return; // Opponent removed the target card out of the table concurently
            foreach (ArrowAdorner arrow in adorners.OfType<ArrowAdorner>())
            {
                layer.Remove(arrow);
            }

            targetAction.FromCard.TargetsOtherCards = false;
        }
    }
}