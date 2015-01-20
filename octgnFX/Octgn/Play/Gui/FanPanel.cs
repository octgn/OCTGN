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
        private const double LayoutAnimationDelay = 0.1;
        private System.Collections.Generic.Dictionary<int, double> cardLocations;
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
        public double handDensity
        {
            get { return Octgn.Core.Prefs.HandDensity / 100; }
        }

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
            if (Children == null || Children.Count == 0) return 0;
            int idx = cardLocations.First(x => x.Value == cardLocations.Values.OrderBy(y => Math.Abs(y - position.X)).First()).Key;
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
            _insertAdorner.MoveTo(new Point(cardLocations[idx], 0));

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
            fanWidth = 0;

            // Allow children as much room as they want - then scale them
            var size = new Size(Double.PositiveInfinity, Double.PositiveInfinity);
            foreach (UIElement child in Children)
            {
                child.Measure(size);
                if (fanWidth + child.DesiredSize.Width > idealSize.Width)
                {
                    idealSize.Width = fanWidth + child.DesiredSize.Width;
                }
                fanWidth += (child.DesiredSize.Width * handDensity);

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
            var size = new Size(Double.PositiveInfinity, Double.PositiveInfinity);

            double totalChildWidth = 0;

            foreach (UIElement child in Children)
            {
                child.Measure(size);
                child.Arrange(new Rect(0, 0, child.DesiredSize.Width, child.DesiredSize.Height));
                totalChildWidth += child.DesiredSize.Width;
            }

            if (Math.Abs(Children[0].DesiredSize.Height - 0) < double.Epsilon) return finalSize; //TODO remove reliance on first card, not sure what this catches
            double ratio = finalSize.Height / Children[0].DesiredSize.Height;                    //TODO "              "; figure out if the ratio is ever not 1

            // starts from min. fanning from settings, fill out extra space if available
            this.InvalidateMeasure(); // fixes issues with changing the hand density mid-game, seems to casue some odd visual effects though
            this.Measure(size); // Have to re-measure after ^
            double scaleHand = 1;
            if (finalSize.Width > this.DesiredSize.Width && handDensity != 1) // don't need to do anything if no extra space or not fanning
            {
                double handPadding = this.DesiredSize.Width - fanWidth; // the space reserved after the fanning of the cards to prevent clipping
                double adjustedPadding = handPadding / (1 - handDensity); // the full width of the card that the right side is padded for
                double adjustedFan = fanWidth - (adjustedPadding - handPadding); // the width of the fan minus the portion aloted for above card
                scaleHand = (finalSize.Width - adjustedPadding) / adjustedFan; // how much to scale the hand density to fill the space
            }
            double percentToShow = Math.Min(scaleHand * (handDensity), 1d); // show a maximum of 100% of cards

            cardLocations = new System.Collections.Generic.Dictionary<int, double>(); // for indexing the positions of the cards
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

                int idx = Children.IndexOf(child);
                cardLocations.Add(idx, xposition);
                if (cardLocations.Count == Children.Count)
                    cardLocations.Add(idx + 1, xposition + child.DesiredSize.Width); // add index for dragging to end of hand
                xposition += (child.DesiredSize.Width * percentToShow); // I have no idea where the padding between cards is comming from
                
            }

            _mouseOverElement = newMouseOverElement;
            return finalSize;
        }

        #endregion
    }
}