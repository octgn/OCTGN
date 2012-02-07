using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Octgn.Controls;

namespace Octgn.Play.Gui
{
    public class WrapPanel : AnimatedWrapPanel
    {
        #region Private vars

        private const int SpacingWidth = 8;

        private static readonly DoubleAnimation animation = new DoubleAnimation
                                                                {
                                                                    DecelerationRatio = 0.7,
                                                                    Duration = new Duration(TimeSpan.FromMilliseconds(300))
                                                                };

        private InsertAdorner insertAdorner;

        private UIElement spacedItem1, spacedItem2;

        #endregion

        public Visual AdornerLayerVisual { get; set; }
        public FrameworkElement ClippingVisual { get; set; }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            if (visualAdded != null)
            {
                var child = (UIElement) visualAdded;
                var group = new TransformGroup();
                child.RenderTransform = group;
                group.Children.Add(new TranslateTransform()); // used for drag and drop effects
                group.Children.Add(Transform.Identity); // slot reserved for layout to layout animations (in base class)
            }
        }

        #region Insertion related

        private bool insertAtEndOfRow; // indicates wheter the insertion adorner should preferably 
        // get displayed at the end of row or at the beginning of the next one

        public int GetIndexFromPoint(Point position)
        {
            if (VisualChildrenCount == 0) return 0;
            // Assuming all items have the same size, get the size of the first one
            var firstChild = (FrameworkElement) GetVisualChild(0);
            if (firstChild != null)
            {
                double itemHeight = firstChild.ActualHeight + firstChild.Margin.Top + firstChild.Margin.Bottom;
                double itemWidth = firstChild.ActualWidth + firstChild.Margin.Left + firstChild.Margin.Right;
                int rowSize = Math.Max(1, (int) (ActualWidth/itemWidth));

                int hOffset = Math.Min(rowSize, (int) (position.X/itemWidth + 0.5));
                insertAtEndOfRow = hOffset == rowSize;
                int index = (int) (position.Y/itemHeight)*rowSize + hOffset;

                if (index > VisualChildrenCount) index = VisualChildrenCount;
                return index;
            }
            return 0;
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
                var visualChild = (FrameworkElement) GetVisualChild(idx - 1);
                if (visualChild != null && (idx > 0 && visualChild.DataContext == source))
                {
                    HideInsertIndicator();
                    return;
                }
            }

            // Create an adorner if none exists yet
            if (insertAdorner == null)
            {
                insertAdorner = new InsertAdorner(this) {Height = 106, ClippingVisual = ClippingVisual};
                    // HACK: currently WarpPanel is only used by the group window, but card height should be a property and not hard-coded like that
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(AdornerLayerVisual ?? this);
                layer.Add(insertAdorner);
            }

            // Assuming all items have the same size, get the size of the first one
            double itemHeight = 0;
            Double itemWidth = 0;
            int rowSize;
            if (VisualChildrenCount == 0)
            {
                itemHeight = itemWidth = 0;
                rowSize = int.MaxValue;
            }
            else
            {
                var firstChild = (FrameworkElement) GetVisualChild(0);
                if (firstChild != null)
                {
                    itemHeight = firstChild.ActualHeight + firstChild.Margin.Top + firstChild.Margin.Bottom;
                    itemWidth = firstChild.ActualWidth + firstChild.Margin.Left + firstChild.Margin.Right;
                }
                rowSize = Math.Max(1, (int) (ActualWidth/itemWidth));
            }

            // Position the adorner correctly            
            int hOffset, vOffset = Math.DivRem(idx, rowSize, out hOffset);
            if (hOffset == 0 && insertAtEndOfRow)
            {
                vOffset -= 1;
                hOffset = rowSize;
            }
            insertAdorner.MoveTo(new Point(hOffset*itemWidth, vOffset*itemHeight));

            CancelSpacing();

            // Space neighbors
            if (idx < VisualChildrenCount)
            {
                spacedItem2 = (UIElement) GetVisualChild(idx);
                SetSpacing(spacedItem2, SpacingWidth);
                if (idx > 0)
                {
                    spacedItem1 = (UIElement) GetVisualChild(idx - 1);
                    SetSpacing(spacedItem1, -SpacingWidth);
                }
            }
        }

        public void HideInsertIndicator()
        {
            if (insertAdorner == null) return;
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(AdornerLayerVisual ?? this);
            layer.Remove(insertAdorner);
            insertAdorner = null;
            CancelSpacing();
        }

        private void CancelSpacing()
        {
            if (spacedItem1 != null)
            {
                CancelSpacing(spacedItem1);
                spacedItem1 = null;
            }
            if (spacedItem2 != null)
            {
                CancelSpacing(spacedItem2);
                spacedItem2 = null;
            }
        }

        private void CancelSpacing(UIElement element)
        {
            var group = (TransformGroup) element.RenderTransform;
            var translate = (TranslateTransform) group.Children[0];
            animation.To = 0;
            animation.FillBehavior = FillBehavior.Stop;
            translate.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        private void SetSpacing(UIElement element, int value)
        {
            var group = (TransformGroup) element.RenderTransform;
            var translate = (TranslateTransform) group.Children[0];
            animation.To = value;
            animation.FillBehavior = FillBehavior.HoldEnd;
            translate.BeginAnimation(TranslateTransform.XProperty, animation);
        }

        protected override Transform GetCurrentLayoutInfo(UIElement element, out Point arrangePosition)
        {
            var group = (TransformGroup) element.RenderTransform;

            Point currentPos = element.TransformToAncestor(this).Transform(new Point());
            Debug.Assert(@group.Inverse != null, "@group.Inverse != null");
            arrangePosition = @group.Inverse.Transform(currentPos);

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