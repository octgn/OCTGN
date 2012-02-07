using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Octgn.Play.Gui
{
    public class PilePanel : VirtualizingPanel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = new Size();
            ItemsControl parent = ItemsControl.GetItemsOwner(this);
            int count = parent != null && parent.HasItems ? parent.Items.Count : 0;

            // Next line needed otherwise ItemContainerGenerator is null (bug in WinFX ?)
            UIElementCollection children = base.InternalChildren;
            IItemContainerGenerator generator = this.ItemContainerGenerator;

            if (count == 0)
            {
                generator.RemoveAll();
                if (children.Count > 0) RemoveInternalChildRange(0, children.Count);
                return desiredSize;
            }

            // Get the generator position of the first visible data item
            GeneratorPosition startPos = generator.GeneratorPositionFromIndex(count - 1);
            using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                bool newlyRealized;
                // Get or create the child
                UIElement child = generator.GenerateNext(out newlyRealized) as UIElement;
                if (newlyRealized)
                {
                    AddInternalChild(child);
                    generator.PrepareItemContainer(child);
                }
                child.Measure(availableSize);
                desiredSize = child.DesiredSize;
            }

            // Remove all other items than the top one
            for (int i = children.Count - 1; i >= 0; i--)
            {
                GeneratorPosition childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);
                if (itemIndex != count - 1)
                {
                    generator.Remove(childGeneratorPos, 1);
                    RemoveInternalChildRange(i, 1);
                }
            }
            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (VisualChildrenCount > 0)
            {
                UIElement child = GetVisualChild(0) as UIElement;
                child.Arrange(new Rect(finalSize));
            }
            return finalSize;
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
            }
            base.OnItemsChanged(sender, args);
        }
    }
}