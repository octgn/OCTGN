using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Octgn.Play.Gui.Adorners;

namespace Octgn.Play.Gui
{
    public class FanPanel : Panel
    {
        #region Private vars

        private const int SpacingWidth = 8;
        private const double LayoutAnimationDelay = 0.1;
        private static readonly IEasingFunction ExpoEasing = new ExponentialEase();

        private static readonly DoubleAnimation Animation = new DoubleAnimation
                                                                {
                                                                    EasingFunction = ExpoEasing,
                                                                    Duration = TimeSpan.FromMilliseconds(300)
                                                                };

        private static readonly DoubleAnimation StaticAnimation = new DoubleAnimation
                                                                      {
                                                                          EasingFunction = ExpoEasing,
                                                                          FillBehavior = FillBehavior.Stop
                                                                      };

        private InsertAdorner _insertAdorner;
        private double _itemSkipSize;
        private UIElement _mouseOverElement;
        private UIElement _spacedItem1, _spacedItem2;

        #endregion

        #region Dependency properties

        private static readonly DependencyProperty XPositionProperty =
            DependencyProperty.RegisterAttached("XPositionProperty", typeof (double), typeof (FanPanel));

        private static double GetXPosition(DependencyObject obj)
        {
            return (double) obj.GetValue(XPositionProperty);
        }

        private static void SetXPosition(DependencyObject obj, double value)
        {
            obj.SetValue(XPositionProperty, value);
        }

        #endregion

        public FanPanel()
        {
            //  Wire events
            PreviewMouseMove += FanPanelMouseMove;
            MouseEnter += MouseRequiresArrange;
            MouseLeave += MouseRequiresArrange;
        }

        #region Insertion related

        public int GetIndexFromPoint(Point position)
        {
            if (Math.Abs(_itemSkipSize - 0) < double.Epsilon) return 0;
            int idx = position.X < 0 ? 0 : (int) (position.X/_itemSkipSize + 0.5);
            if (idx > Children.Count) idx = Children.Count;
            return idx;
        }

        public void DisplayInsertIndicator(Card source, int idx)
        {
            // If the index corresponds to the card being dragged, it's a NOP so don't provide any visual feedback
            if (idx < Children.Count)
            {
                if (((FrameworkElement) Children[idx]).DataContext == source)
                {
                    HideInsertIndicator();
                    return;
                }
                if (idx > 0 && ((FrameworkElement) Children[idx - 1]).DataContext == source)
                {
                    HideInsertIndicator();
                    return;
                }
            }

            // Create an adorner if it doesn't exist yet
            if (_insertAdorner == null)
            {
                _insertAdorner = new InsertAdorner(this);
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
                layer.Add(_insertAdorner);
            }

            // Position the insert adorner correctly
            _insertAdorner.MoveTo(new Point(idx*_itemSkipSize, 0));

            // Cancel previous spacing
            CancelSpacing();

            // Space neighbors
            if (idx >= Children.Count) return;
            _spacedItem2 = Children[idx];
            SetSpacing(_spacedItem2, SpacingWidth);
            if (idx <= 0) return;
            _spacedItem1 = Children[idx - 1];
            SetSpacing(_spacedItem1, -SpacingWidth);
        }

        public void HideInsertIndicator()
        {
            if (_insertAdorner == null) return;
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            layer.Remove(_insertAdorner);
            _insertAdorner = null;
            CancelSpacing();
        }

        private void CancelSpacing()
        {
            if (_spacedItem1 != null)
            {
                CancelSpacing(_spacedItem1);
                _spacedItem1 = null;
            }
            if (_spacedItem2 == null) return;
            CancelSpacing(_spacedItem2);
            _spacedItem2 = null;
        }

        private static void CancelSpacing(UIElement element)
        {
            var group = (TransformGroup) element.RenderTransform;
            var translate = (TranslateTransform) group.Children[1];
            Animation.To = 0;
            Animation.FillBehavior = FillBehavior.Stop;
            translate.BeginAnimation(TranslateTransform.XProperty, Animation);
        }

        private static void SetSpacing(UIElement element, int value)
        {
            var group = (TransformGroup) element.RenderTransform;
            var translate = (TranslateTransform) group.Children[1];
            Animation.To = value;
            Animation.FillBehavior = FillBehavior.HoldEnd;
            translate.BeginAnimation(TranslateTransform.XProperty, Animation);
        }

        #endregion

        #region Mouse related

        private void FanPanelMouseMove(object sender, MouseEventArgs e)
        {
            if (_mouseOverElement == null)
            {
                if (!IsMouseDirectlyOver) InvalidateArrange();
            }
            else if (!_mouseOverElement.IsMouseOver) InvalidateArrange();
        }

