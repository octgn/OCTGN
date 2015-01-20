using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Octgn.Controls;
using Octgn.Play.Gui.Adorners;

namespace Octgn.Play.Gui
{
    public class WrapPanel : AnimatedWrapPanel
    {
        #region Private vars

        private const int SpacingWidth = 8;

        private static readonly DoubleAnimation Animation = new DoubleAnimation
                                                                {
                                                                    DecelerationRatio = 0.7,
                                                                    Duration =
                                                                        new Duration(TimeSpan.FromMilliseconds(300))
                                                                };

        private InsertAdorner _insertAdorner;

        private UIElement _spacedItem1, _spacedItem2;

        #endregion

        public Visual AdornerLayerVisual { get; set; }
        public FrameworkElement ClippingVisual { get; set; }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (visualAdded == null) return;
            var child = (UIElement) visualAdded;
            var group = new TransformGroup();
            child.RenderTransform = @group;
            @group.Children.Add(new TranslateTransform()); // used for drag and drop effects
            @group.Children.Add(Transform.Identity); // slot reserved for layout to layout animations (in base class)
        }

        #region Insertion related

        public int GetIndexFromPoint(Card card, Point position)
        {
            if (this.VisualChildrenCount == 0) return 0;
            var curChild = 0;
			// Assuming all items are same height. Otherwise shit would be wild as FUUUUCK.
            var firstElem = (FrameworkElement) this.GetVisualChild(0);
            var defaultHeight = firstElem.ActualHeight + firstElem.Margin.Top + firstElem.Margin.Bottom;
            //curY = defaultHeight;// / 2;
            //curX = (firstElem.ActualWidth + firstElem.Margin.Left + firstElem.Margin.Right);///2;
            var curXTrigger = 0d;
            var curX = 0d;
            var curY = defaultHeight;
            for (; curChild < this.VisualChildrenCount; curChild++)
            {
                var cur = (FrameworkElement)this.GetVisualChild(curChild);
				var addWidth = cur.ActualWidth + cur.Margin.Left + cur.Margin.Right;
                curX += addWidth;
                if (curX > this.ActualWidth)
                {
                    curX = cur.ActualWidth + cur.Margin.Left + cur.Margin.Right;
                    if (position.Y <= curY)
                    {
                        //.curChild--;
                        break;
                    }
                    curY += defaultHeight;
                }
                curXTrigger = curX - (addWidth/2);

                if (position.Y <= curY && position.X <= curXTrigger) break;
            }

            return curChild;
        }

