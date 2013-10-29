using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Octgn.Play.Gui.Adorners
{
    public class DragAdorner : Adorner
    {
        public DragAdorner(UIElement adorned)
            : base(adorned)
        {
            var brush = new VisualBrush(adorned) {Stretch = Stretch.None, AlignmentX = AlignmentX.Left};
            // HACK: this makes the markers work properly, 
            // even after adding 4+ markers and then removing to 3-,
            // and then shift-dragging (in which case the size of the adorner is correct, 
            // but the VisualBrush tries to render the now invisible number.
            _child = new Rectangle();
            _child.BeginInit();
            _child.Width = adorned.RenderSize.Width;
            _child.Height = adorned.RenderSize.Height;
            _child.Fill = brush;
            _child.IsHitTestVisible = false;
            _child.EndInit();

            var animation = new DoubleAnimation(0.6, 0.85, new Duration(TimeSpan.FromMilliseconds(500)))
                                {AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever};
            animation.Freeze();

            brush.BeginAnimation(Brush.OpacityProperty, animation);
        }

        #region Content Management

        private readonly Rectangle _child;

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _child.Measure(constraint);
            return _child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }

        #endregion

        #region Position

        private double _leftOffset, _topOffset;

        public double LeftOffset
        {
            get { return _leftOffset; }
            set
            {
                _leftOffset = value;
                UpdatePosition();
            }
        }

        public double TopOffset
        {
            get { return _topOffset; }
            set
            {
                _topOffset = value;
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            var layer = (AdornerLayer) Parent;
            if (layer != null) layer.Update(AdornedElement);
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();
            result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
            GeneralTransform gdt = base.GetDesiredTransform(transform);
            if (gdt != null)
                result.Children.Add(gdt);
            return result;
        }

        #endregion
    }
}