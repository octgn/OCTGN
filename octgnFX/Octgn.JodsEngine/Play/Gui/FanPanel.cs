using System;
using System.Linq;
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
        private const double LayoutAnimationDuration = .3;
        private System.Collections.Generic.Dictionary<int, double> cardLocations = new System.Collections.Generic.Dictionary<int, double>
                                                                                        {  
                                                                                            { 0, 0 } 
                                                                                        };
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

        private double fanWidth = 0;
        private InsertAdorner _insertAdorner;
        private UIElement _mouseOverElement;
        private UIElement _spacedItem1, _spacedItem2;

        #endregion

        public double HandDensity { get; set; } = 0;

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
            if (Children == null || Children.Count == 0) return 0;
            int idx = cardLocations.OrderBy(y => Math.Abs(y.Value - position.X)).First().Key;
            //if (idx > Children.Count) idx = Children.Count;
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
            _insertAdorner.MoveTo(new Point(cardLocations[idx], 0));

            // Cancel previous spacing
            CancelSpacing();

            // Space neighbors
            if (idx < Children.Count)
            {
                _spacedItem2 = Children[idx];
                SetSpacing(_spacedItem2, -SpacingWidth);
            }
            if (idx > 0)
            {
                _spacedItem1 = Children[idx - 1];
                SetSpacing(_spacedItem1, SpacingWidth);
            }
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
            var idealSize = new Size();

            if (Children == null)
                return idealSize;

            // Set Height first if constrained
            if (availableSize.Height != double.PositiveInfinity)
            {
                idealSize.Height = availableSize.Height;
            }

            fanWidth = 0;
            for(int i = Children.Count-1; i >= 0; i--)
            {
                var child = Children[i];

                child.Measure(availableSize);
                if (fanWidth + child.DesiredSize.Width > idealSize.Width)
                {
                    idealSize.Width = fanWidth + child.DesiredSize.Width;
                }
                fanWidth += (child.DesiredSize.Width * HandDensity);
            }
            return idealSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children == null || Children.Count == 0)
                return new Size();

            cardLocations = new System.Collections.Generic.Dictionary<int, double>();

            double scaleHand = 1;
            if (finalSize.Width > this.DesiredSize.Width && HandDensity != 1) // don't need to do anything if no extra space or not fanning
            {
                double handPadding = this.DesiredSize.Width - fanWidth;      // the space reserved after the fanning of the cards to prevent clipping
                double paddingCard = handPadding / (1 - HandDensity);        // the full width of the card that the right side is padded for
                double adjustedFan = fanWidth - (paddingCard - handPadding); // the width of the fan minus the portion aloted for above card
                scaleHand = (finalSize.Width - paddingCard) / adjustedFan;   // how much to scale the hand density to fill the space
            }
            double percentToShow = Math.Min(scaleHand * (HandDensity), 1d); // show a maximum of 100% of cards

            double xposition = 0;
            double animationDelay = 0;
            double perChildDelay = LayoutAnimationDuration / Children.Count;
            UIElement newMouseOverElement = null;

            for (int i = Children.Count-1; i >= 0; i--)
            {
                var child = Children[i];

                Canvas.SetZIndex(child, -i);
                var group = (TransformGroup) child.RenderTransform;
                var scale = (ScaleTransform) group.Children[0];
                var translate = (TranslateTransform) group.Children[1];

                if (child.IsMouseOver && HandDensity > 0)
                {
                    newMouseOverElement = child;
                    if (child != _mouseOverElement)
                    {
                        Animation.To = 1.3;
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

                    StaticAnimation.To = StaticAnimation.From = Animation.From;
                    StaticAnimation.Duration = delay;
                    translate2.BeginAnimation(TranslateTransform.XProperty, StaticAnimation);

                    Animation.FillBehavior = FillBehavior.Stop;
                    Animation.From = oldPos - xposition;
                    Animation.To = 0;
                    Animation.BeginTime = delay;
                    translate2.BeginAnimation(TranslateTransform.XProperty, Animation, HandoffBehavior.Compose);

                    animationDelay += perChildDelay;
                }
                SetXPosition(child, xposition);

                if (HandDensity > 0)
                {
                    cardLocations.Add(i + 1, xposition);
                    xposition += (child.DesiredSize.Width * percentToShow);
                }
            }
            cardLocations.Add(0, xposition + Children[0].DesiredSize.Width - (Children[0].DesiredSize.Width * percentToShow));

            _mouseOverElement = newMouseOverElement;

            Animation.From = null;
            Animation.BeginTime = TimeSpan.Zero;

            return finalSize;
        }

        #endregion
    }
}