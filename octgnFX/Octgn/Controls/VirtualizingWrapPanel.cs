using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Octgn.Controls
{
    public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
    {
        public static readonly DependencyProperty ChildWidthProperty =
            DependencyProperty.RegisterAttached("ChildWidth", typeof (double), typeof (VirtualizingWrapPanel),
                                                new FrameworkPropertyMetadata(100.0d,
                                                                              FrameworkPropertyMetadataOptions.
                                                                                  AffectsMeasure |
                                                                              FrameworkPropertyMetadataOptions.
                                                                                  AffectsArrange));

        public static readonly DependencyProperty ChildHeightProperty =
            DependencyProperty.RegisterAttached("ChildHeight", typeof (double), typeof (VirtualizingWrapPanel),
                                                new FrameworkPropertyMetadata(100.0d,
                                                                              FrameworkPropertyMetadataOptions.
                                                                                  AffectsMeasure |
                                                                              FrameworkPropertyMetadataOptions.
                                                                                  AffectsArrange));

        public VirtualizingWrapPanel()
        {
            // For use in the IScrollInfo implementation
            RenderTransform = _trans;
        }

        public double ChildWidth
        {
            get { return (double) GetValue(ChildWidthProperty); }
            set { SetValue(ChildWidthProperty, value); }
        }

        public double ChildHeight
        {
            get { return (double) GetValue(ChildHeightProperty); }
            set { SetValue(ChildHeightProperty, value); }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            ItemsControl itemCtrl = ItemsControl.GetItemsOwner(this);
            if (itemCtrl.Items.Count == 0)
                return new Size();

            UpdateScrollInfo(availableSize);
            // Figure out range that's visible based on layout algorithm
            int firstVisibleItemIndex, lastVisibleItemIndex;
            GetVisibleRange(out firstVisibleItemIndex, out lastVisibleItemIndex);
            // We need to access InternalChildren before the generator to work around a bug
            UIElementCollection children = InternalChildren;
            IItemContainerGenerator generator = ItemContainerGenerator;
            // Get the generator position of the first visible data item
            GeneratorPosition startPos = generator.GeneratorPositionFromIndex(firstVisibleItemIndex);
            // Get index where we'd insert the child for this position. If the item is realized
            // (position.Offset == 0), it's just position.Index, otherwise we have to add one to
            // insert after the corresponding child
            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;
            using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                for (int itemIndex = firstVisibleItemIndex;
                     itemIndex <= lastVisibleItemIndex;
                     itemIndex++, childIndex++)
                {
                    bool isNewlyRealized;
                    var child = generator.GenerateNext(out isNewlyRealized) as UIElement;
                    if (isNewlyRealized)
                    {
                        if (childIndex >= children.Count)
                            AddInternalChild(child);
                        else
                            InsertInternalChild(childIndex, child);
                        generator.PrepareItemContainer(child);
                    }
                    else
                    {
                        // The child has already been created, let's be sure it's in the right spot
                        Debug.Assert(child == children[childIndex], "Wrong child was generated");
                    }
                    // Measurements will depend on layout algorithm
                    if (child != null) child.Measure(GetChildSize());
                }
            }
            // Note: this could be deferred to idle time for efficiency
            CleanUpItems(firstVisibleItemIndex, lastVisibleItemIndex);
            return availableSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            IItemContainerGenerator generator = ItemContainerGenerator;
            UpdateScrollInfo(finalSize);
            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];
                // Map the child offset to an item offset
                int itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
                ArrangeChild(itemIndex, child, finalSize);
            }
            return finalSize;
        }

        private void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated)
        {
            UIElementCollection children = InternalChildren;
            IItemContainerGenerator generator = ItemContainerGenerator;
            for (int i = children.Count - 1; i >= 0; i--)
            {
                var childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex >= minDesiredGenerated && itemIndex <= maxDesiredGenerated) continue;
                generator.Remove(childGeneratorPos, 1);
                RemoveInternalChildRange(i, 1);
            }
        }

        /// When items are removed, remove the corresponding UI if necessary
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
            }
        }

        #region Layout specific code

        /// Calculate the extent of the view based on the available size
        private Size CalculateExtent(Size availableSize, int itemCount)
        {
            int childrenPerRow = CalculateChildrenPerRow(availableSize);
            // See how big we are
            return new Size(childrenPerRow*ChildWidth,
                            ChildHeight*Math.Ceiling((double) itemCount/childrenPerRow));
        }

        /// Get the range of children that are visible
        private void GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex)
        {
            int childrenPerRow = CalculateChildrenPerRow(_extent);
            firstVisibleItemIndex = (int) Math.Floor(_offset.Y/ChildHeight)*childrenPerRow;
            lastVisibleItemIndex = (int) Math.Ceiling((_offset.Y + _viewport.Height)/ChildHeight)*childrenPerRow - 1;
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;
            if (lastVisibleItemIndex >= itemCount)
                lastVisibleItemIndex = itemCount - 1;
        }

        /// Get the size of the children. We assume they are all the same
        private Size GetChildSize()
        {
            return new Size(ChildWidth, ChildHeight);
        }

        /// Position a child
        private void ArrangeChild(int itemIndex, UIElement child, Size finalSize)
        {
            int childrenPerRow = CalculateChildrenPerRow(finalSize);
            int row = itemIndex/childrenPerRow;
            int column = itemIndex%childrenPerRow;
            child.Arrange(new Rect(column*ChildWidth, row*ChildHeight, ChildWidth, ChildHeight));
        }

        private int CalculateChildrenPerRow(Size availableSize)
        {
            // Figure out how many children fit on each row
            int childrenPerRow;
            childrenPerRow = double.IsPositiveInfinity(availableSize.Width) ? Children.Count : Math.Max(1, (int) Math.Floor(availableSize.Width/ChildWidth));
            return childrenPerRow;
        }

        #endregion

        #region IScrollInfo implementation

        // See Ben Constable's series of posts at http://blogs.msdn.com/bencon/

        private readonly TranslateTransform _trans = new TranslateTransform();
        private Size _extent = new Size(0, 0);
        private Point _offset;
        private ScrollViewer _owner;
        private Size _viewport = new Size(0, 0);

        public ScrollViewer ScrollOwner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public bool CanHorizontallyScroll { get; set; }

        public bool CanVerticallyScroll { get; set; }

        public double HorizontalOffset
        {
            get { return _offset.X; }
        }

        public double VerticalOffset
        {
            get { return _offset.Y; }
        }

        public double ExtentHeight
        {
            get { return _extent.Height; }
        }

        public double ExtentWidth
        {
            get { return _extent.Width; }
        }

        public double ViewportHeight
        {
            get { return _viewport.Height; }
        }

        public double ViewportWidth
        {
            get { return _viewport.Width; }
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - 10);
        }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + 10);
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - _viewport.Height);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + _viewport.Height);
        }

        public void MouseWheelUp()
        {
            //SetVerticalOffset(this.VerticalOffset - 10);
            SmoothScroll(10);
        }

        public void MouseWheelDown()
        {
            //SetVerticalOffset(this.VerticalOffset + 10);
            SmoothScroll(-10);
        }

        public void LineLeft()
        {
            throw new InvalidOperationException();
        }

        public void LineRight()
        {
            throw new InvalidOperationException();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            return new Rect();
        }

        public void MouseWheelLeft()
        {
            throw new InvalidOperationException();
        }

        public void MouseWheelRight()
        {
            throw new InvalidOperationException();
        }

        public void PageLeft()
        {
            throw new InvalidOperationException();
        }

        public void PageRight()
        {
            throw new InvalidOperationException();
        }

        public void SetHorizontalOffset(double offset)
        {
            throw new InvalidOperationException();
        }

        public void SetVerticalOffset(double offset)
        {
            if (offset < 0 || _viewport.Height >= _extent.Height)
            {
                offset = 0;
            }
            else
            {
                if (offset + _viewport.Height >= _extent.Height)
                {
                    offset = _extent.Height - _viewport.Height;
                }
            }
            _offset.Y = offset;
            if (_owner != null)
                _owner.InvalidateScrollInfo();
            _trans.Y = -offset;
            // Force us to realize the correct children
            InvalidateMeasure();
        }

        private void UpdateScrollInfo(Size availableSize)
        {
            // See how many items there are
            ItemsControl itemsControl = ItemsControl.GetItemsOwner(this);
            int itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;
            Size extent = CalculateExtent(availableSize, itemCount);
            // Update extent
            if (extent != _extent)
            {
                _extent = extent;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }
            // Update viewport
            if (availableSize == _viewport) return;
            _viewport = availableSize;
            if (_owner != null)
                _owner.InvalidateScrollInfo();
        }

        #endregion

        #region Smooth scrolling

        private static readonly Duration SmoothScrollDuration = TimeSpan.FromMilliseconds(500);

        private static readonly DependencyProperty AnimatableVerticalOffsetProperty =
            DependencyProperty.Register("AnimatableVerticalOffset", typeof (double), typeof (VirtualizingWrapPanel),
                                        new UIPropertyMetadata(0.0, AnimatableVerticalOffsetChanged));

        private DoubleAnimation scrollAnimation;
        private int scrollDirection;
        private double scrollTarget;

        private double AnimatableVerticalOffset
        {
            get { return (double) GetValue(AnimatableVerticalOffsetProperty); }
            set { SetValue(AnimatableVerticalOffsetProperty, value); }
        }

        private static void AnimatableVerticalOffsetChanged(DependencyObject sender,
                                                            DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as VirtualizingWrapPanel;
            if (ctrl != null) ctrl.SetVerticalOffset((double) e.NewValue);
        }

        private void SmoothScroll(int delta)
        {
            // Add inerita to scrolling for a very smooth effect      
            int sign = Math.Sign(delta);
            double offset = -sign*48.0;
            if (sign == scrollDirection)
                scrollTarget += offset;
            else
            {
                scrollDirection = sign;
                scrollTarget = VerticalOffset + offset;
            }
            EnsureScrollAnimation();
            scrollAnimation.From = VerticalOffset;
            scrollAnimation.To = scrollTarget;
            BeginAnimation(AnimatableVerticalOffsetProperty, scrollAnimation);
        }

        private void EnsureScrollAnimation()
        {
            scrollAnimation = new DoubleAnimation {Duration = SmoothScrollDuration, DecelerationRatio = 0.5};
            scrollAnimation.Completed += delegate
                                             {
                                                 scrollAnimation = null;
                                                 scrollDirection = 0;
                                             };
        }

        #endregion

        // TODO: this code duplicated from CardListControl.xaml.cs
    }
}