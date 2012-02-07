using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Octgn.Play.Gui
{
    public class DragAdorner : Adorner
    {
        public DragAdorner(UIElement adorned)
            : base(adorned)
        {
            VisualBrush brush = new VisualBrush(adorned);
            // HACK: this makes the markers work properly, 
            // even after adding 4+ markers and then removing to 3-,
            // and then shift-dragging (in which case the size of the adorner is correct, 
            // but the VisualBrush tries to render the now invisible number.
            brush.Stretch = Stretch.None; brush.AlignmentX = AlignmentX.Left;
            child = new Rectangle();
            child.BeginInit();
            child.Width = adorned.RenderSize.Width;
            child.Height = adorned.RenderSize.Height;
            child.Fill = brush;
            child.IsHitTestVisible = false;
            child.EndInit();

            DoubleAnimation animation = new DoubleAnimation(0.6, 0.85, new Duration(TimeSpan.FromMilliseconds(500)));
            animation.AutoReverse = true;
            animation.RepeatBehavior = RepeatBehavior.Forever;
            animation.Freeze();

            brush.BeginAnimation(Brush.OpacityProperty, animation);
        }

        #region Content Management

        private Rectangle child;

        protected override Size MeasureOverride(Size constraint)
        {
            child.Measure(constraint);
            return child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        { return child; }

        protected override int VisualChildrenCount
        { get { return 1; } }

        #endregion

        #region Position

        private double leftOffset, topOffset;

        public double LeftOffset
        {
            get { return leftOffset; }
            set
            {
                leftOffset = value;
                UpdatePosition();
            }
        }

        public double TopOffset
        {
            get { return topOffset; }
            set
            {
                topOffset = value;
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            AdornerLayer layer = (AdornerLayer)Parent;
            if (layer != null) layer.Update(AdornedElement);
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(new TranslateTransform(leftOffset, topOffset));
            result.Children.Add(base.GetDesiredTransform(transform));
            return result;
        }

        #endregion
    }
}