        public void DisplayInsertIndicator(Card source, int idx)
        {
            // If the index corresponds to the card being dragged, it's a NOP so don't provide any visual feedback
            if (idx < VisualChildrenCount)
            {
                var frameworkElement = (FrameworkElement) GetVisualChild(idx);
                if (frameworkElement != null && frameworkElement.DataContext == source)
                {
                    HideInsertIndicator();
                    return;
                }
                var visualChild = (FrameworkElement)GetVisualChild(idx);
                if (visualChild != null && (idx > 0 && visualChild.DataContext == source))
                {
                    HideInsertIndicator();
                    return;
                }
            }

            // Create an adorner if none exists yet
            if (_insertAdorner == null)
            {
                _insertAdorner = new InsertAdorner(this) {Height = 106, ClippingVisual = ClippingVisual};
                // HACK: currently WarpPanel is only used by the group window, but card height should be a property and not hard-coded like that
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(AdornerLayerVisual ?? this);
                layer.Add(_insertAdorner);
            }

            // Assuming all items are same height. Otherwise shit would be wild as FUUUUCK.
            var curY = 0d;
            var curX = 0d;
            if (this.VisualChildrenCount > 0)
            {
                var firstElem = (FrameworkElement) this.GetVisualChild(0);
                var defaultHeight = firstElem.ActualHeight + firstElem.Margin.Top + firstElem.Margin.Bottom;
                for (var i = 0; i < idx; i++)
                {
                    var cur = (FrameworkElement) this.GetVisualChild(i);
                    var addWidth = cur.ActualWidth + cur.Margin.Left + cur.Margin.Right;
                    curX += addWidth;
                    if (curX > this.ActualWidth && i != idx - 1)
                    {
                        curX = cur.ActualWidth + cur.Margin.Left + cur.Margin.Right;
                        curY += defaultHeight;
                    }
                }
                //var newCardWidth = source.Size.Width + firstElem.Margin.Left + firstElem.Margin.Right;
                //if (idx < VisualChildrenCount)
                //{
                //    var nextElem = (FrameworkElement) this.GetVisualChild(idx);
                //    var nx = curX += nextElem.ActualWidth + nextElem.Margin.Left + nextElem.Margin.Right;
                //    if (nx > this.ActualWidth)
                //    {
                //        curX = ;
                //        curY = curY += defaultHeight;
                //        //_insertAtEndOfRow = true;
                //    }
                //}
            }
            _insertAdorner.MoveTo(new Point(curX, curY));

            // Assuming all items have the same size, get the size of the first one
            //double itemHeight = 0;
            //Double itemWidth = 0;
            //int rowSize;
            //if (VisualChildrenCount == 0)
            //{
            //    itemHeight = itemWidth = 0;
            //    rowSize = int.MaxValue;
            //}
            //else
            //{
            //    var firstChild = (FrameworkElement) GetVisualChild(0);
            //    if (firstChild != null)
            //    {
            //        itemHeight = firstChild.ActualHeight + firstChild.Margin.Top + firstChild.Margin.Bottom;
            //        itemWidth = firstChild.ActualWidth + firstChild.Margin.Left + firstChild.Margin.Right;
            //    }
            //    rowSize = Math.Max(1, (int) (ActualWidth/itemWidth));
            //}

            //// Position the adorner correctly            
            //int hOffset, vOffset = Math.DivRem(idx, rowSize, out hOffset);
            //if (hOffset == 0 && _insertAtEndOfRow)
            //{
            //    vOffset -= 1;
            //    hOffset = rowSize;
            //}
            //_insertAdorner.MoveTo(new Point(hOffset*itemWidth, vOffset*itemHeight));

            CancelSpacing();

            // Space neighbors
            if (idx >= VisualChildrenCount) return;
            _spacedItem2 = (UIElement) GetVisualChild(idx);
            SetSpacing(_spacedItem2, SpacingWidth);
            if (idx <= 0) return;
            _spacedItem1 = (UIElement) GetVisualChild(idx - 1);
            SetSpacing(_spacedItem1, -SpacingWidth);
        }

        public void HideInsertIndicator()
        {
            if (_insertAdorner == null) return;
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(AdornerLayerVisual ?? this);
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
            var translate = (TranslateTransform) group.Children[0];
            Animation.To = 0;
            Animation.FillBehavior = FillBehavior.Stop;
            translate.BeginAnimation(TranslateTransform.XProperty, Animation);
        }

        private static void SetSpacing(UIElement element, int value)
        {
            var group = (TransformGroup) element.RenderTransform;
            var translate = (TranslateTransform) group.Children[0];
            Animation.To = value;
            Animation.FillBehavior = FillBehavior.HoldEnd;
            translate.BeginAnimation(TranslateTransform.XProperty, Animation);
        }

        protected override Transform GetCurrentLayoutInfo(UIElement element, out Point arrangePosition)
        {
            var group = (TransformGroup) element.RenderTransform;

            Point currentPos = element.TransformToAncestor(this).Transform(new Point());
            arrangePosition = new Point(0, 0);
            if (@group.Inverse != null) arrangePosition = @group.Inverse.Transform(currentPos);

            return group.Children[1];
        }

        protected override void SetLayout2LayoutTransform(UIElement element, Transform transform)
        {
            var group = (TransformGroup) element.RenderTransform;
            group.Children[1] = transform;
        }

        #endregion
    }
}