        private void MouseRequiresArrange(object sender, MouseEventArgs e)
        {
            InvalidateArrange();
        }

        #endregion

        #region Layout

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            // Set up the transformations
            if (visualAdded == null) return;
            var child = (UIElement) visualAdded;
            child.RenderTransformOrigin = new Point(0, 0);
            var group = new TransformGroup();
            child.RenderTransform = @group;
            @group.Children.Add(new ScaleTransform()); // Used for hover effects
            @group.Children.Add(new TranslateTransform()); // Y used for hover effects, X for drag and drop
            @group.Children.Add(new TranslateTransform()); // X used for layout to layout animations
            SetXPosition(child, double.NaN);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var idealSize = new Size(0, 0);

            // Allow children as much room as they want - then scale them
            var size = new Size(Double.PositiveInfinity, Double.PositiveInfinity);
            foreach (UIElement child in Children)
            {
                child.Measure(size);
                idealSize.Width += child.DesiredSize.Width;
                idealSize.Height = Math.Max(idealSize.Height, child.DesiredSize.Height);
            }

            // EID calls us with infinity, but framework doesn't like us to return infinity
            if (double.IsInfinity(availableSize.Height) || double.IsInfinity(availableSize.Width))
                return idealSize;
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children == null || Children.Count == 0)
                return finalSize;

            //double totalChildWidth = 0;

            foreach (UIElement child in Children)
            {
                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));
                //totalChildWidth += child.DesiredSize.Width;
            }

            // Assume all children have the same width
            if (Math.Abs(Children[0].DesiredSize.Height - 0) < double.Epsilon) return finalSize;
            double ratio = finalSize.Height/Children[0].DesiredSize.Height;
            double childWidth = Children[0].DesiredSize.Width*ratio;

            // Check if enough space is available
            _itemSkipSize = childWidth + 2;
            if (Children.Count > 1 && Children.Count*_itemSkipSize > finalSize.Width)
                _itemSkipSize = (finalSize.Width - childWidth)/(Children.Count - 1);

            double xposition = 0;
            double animationDelay = 0;
            UIElement newMouseOverElement = null;

            foreach (UIElement child in Children)
            {
                var group = (TransformGroup) child.RenderTransform;
                var scale = (ScaleTransform) group.Children[0];
                var translate = (TranslateTransform) group.Children[1];
                scale.ScaleX = scale.ScaleY = ratio;
                if (child.IsMouseOver)
                {
                    newMouseOverElement = child;
                    if (child != _mouseOverElement)
                    {
                        Animation.To = 1.3*ratio;
                        Animation.FillBehavior = FillBehavior.HoldEnd;
                        scale.BeginAnimation(ScaleTransform.ScaleXProperty, Animation);
                        scale.BeginAnimation(ScaleTransform.ScaleYProperty, Animation);
                        Animation.To = -0.4*finalSize.Height;
                        translate.BeginAnimation(TranslateTransform.YProperty, Animation);
                    }
                }
                else
                {
                    if (child == _mouseOverElement)
                    {
                        Animation.To = 1;
                        Animation.FillBehavior = FillBehavior.Stop;
                        scale.BeginAnimation(ScaleTransform.ScaleXProperty, Animation);
                        scale.BeginAnimation(ScaleTransform.ScaleYProperty, Animation);
                        Animation.To = 0;
                        translate.BeginAnimation(TranslateTransform.YProperty, Animation);
                    }
                }

                child.Arrange(new Rect(new Point(xposition, 0), child.DesiredSize));

                // Perform layout to layout animation                
                double oldPos = GetXPosition(child);
                if (!double.IsNaN(oldPos) && Math.Abs(xposition - oldPos) > 2)
                {
                    var translate2 = (TranslateTransform) group.Children[2];
                    TimeSpan delay = TimeSpan.FromSeconds(animationDelay);
                    Animation.FillBehavior = FillBehavior.Stop;
                    Animation.From = oldPos - xposition;
                    Animation.To = 0;
                    Animation.BeginTime = delay;
                    StaticAnimation.To = StaticAnimation.From = Animation.From;
                    StaticAnimation.Duration = delay;
                    if (animationDelay > 0)
                        translate2.BeginAnimation(TranslateTransform.XProperty, StaticAnimation);
                    translate2.BeginAnimation(TranslateTransform.XProperty, Animation, HandoffBehavior.Compose);
                    Animation.From = null;
                    Animation.BeginTime = TimeSpan.Zero;
                    animationDelay += LayoutAnimationDelay;
                }
                SetXPosition(child, xposition);

                xposition += _itemSkipSize;
            }

            _mouseOverElement = newMouseOverElement;
            return finalSize;
        }

        #endregion
    }
}