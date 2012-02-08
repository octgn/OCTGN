using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimplestDragDrop
{
    internal class DragAdorner : Adorner
    {
        protected double XCenter;
        protected double YCenter;
        protected UIElement _child;
        private double _leftOffset;
        protected UIElement _owner;
        private double _topOffset;
        public double scale = 1.0;

        public DragAdorner(UIElement owner) : base(owner)
        {
        }

        public DragAdorner(UIElement owner, UIElement adornElement, bool useVisualBrush, double opacity)
            : base(owner)
        {
            Debug.Assert(owner != null);
            Debug.Assert(adornElement != null);
            _owner = owner;
            if (useVisualBrush)
            {
                var _brush = new VisualBrush(adornElement);
                _brush.Opacity = opacity;
                var r = new Rectangle();
                r.RadiusX = 3;
                r.RadiusY = 3;

                //TODO: questioning DesiredSize vs. Actual 
                r.Width = adornElement.DesiredSize.Width;
                r.Height = adornElement.DesiredSize.Height;

                XCenter = adornElement.DesiredSize.Width/2;
                YCenter = adornElement.DesiredSize.Height/2;

                r.Fill = _brush;
                _child = r;
            }
            else
                _child = adornElement;
        }


        public double LeftOffset
        {
            get { return _leftOffset; }
            set
            {
                _leftOffset = value - XCenter;
                UpdatePosition();
            }
        }

        public double TopOffset
        {
            get { return _topOffset; }
            set
            {
                _topOffset = value - YCenter;

                UpdatePosition();
            }
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        private void UpdatePosition()
        {
            var adorner = (AdornerLayer) Parent;
            if (adorner != null)
            {
                adorner.Update(AdornedElement);
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _child;
        }


        protected override Size MeasureOverride(Size finalSize)
        {
            _child.Measure(finalSize);
            return _child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _child.Arrange(new Rect(_child.DesiredSize));
            return finalSize;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();

            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(_leftOffset, _topOffset));
            return result;
        }
    }
}