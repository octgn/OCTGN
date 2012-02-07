using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Octgn.Play.Actions;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using Octgn.Definitions;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Octgn.Play.Gui
{
    partial class TableControl : GroupControl
    {
        protected bool isCardSizeValid = false;
        private IDragOperation dragOperation;

        private Size _cardSize;
        protected Size CardSize
        {
            get
            {
                if (!isCardSizeValid)
                {
                    double scale = Math.Min(ActualWidth / Program.Game.Definition.TableDefinition.Width, ActualHeight / Program.Game.Definition.TableDefinition.Height);
                    scale *= Zoom;
                    _cardSize = new Size(Program.Game.Definition.CardDefinition.Width * scale,
                                         Program.Game.Definition.CardDefinition.Height * scale);
                    isCardSizeValid = true;
                }
                return _cardSize;
            }
        }

        private int _defaultWidth, _defaultHeight;
        public int DefaultCardWidth
        { get { return _defaultWidth; } }
        public int DefaultCardHeight
        { get { return _defaultHeight; } }

        public TableControl()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;

            var tableDef = Program.Game.Definition.TableDefinition;
            if (tableDef.Background != null)
                SetBackground(tableDef);
            if (tableDef.Board != null)
                SetBoard(tableDef);

            if (!Program.GameSettings.UseTwoSidedTable)
                middleLine.Visibility = Visibility.Collapsed;

            if (Player.LocalPlayer.InvertedTable)
                transforms.Children.Insert(0, new ScaleTransform(-1, -1));

            _defaultWidth = Program.Game.Definition.CardDefinition.Width;
            _defaultHeight = Program.Game.Definition.CardDefinition.Height;
            SizeChanged += delegate
            {
                isCardSizeValid = false;
                AspectRatioChanged();
            };
            MoveCard.Done += CardMoved;
            CreateCard.Done += CardCreated;
            Unloaded += delegate
            {
                MoveCard.Done -= CardMoved;
                CreateCard.Done -= CardCreated;
            };
            Loaded += delegate { CenterView(); };
        }

        internal override ItemContainerGenerator GetItemContainerGenerator()
        { return cardsView.ItemContainerGenerator; }

        public void CenterView()
        {
            var tableDef = Program.Game.Definition.TableDefinition;
            Offset = new Vector(tableDef.Width / 2, tableDef.Height / 2);
        }

        private void SetBackground(GroupDef tableDef)
        {
            var backBrush = new ImageBrush(new BitmapImage(new Uri(tableDef.Background)));
            if (tableDef.BackgroundStyle != null)
                switch (tableDef.BackgroundStyle)
                {
                    case "tile":
                        backBrush.TileMode = TileMode.Tile;
                        backBrush.Viewport = new Rect(0, 0, backBrush.ImageSource.Width, backBrush.ImageSource.Height);
                        backBrush.ViewportUnits = BrushMappingMode.Absolute;
                        break;
                    case "uniform": backBrush.Stretch = Stretch.Uniform; break;
                    case "uniformToFill": backBrush.Stretch = Stretch.UniformToFill; break;
                    case "stretch": backBrush.Stretch = Stretch.Fill; break;
                }
            Background = backBrush;
        }

        private void SetBoard(GroupDef tableDef)
        {
            var pos = tableDef.BoardPosition;
            var img = new Image
            {
                Source = new BitmapImage(new Uri(tableDef.Board)),
                Width = pos.Width,
                Height = pos.Height,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(pos.Left, pos.Top, 0, 0)
            };
            boardContainer.Children.Insert(0, img);
        }

        public static bool IsInInvertedZone(double y)
        { return y < -Program.Game.Definition.CardDefinition.Height / 2; }

        private void CardMoved(object sender, EventArgs e)
        {
            var action = (MoveCard)sender;
            if (action.to == group)
                BringCardIntoView(action.card);
        }

        private void CardCreated(object sender, EventArgs e)
        {
            var action = (CreateCard)sender;
            BringCardIntoView(action.card);
        }

        protected override void OnCardOver(object sender, CardsEventArgs e)
        {
            base.OnCardOver(sender, e);
            e.CardSize = CardSize;

            if (Program.GameSettings.UseTwoSidedTable)
            {
                var cardDef = Program.Game.Definition.CardDefinition;
                var cardCtrl = (CardControl)e.OriginalSource;
                var baseCard = cardCtrl.Card;
                double mouseY = Mouse.GetPosition(cardsView).Y;
                double baseY = (cardCtrl.IsInverted ||
                               (Player.LocalPlayer.InvertedTable && !cardCtrl.IsOnTableCanvas)) ?
                   mouseY - cardDef.Height + e.MouseOffset.Y :
                   mouseY - e.MouseOffset.Y;
                foreach (var adorner in e.Adorners)
                {
                    var y = baseY + adorner.SourceCard.Card.Y - baseCard.Y;
                    adorner.onHoverRequestInverted = IsInInvertedZone(y) ^ Player.LocalPlayer.InvertedTable;
                }
            }
        }

        protected override void OnCardDropped(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            CardControl cardCtrl = e.OriginalSource as CardControl;

            var cardDef = Program.Game.Definition.CardDefinition;
            int delta = cardDef.Height - cardDef.Width;
            Table table = Program.Game.Table;
            Vector mouseOffset;
            if (cardCtrl.IsInverted ||
               (Player.LocalPlayer.InvertedTable && !cardCtrl.IsOnTableCanvas))
                mouseOffset = new Vector(cardDef.Width - e.MouseOffset.X, cardDef.Height - e.MouseOffset.Y);
            else
                mouseOffset = e.MouseOffset;
            Point pt = MousePosition();
            pt -= mouseOffset;

            if (Selection.IsEmpty() || !(Selection.Source is Table))
            {
                if (Program.GameSettings.UseTwoSidedTable && (e.ClickedCard.Orientation & CardOrientation.Rot90) != 0)
                {
                    // We have to offset the position if we cross the middle line
                    bool newPosInverted = IsInInvertedZone(pt.Y);
                    if (!cardCtrl.IsInverted && newPosInverted)
                    { pt.X += delta; pt.Y += delta; }
                    else if (cardCtrl.IsInverted && !newPosInverted)
                    { pt.X -= delta; pt.Y -= delta; }
                }

                int idx = table.GetCardIndex(e.ClickedCard);
                if (idx != -1 || table.Visibility != GroupVisibility.Undefined)
                    e.FaceUp = e.ClickedCard.FaceUp;
                if (idx == -1)
                    idx = table.Cards.Count;
                e.ClickedCard.MoveToTable((int)pt.X, (int)pt.Y, e.FaceUp.Value, idx);

                // If there were other cards (i.e. dragging from a count number in GroupWindow), move them accordingly
                double xOffset = Program.Game.Definition.CardDefinition.Width * 1.05;
                foreach (Card c in e.Cards)
                {
                    if (c == e.ClickedCard) continue;
                    pt.X += xOffset;
                    c.MoveToTable((int)pt.X, (int)pt.Y, e.FaceUp.Value, idx);
                }
            }
            else
            {
                // There are multiple cards, coming from the table. Offset them accordingly
                double dx = pt.X - e.ClickedCard.X;
                double dy = pt.Y - e.ClickedCard.Y;
                foreach (Card c in Selection.Cards)
                {
                    int x = (int)(c.X + dx), y = (int)(c.Y + dy);
                    int idx = table.GetCardIndex(c);
                    // If the card is tapped and has crossed the middle line in a two-sided table, we have to adjust its position
                    if (Program.GameSettings.UseTwoSidedTable && (c.Orientation & CardOrientation.Rot90) != 0)
                    {
                        bool oldPosInverted = IsInInvertedZone(c.Y);
                        bool newPosInverted = IsInInvertedZone(y);
                        if (!oldPosInverted && newPosInverted)
                        { x += delta; y += delta; }
                        else if (oldPosInverted && !newPosInverted)
                        { x -= delta; y -= delta; }
                    }
                    c.MoveToTable(x, y, c.FaceUp, idx);
                }
            }
        }

        private bool IsCardAtInvertedPosition(double y, CardEventArgs e)
        {
            var cardCtrl = e.OriginalSource as CardControl;
            double offset = e.MouseOffset.Y;
            if (Player.LocalPlayer.InvertedTable)
            {
                y -= Program.Game.Definition.CardDefinition.Height;
                offset = -offset;
            }
            y -= offset;
            return TableControl.IsInInvertedZone(y);
        }

        #region Mouse

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (Keyboard.IsKeyDown(Key.Space))
            {
                dragOperation = new Pan(this);
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            // Prevents popup menu from opening during DnD operations, which may result in e.g. stuck selection rectangles
            if (dragOperation == null)
                base.OnPreviewMouseRightButtonUp(e);
            else
                e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            dragOperation = new SelectCards(this);
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dragOperation != null)
                dragOperation.Dragging(e);
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (dragOperation != null)
                    {
                        dragOperation.EndDrag();
                        dragOperation = null;
                    }
                    e.Handled = true;
                    break;
            }
            base.OnMouseUp(e);
        }

        public Point MousePosition()
        { return Mouse.GetPosition(cardsView); }

        #endregion

        #region Zoom and pan

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof(double), typeof(TableControl),
                                        new PropertyMetadata(1.0, ZoomChanged));
        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof(Vector), typeof(TableControl),
                                        new PropertyMetadata(new Vector(), OffsetChanged));

        public static readonly DependencyProperty YCenterOffsetProperty =
              DependencyProperty.Register("YCenterOffset", typeof(double), typeof(TableControl));

        public double Zoom
        {
            get { return (double)GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public Vector Offset
        {
            get { return (Vector)GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public double YCenterOffset
        {
            get { return (double)GetValue(YCenterOffsetProperty); }
            set { SetValue(YCenterOffsetProperty, value); }
        }

        protected static void ZoomChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        { ((TableControl)sender).ZoomChanged(); }

        protected void ZoomChanged()
        {
            isCardSizeValid = false;
            UpdateYCenterOffset();
        }

        protected static void OffsetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        { ((TableControl)sender).OffsetChanged(); }

        protected void OffsetChanged()
        { UpdateYCenterOffset(); }

        private DispatcherOperation updateYCenterOperation;

        protected void UpdateYCenterOffset()
        {
            // Don't queue the update multiple times (could happen e.g. when updating the Offset and then the Zoom property)
            if (updateYCenterOperation != null && updateYCenterOperation.Status == DispatcherOperationStatus.Pending)
                return;

            // Bug fix: if done immediately, the layout is slightly incorrect (e.g. in the case of mouse wheel zoom).
            // so we dispatch the update until all transforms are updated.         
            updateYCenterOperation = Dispatcher.BeginInvoke(new Action(delegate
            {
                Point pt = cardsView.TransformToAncestor(this).Transform(new Point());
                YCenterOffset = pt.Y;
            }), System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;
            isCardSizeValid = false;

            Point center = e.GetPosition(cardsView);
            double oldZoom = Zoom;     // May be animated

            // Set the new zoom level
            if (e.Delta > 0)
                Zoom = oldZoom + 0.125;
            else if (oldZoom > 0.15)
                Zoom = oldZoom - 0.125;
            BeginAnimation(ZoomProperty, null);     // Stop any animation, which could override the current zoom level

            // Adjust the offset to center the zoom on the mouse pointer
            double ratio = oldZoom - Zoom;
            if (Player.LocalPlayer.InvertedTable) ratio = -ratio;
            Offset += new Vector(center.X * ratio, center.Y * ratio);
            BeginAnimation(OffsetProperty, null);   // Stop any animation, which could override the current Offset

            base.OnMouseWheel(e);
        }

        #endregion

        #region Table extents

        private double logicalRatio;
        private Size stretchMargins;

        protected void AspectRatioChanged()
        {
            if (Program.Game == null) return;   // Bug fix: Program.Game can be null when closing the game window.

            int tWidth = Program.Game.Table.Definition.Width, tHeight = Program.Game.Table.Definition.Height;
            double wRatio = ActualWidth / tWidth, hRatio = ActualHeight / tHeight;

            if (wRatio < hRatio)
            {
                logicalRatio = wRatio;
                stretchMargins.Width = 0;
                stretchMargins.Height = Math.Max((ActualHeight / wRatio - tHeight) / 2, 0);
            }
            else
            {
                logicalRatio = hRatio;
                stretchMargins.Width = Math.Max((ActualWidth / logicalRatio - tWidth) / 2, 0);
                stretchMargins.Height = 0;
            }

            UpdateYCenterOffset();
        }

        protected void BringCardIntoView(Card card)
        {
            // Get the current target viewport (bypass animations in progress)
            var transform = TransformToDescendant(cardsView);
            Rect visibleBounds = transform.TransformBounds(new Rect(0, 0, ActualWidth, ActualHeight));

            Rect cardRect = new Rect(card.X, card.Y, Program.Game.Definition.CardDefinition.Width, Program.Game.Definition.CardDefinition.Height);
            if (visibleBounds.Contains(cardRect)) return;   // okay, already completely into view

            // Compute the new table bounds
            Rect newBounds = visibleBounds;
            if (cardRect.Left < visibleBounds.Left) newBounds.X = cardRect.Left;
            newBounds.Width = Math.Max(visibleBounds.Right, cardRect.Right) - newBounds.Left;
            if (cardRect.Top < visibleBounds.Top) newBounds.Y = cardRect.Top;
            newBounds.Height = Math.Max(visibleBounds.Bottom, cardRect.Bottom) - newBounds.Top;

            if (Player.LocalPlayer.InvertedTable)
            {
                // Transform the viewport so that it will fit correctly after the invert transform is applied
                newBounds.X = -newBounds.Right;
                newBounds.Y = -newBounds.Bottom;
            }

            // Compute the zoom and offset corresponding to the new view
            double newZoom = Math.Min(
                (Program.Game.Table.Definition.Width + 2 * stretchMargins.Width) / newBounds.Width,
                (Program.Game.Table.Definition.Height + 2 * stretchMargins.Height) / newBounds.Height);
            Vector newOffset = new Vector(
                -stretchMargins.Width - newBounds.X * newZoom,
                -stretchMargins.Height - newBounds.Y * newZoom);

            // Combine new values with the current ones 
            // (bypassing animations, e.g. when moving several cards outside the bounds at the same time
            var realZoom = (double)GetAnimationBaseValue(ZoomProperty);
            //var realOffset = (Vector)GetAnimationBaseValue(OffsetProperty);
            if (newZoom > realZoom) newZoom = realZoom;
            //if (newOffset.X < realOffset.X) newOffset.X = realOffset.X;
            //if (newOffset.Y < realOffset.Y) newOffset.Y = realOffset.Y;

            double oldZoom = Zoom;
            Vector oldOffset = Offset;
            Zoom = newZoom; Offset = newOffset;
            // Animate the table with the result
            AnimationTimeline anim = new DoubleAnimation() { From = oldZoom, To = newZoom, Duration = TimeSpan.FromMilliseconds(150), FillBehavior = FillBehavior.Stop };
            BeginAnimation(ZoomProperty, anim);
            anim = new VectorAnimation() { From = oldOffset, To = newOffset, Duration = TimeSpan.FromMilliseconds(150), FillBehavior = FillBehavior.Stop };
            BeginAnimation(OffsetProperty, anim);

            // Note: the new visibleBounds may end up bigger than newBounds, 
            // because the window aspect ratio may be different
        }

        #endregion

        #region Focus and keyboard

        public static readonly RoutedEvent TableKeyEvent = EventManager.RegisterRoutedEvent("TableKey", RoutingStrategy.Bubble, typeof(EventHandler<TableKeyEventArgs>), typeof(TableControl));

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            Focus();
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Key == Key.Space) UpdateCursor();
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Space) UpdateCursor();
            base.OnPreviewKeyUp(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            UpdateCursor();
            base.OnMouseEnter(e);
        }

        private void UpdateCursor()
        {
            Cursor = Keyboard.IsKeyDown(Key.Space) ? Cursors.ScrollAll : null;
        }

        #endregion

        #region Context menu

        protected Point contextMenuPosition;

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            contextMenuPosition = MousePosition();
            base.OnContextMenuOpening(e);
        }

        internal override void ShowContextMenu(Card card, bool showGroupActions = true)
        {
            contextMenuPosition = MousePosition();
            base.ShowContextMenu(card, card == null); // Don't show group actions when a card is selected on table
        }

        protected override System.Collections.Generic.List<Control> CreateCardMenuItems(GroupDef def)
        {
            var items = base.CreateCardMenuItems(def);

            var item = new MenuItem { Header = "Move to" };
            var subItem = new MenuItem { Header = Program.Game.Definition.PlayerDefinition.Hand.Name, InputGestureText = Program.Game.Definition.PlayerDefinition.Hand.Shortcut };
            subItem.Click += delegate { Selection.Do(c => c.MoveTo(Player.LocalPlayer.Hand, true), contextCard); };
            item.Items.Add(subItem);
            var groupDefs = Program.Game.Definition.PlayerDefinition.Groups;
            var moveToBottomItems = new List<MenuItem>();
            for (int i = 0; i < groupDefs.Length; ++i)
            {
                var groupDef = groupDefs[i];
                var group = Player.LocalPlayer.IndexedGroups[i + 1];	 // 0 is hand
                subItem = new MenuItem { Header = groupDef.Name, InputGestureText = groupDef.Shortcut };
                subItem.Click += delegate { Selection.Do(c => c.MoveTo(group, true), contextCard); };
                item.Items.Add(subItem);
                subItem = new MenuItem
                {
                    Header = string.Format("Bottom of {0}", groupDef.Name),
                    InputGestureText = string.IsNullOrEmpty(groupDef.Shortcut) ? null : "Alt+" + groupDef.Shortcut
                };
                subItem.Click += delegate { Selection.Do(c => c.MoveTo(group, true, group.Count), contextCard); };
                moveToBottomItems.Add(subItem);
            }
            if (moveToBottomItems.Count > 0) item.Items.Add(new Separator());
            foreach (var x in moveToBottomItems) item.Items.Add(x);
            items.Add(item);

            item = new MenuItem { Header = "Bring to front", InputGestureText = "PgUp" };
            item.Click += delegate { Selection.Do(c => Program.Game.Table.BringToFront(c), contextCard); };
            items.Add(item);

            item = new MenuItem { Header = "Send to back", InputGestureText = "PgDn" };
            item.Click += delegate { Selection.Do(c => Program.Game.Table.SendToBack(c), contextCard); };
            items.Add(item);

            return items;
        }

        protected override void GroupActionClicked(object sender, RoutedEventArgs e)
        {
            ActionDef action = (ActionDef)((MenuItem)sender).Tag;
            if (action.Execute != null)
                scriptEngine.ExecuteOnGroup(action.Execute, group, contextMenuPosition);
        }

        protected override void CardActionClicked(object sender, RoutedEventArgs e)
        {
            var action = (ActionDef)((MenuItem)sender).Tag;
            if (action.Execute != null)
                scriptEngine.ExecuteOnCards(action.Execute, Selection.ExtendToSelection(contextCard), contextMenuPosition);
            else if (action.BatchExecute != null)
                scriptEngine.ExecuteOnBatch(action.BatchExecute, Selection.ExtendToSelection(contextCard), contextMenuPosition);
        }

        #endregion

        #region Drag operations

        private sealed class Pan : DragOperation<TableControl>
        {
            public Pan(TableControl table)
                : base(table)
            { }

            protected override void StartDragCore(Point position)
            { }

            protected override void DraggingCore(Point position, Vector delta)
            {
                target.Offset += delta / target.logicalRatio;
                target.BeginAnimation(OffsetProperty, null);   // Stop any animation, which could override the current Offset
            }

            protected override void EndDragCore()
            { }
        }

        private sealed class SelectCards : DragOperation<TableControl>
        {
            public SelectCards(TableControl table)
                : base(table)
            { }

            private SelectAdorner adorner;
            private Point fromPt;

            protected override void StartDragCore(Point position)
            {
                fromPt = position;
                adorner = new SelectAdorner(target) { Rectangle = new Rect(position, position) };
                var layer = AdornerLayer.GetAdornerLayer(target);
                layer.Add(adorner);
            }

            protected override void DraggingCore(Point position, Vector delta)
            {
                // Update adorner
                var rect = new Rect(fromPt, position);
                adorner.Rectangle = rect;

                // Get position in table space
                GeneralTransform transform = target.TransformToDescendant(target.cardsView);
                rect = transform.TransformBounds(rect);
                var width = Program.Game.Definition.CardDefinition.Width;
                var height = Program.Game.Definition.CardDefinition.Height;

                // Remove cards which are not in the selection anymore
                Selection.RemoveAll(card => !rect.IntersectsWith(ComputeCardBounds(card, width, height)));

                // Add cards which entered the selection rectangle
                var allCards = target.group.Cards;
                var cards = from card in allCards
                            where card.Controller == Player.LocalPlayer
                              && !card.Selected
                              && rect.IntersectsWith(ComputeCardBounds(card, width, height))
                            select card;
                foreach (var card in cards) Selection.Add(card);
            }

            protected override void EndDragCore()
            {
                var layer = AdornerLayer.GetAdornerLayer(target);
                layer.Remove(adorner);
            }

            private Rect ComputeCardBounds(Card c, int w, int h)
            {
                Rect result =
                    // Case 1: straight card
                  (c.Orientation & CardOrientation.Rot90) == 0 ?
                  new Rect(c.X, c.Y, w, h) :
                    // Case 2: rotated card on a 1-sided table
                  !Program.GameSettings.UseTwoSidedTable ?
                  new Rect(c.X, c.Y + h - w, h, w) :
                    // Case 3: rotated card on a 2-sided table, the card is not inversed
                  !IsInInvertedZone(c.Y) ?
                  new Rect(c.X, c.Y + h - w, h, w) :
                    // Case 4: rotated and inversed card on a 2-sided table
                  new Rect(c.X - h + w, c.Y, h, w);
                return result;
            }
        }

        #endregion
    }

    public class TableKeyEventArgs : RoutedEventArgs
    {
        public readonly KeyEventArgs KeyEventArgs;

        public TableKeyEventArgs(object source, KeyEventArgs keyEventArgs)
            : base(TableControl.TableKeyEvent, source)
        { KeyEventArgs = keyEventArgs; }
    }
}