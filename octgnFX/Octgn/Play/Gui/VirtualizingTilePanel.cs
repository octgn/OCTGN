using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Octgn.Play.Gui
{
    internal class VirtualizingTilePanel : VirtualizingPanel, IScrollInfo
    {
        // Dependency property that controls the size of the child elements
        public static readonly DependencyProperty ChildSizeProperty
            = DependencyProperty.RegisterAttached("ChildSize", typeof (double), typeof (VirtualizingTilePanel),
                                                  new FrameworkPropertyMetadata(200.0d,
                                                                                FrameworkPropertyMetadataOptions.
                                                                                    AffectsMeasure |
                                                                                FrameworkPropertyMetadataOptions.
                                                                                    AffectsArrange));

        public VirtualizingTilePanel()
        {
            // For use in the IScrollInfo implementation
            RenderTransform = _trans;
        }

        // Accessor for the child size dependency property
        public double ChildSize
        {
            get { return (double) GetValue(ChildSizeProperty); }
            set { SetValue(ChildSizeProperty, value); }
        }

        /// <summary>
        ///   Measure the children
        /// </summary>
        /// <param name="availableSize"> Size available </param>
        /// <returns> Size desired </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            UpdateScrollInfo(availableSize);

            // Figure out range that's visible based on layout algorithm
            int firstVisibleItemIndex, lastVisibleItemIndex;
            GetVisibleRange(out firstVisibleItemIndex, out lastVisibleItemIndex);

            // We need to access InternalChildren before the generator to work around a bug
            var children = InternalChildren;
            var generator = ItemContainerGenerator;

            // Get the generator position of the first visible data item
            var startPos = generator.GeneratorPositionFromIndex(firstVisibleItemIndex);

            // Get index where we'd insert the child for this position. If the item is realized
            // (position.Offset == 0), it's just position.Index, otherwise we have to add one to
            // insert after the corresponding child
            var childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;

            using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                for (var itemIndex = firstVisibleItemIndex;
                     itemIndex <= lastVisibleItemIndex;
                     ++itemIndex, ++childIndex)
                {
                    bool newlyRealized;

                    // Get or create the child
                    var child = generator.GenerateNext(out newlyRealized) as UIElement;
                    if (newlyRealized)
                    {
                        // Figure out if we need to insert the child at the end or somewhere in the middle
                        if (child != null)
                        {
                            if (childIndex >= children.Count)
                            {
                                AddInternalChild(child);
                            }
                            else
                            {
                                InsertInternalChild(childIndex, child);
                            }
                        }
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

        /// <summary>
        ///   Arrange the children
        /// </summary>
        /// <param name="finalSize"> Size available </param>
        /// <returns> Size used </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            var generator = ItemContainerGenerator;

            UpdateScrollInfo(finalSize);

            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];

                // Map the child offset to an item offset
                var itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

                ArrangeChild(itemIndex, child, finalSize);
            }

            return finalSize;
        }

        /// <summary>
        ///   Revirtualize items that are no longer visible
        /// </summary>
        /// <param name="minDesiredGenerated"> first item index that should be visible </param>
        /// <param name="maxDesiredGenerated"> last item index that should be visible </param>
        private void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated)
        {
            var children = InternalChildren;
            var generator = ItemContainerGenerator;

            for (var i = children.Count - 1; i >= 0; i--)
            {
                var childGeneratorPos = new GeneratorPosition(i, 0);
                var itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex >= minDesiredGenerated && itemIndex <= maxDesiredGenerated) continue;
                generator.Remove(childGeneratorPos, 1);
                RemoveInternalChildRange(i, 1);
            }
        }

        /// <summary>
        ///   When items are removed, remove the corresponding UI if necessary
        /// </summary>
        /// <param name="sender"> </param>
        /// <param name="args"> </param>
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

        // I've isolated the layout specific code to this region. If you want to do something other than tiling, this is
        // where you'll make your changes

        /// <summary>
        ///   Calculate the extent of the view based on the available size
        /// </summary>
        /// <param name="availableSize"> available size </param>
        /// <param name="itemCount"> number of data items </param>
        /// <returns> </returns>
        private Size CalculateExtent(Size availableSize, int itemCount)
        {
            var childrenPerRow = CalculateChildrenPerRow(availableSize);

            // See how big we are
            return new Size(childrenPerRow*ChildSize,
                            ChildSize*Math.Ceiling((double) itemCount/childrenPerRow));
        }

        /// <summary>
        ///   Get the range of children that are visible
        /// </summary>
        /// <param name="firstVisibleItemIndex"> The item index of the first visible item </param>
        /// <param name="lastVisibleItemIndex"> The item index of the last visible item </param>
        private void GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex)
        {
            var childrenPerRow = CalculateChildrenPerRow(_extent);

            firstVisibleItemIndex = (int) Math.Floor(_offset.Y/ChildSize)*childrenPerRow;
            lastVisibleItemIndex = (int) Math.Ceiling((_offset.Y + _viewport.Height)/ChildSize)*childrenPerRow - 1;

            var itemsControl = ItemsControl.GetItemsOwner(this);
            var itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;
            if (lastVisibleItemIndex >= itemCount)
                lastVisibleItemIndex = itemCount - 1;
        }

        /// <summary>
        ///   Get the size of the children. We assume they are all the same
        /// </summary>
        /// <returns> The size </returns>
        private Size GetChildSize()
        {
            return new Size(ChildSize, ChildSize);
        }

        /// <summary>
        ///   Position a child
        /// </summary>
        /// <param name="itemIndex"> The data item index of the child </param>
        /// <param name="child"> The element to position </param>
        /// <param name="finalSize"> The size of the panel </param>
        private void ArrangeChild(int itemIndex, UIElement child, Size finalSize)
        {
            var childrenPerRow = CalculateChildrenPerRow(finalSize);

            var row = itemIndex/childrenPerRow;
            var column = itemIndex%childrenPerRow;

            child.Arrange(new Rect(column*ChildSize, row*ChildSize, ChildSize, ChildSize));
        }

        /// <summary>
        ///   Helper function for tiling layout
        /// </summary>
        /// <param name="availableSize"> Size available </param>
        /// <returns> </returns>
        private int CalculateChildrenPerRow(Size availableSize)
        {
            // Figure out how many children fit on each row
            var childrenPerRow = double.IsPositiveInfinity(availableSize.Width)
                                     ? Children.Count
                                     : Math.Max(1, (int) Math.Floor(availableSize.Width/ChildSize));
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
            SetVerticalOffset(VerticalOffset - 10);
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + 10);
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
            var itemsControl = ItemsControl.GetItemsOwner(this);
            var itemCount = itemsControl.HasItems ? itemsControl.Items.Count : 0;

            var extent = CalculateExtent(availableSize, itemCount);
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
    }
}