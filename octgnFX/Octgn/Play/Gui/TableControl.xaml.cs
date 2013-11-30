using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Octgn.Play.Actions;
using Octgn.Play.Gui.Adorners;
using Octgn.Play.Gui.DragOperations;
using Octgn.Extentions;

namespace Octgn.Play.Gui
{
    using System.IO;
    using System.Text;
    using Octgn.Annotations;
    using Octgn.Core;

    partial class TableControl : INotifyPropertyChanged
    {
        public double Angle
        {
            get
            {
                return this.angle;
            }
            set
            {
                if (value.Equals(this.angle))
                {
                    return;
                }
                this.angle = value;
                this.OnPropertyChanged("Angle");
            }
        }

        private readonly int _defaultHeight;
        private readonly int _defaultWidth;
        protected bool IsCardSizeValid;
        private Size _cardSize;
        private IDragOperation _dragOperation;

        public string ManipulationString
        {
            get
            {
                return this.manipulationString;
            }
            set
            {
                if (this.manipulationString == value) return;
                this.manipulationString = value;
                this.OnPropertyChanged("ManipulationString");
            }
        }

        public TableControl()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            var tableDef = Program.GameEngine.Definition.Table;
            var subbed = SubscriptionModule.Get().IsSubscribed ?? false;
            if(subbed && !String.IsNullOrWhiteSpace(Prefs.DefaultGameBack) && File.Exists(Prefs.DefaultGameBack))
            {
                SetBackground(Prefs.DefaultGameBack, "uniformToFill");
            }
            else
            {
                if (tableDef.Background != null) 
                    SetBackground(tableDef);
            }
            Program.GameEngine.BoardImage = tableDef.Board;
            //if (!Program.GameSettings.HideBoard)
            //    if (tableDef.Board != null)
            //        SetBoard(tableDef);

            if (!Program.GameSettings.UseTwoSidedTable)
                middleLine.Visibility = Visibility.Collapsed;

            if (Player.LocalPlayer.InvertedTable)
                transforms.Children.Insert(0, new ScaleTransform(-1, -1));

            _defaultWidth = Program.GameEngine.Definition.CardWidth;
            _defaultHeight = Program.GameEngine.Definition.CardHeight;
            SizeChanged += delegate
                               {
                                   IsCardSizeValid = false;
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
            Program.GameEngine.PropertyChanged += GameOnPropertyChanged;
            if (Player.LocalPlayer.InvertedTable)
            {
                var rotateAnimation = new DoubleAnimation(0, 180, TimeSpan.FromMilliseconds(1));
                var rt = (RotateTransform)NoteCanvas.RenderTransform;
                rt.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
            }
            //this.IsManipulationEnabled = true;
            //this.ManipulationDelta += OnManipulationDelta;

            Player.LocalPlayer.PropertyChanged += LocalPlayerOnPropertyChanged;
        }

        private void LocalPlayerOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "InvertedTable")
            {
                var p = sender as Player;
                if (p.InvertedTable)
                {
                    if (transforms.Children.OfType<ScaleTransform>().Any(x => x.ScaleX == -1 && x.ScaleY == -1) == false) transforms.Children.Insert(0, new ScaleTransform(-1, -1));
                    var rt = (RotateTransform)NoteCanvas.RenderTransform;
                    rt.Angle = 180;
                }
                else
                {
                    var sf = transforms.Children.OfType<ScaleTransform>().FirstOrDefault(x => x.ScaleX == -1 && x.ScaleY == -1);
                    if (sf != null) transforms.Children.Remove(sf);
                    var rt = (RotateTransform)NoteCanvas.RenderTransform;
                    rt.Angle = 0;
                }
            }
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            e.Handled = true;
            IsCardSizeValid = false;
            var sb = new StringBuilder();
            sb.AppendLine(Newtonsoft.Json.JsonConvert.SerializeObject(e.DeltaManipulation.Expansion));
            sb.AppendLine(Newtonsoft.Json.JsonConvert.SerializeObject(e.DeltaManipulation.Expansion.Length));
            
            ManipulationString = sb.ToString();
            Point center = e.ManipulationOrigin;
            double oldZoom = Zoom; // May be animated

            // Set the new zoom level
            //Zoom = oldZoom * e.DeltaManipulation.Scale.Length;
            if (e.DeltaManipulation.Expansion.X > 0)
                Zoom = oldZoom + 0.125;
            else if (oldZoom > 0.15)
                Zoom = oldZoom - 0.125;
            BeginAnimation(ZoomProperty, null); // Stop any animation, which could override the current zoom level

            // Adjust the offset to center the zoom on the mouse pointer
            double ratio = oldZoom - Zoom;
            if (Player.LocalPlayer.InvertedTable) ratio = -ratio;
            Offset += new Vector(center.X * ratio, center.Y * ratio);
            BeginAnimation(OffsetProperty, null); // Stop any animation, which could override the current Offset

            e.Handled = true;
            //base.OnManipulationDelta(e);
        }

        private void GameOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            if (propertyChangedEventArgs.PropertyName == "IsTableBackgroundFlipped")
            {
                var tableDef = Program.GameEngine.Definition.Table;
                this.SetBackground(tableDef);
            }
        }

        protected Size CardSize
        {
            get
            {
                if (!IsCardSizeValid)
                {
                    double scale = Math.Min(ActualWidth/Program.GameEngine.Definition.Table.Width,
                                            ActualHeight/Program.GameEngine.Definition.Table.Height);
                    scale *= Zoom;
                    _cardSize = new Size(Program.GameEngine.Definition.CardWidth*scale,
                                         Program.GameEngine.Definition.CardHeight*scale);
                    IsCardSizeValid = true;
                }
                return _cardSize;
            }
        }

        public int DefaultCardWidth
        {
            get { return _defaultWidth; }
        }

        public int DefaultCardHeight
        {
            get { return _defaultHeight; }
        }

        internal override ItemContainerGenerator GetItemContainerGenerator()
        {
            return cardsView.ItemContainerGenerator;
        }

        public void CenterView()
        {
            var tableDef = Program.GameEngine.Definition.Table;
            Offset = new Vector(tableDef.Width/2, tableDef.Height/2);
        }

        internal void ResetBackground()
        {
            if (Program.GameEngine.Definition.Table.Background != null)
                SetBackground(Program.GameEngine.Definition.Table);
        }

        internal void SetBackground(DataNew.Entities.Group tableDef)
        {
            SetBackground(tableDef.Background,tableDef.BackgroundStyle);
        }

        internal void SetBackground(string url, string bs)
        {
            var bim = new BitmapImage();
            bim.BeginInit();
            bim.CacheOption = BitmapCacheOption.OnLoad;
            bim.UriSource = new Uri(url);
            if(Program.GameEngine.IsTableBackgroundFlipped)bim.Rotation = Rotation.Rotate180;
            bim.EndInit();

            var backBrush = new ImageBrush(bim);
            if (!String.IsNullOrWhiteSpace(bs))
                switch (bs)
                {
                    case "tile":
                        backBrush.TileMode = TileMode.Tile;
                        backBrush.Viewport = new Rect(0, 0, backBrush.ImageSource.Width, backBrush.ImageSource.Height);
                        backBrush.ViewportUnits = BrushMappingMode.Absolute;
                        break;
                    case "uniform":
                        backBrush.Stretch = Stretch.Uniform;
                        break;
                    case "uniformToFill":
                        backBrush.Stretch = Stretch.UniformToFill;
                        break;
                    case "stretch":
                        backBrush.Stretch = Stretch.Fill;
                        break;
                }
            Background = backBrush;
        }

        //private void SetBoard(DataNew.Entities.Group tableDef)
        //{
        //    Rect pos = new Rect(tableDef.BoardPosition.X,tableDef.BoardPosition.Y,tableDef.BoardPosition.Width,tableDef.BoardPosition.Height);
        //    var img = new Image
        //                  {
        //                      Source = ExtensionMethods.BitmapFromUri(new Uri(tableDef.Board)),
        //                      Width = pos.Width,
        //                      Height = pos.Height,
        //                      HorizontalAlignment = HorizontalAlignment.Left,
        //                      VerticalAlignment = VerticalAlignment.Top,
        //                      Margin = new Thickness(pos.Left, pos.Top, 0, 0)
        //                  };
        //    boardContainer.Children.Insert(0, img);        
        //}

        public static bool IsInInvertedZone(double y)
        {
            return y < -Program.GameEngine.Definition.CardHeight/2;
        }

        private void CardMoved(object sender, EventArgs e)
        {
            var action = (MoveCard) sender;
            if (action.To == group)
                BringCardIntoView(action.Card);
        }

        private void CardCreated(object sender, EventArgs e)
        {
            var action = (CreateCard) sender;
            BringCardIntoView(action.Card);
        }

        protected override void OnCardOver(object sender, CardsEventArgs e)
        {
            base.OnCardOver(sender, e);
            e.CardSize = CardSize;

            if (!Program.GameSettings.UseTwoSidedTable) return;
            var cardCtrl = (CardControl) e.OriginalSource;
            Card baseCard = cardCtrl.Card;
            double mouseY = Mouse.GetPosition(cardsView).Y;
            double baseY = (cardCtrl.IsInverted ||
                            (Player.LocalPlayer.InvertedTable && !cardCtrl.IsOnTableCanvas))
                               ? mouseY - Program.GameEngine.Definition.CardHeight + e.MouseOffset.Y
                               : mouseY - e.MouseOffset.Y;
            if (baseCard == null) return;
            foreach (CardDragAdorner adorner in e.Adorners)
            {
                if (adorner == null || adorner.SourceCard == null || adorner.SourceCard.Card == null) continue;
                double y = baseY + adorner.SourceCard.Card.Y - baseCard.Y;
                adorner.OnHoverRequestInverted = IsInInvertedZone(y) ^ Player.LocalPlayer.InvertedTable;
            }
        }

        protected override void OnCardDropped(object sender, CardsEventArgs e)
        {
            e.Handled = e.CanDrop = true;
            var cardCtrl = e.OriginalSource as CardControl;

            int delta = Program.GameEngine.Definition.CardHeight - Program.GameEngine.Definition.CardWidth;
            Table table = Program.GameEngine.Table;
            Vector mouseOffset;
            if (cardCtrl != null && (cardCtrl.IsInverted ||
                                     (Player.LocalPlayer.InvertedTable && !cardCtrl.IsOnTableCanvas)))
                mouseOffset = new Vector(Program.GameEngine.Definition.CardWidth - e.MouseOffset.X, Program.GameEngine.Definition.CardHeight - e.MouseOffset.Y);
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
                    if (cardCtrl != null && (!cardCtrl.IsInverted && newPosInverted))
                    {
                        pt.X += delta;
                        pt.Y += delta;
                    }
                    else if (cardCtrl != null && (cardCtrl.IsInverted && !newPosInverted))
                    {
                        pt.X -= delta;
                        pt.Y -= delta;
                    }
                }

                int idx = table.GetCardIndex(e.ClickedCard);
                if (idx != -1 || table.Visibility != DataNew.Entities.GroupVisibility.Undefined)
                    e.FaceUp = e.ClickedCard.FaceUp;
                if (idx == -1)
                    idx = table.Cards.Count;
                e.ClickedCard.MoveToTable((int) pt.X, (int) pt.Y, e.FaceUp != null && e.FaceUp.Value, idx,false);

                // If there were other cards (i.e. dragging from a count number in GroupWindow), move them accordingly
                double xOffset = Program.GameEngine.Definition.CardWidth*1.05;
                foreach (Card c in e.Cards.Where(c => c != e.ClickedCard))
                {
                    pt.X += xOffset;
                    c.MoveToTable((int) pt.X, (int) pt.Y, e.FaceUp != null && e.FaceUp.Value, idx,false);
                }
            }
            else
            {
                // There are multiple cards, coming from the table. Offset them accordingly
                double dx = pt.X - e.ClickedCard.X;
                double dy = pt.Y - e.ClickedCard.Y;
                foreach (Card c in Selection.Cards)
                {
                    int x = (int) (c.X + dx), y = (int) (c.Y + dy);
                    int idx = table.GetCardIndex(c);
                    // If the card is tapped and has crossed the middle line in a two-sided table, we have to adjust its position
                    if (Program.GameSettings.UseTwoSidedTable && (c.Orientation & CardOrientation.Rot90) != 0)
                    {
                        bool oldPosInverted = IsInInvertedZone(c.Y);
                        bool newPosInverted = IsInInvertedZone(y);
                        if (!oldPosInverted && newPosInverted)
                        {
                            x += delta;
                            y += delta;
                        }
                        else if (oldPosInverted && !newPosInverted)
                        {
                            x -= delta;
                            y -= delta;
                        }
                    }
                    c.MoveToTable(x, y, c.FaceUp, idx,false);
                }
            }
        }

        private bool IsCardAtInvertedPosition(double y, CardEventArgs e)
        {
            //var cardCtrl = e.OriginalSource as CardControl;
            double offset = e.MouseOffset.Y;
            if (Player.LocalPlayer.InvertedTable)
            {
                y -= Program.GameEngine.Definition.CardHeight;
                offset = -offset;
            }
            y -= offset;
            return IsInInvertedZone(y);
        }

        public void AddNote(double x, double y)
        {
            AddNote(x, y, "");
        }

        public void AddNote(double x, double y, string message)
        {
            var nc = new NoteControl(message);
            NoteCanvas.Children.Add(nc);
            Canvas.SetLeft(nc, x);
            Canvas.SetTop(nc, y);
            var na = new NoteAdorner(nc);
            nc.Adorner = na;
            AdornerLayer.GetAdornerLayer(NoteCanvas).Add(na);
            if (Player.LocalPlayer.InvertedTable)
            {
                
            }
        }

        #region Mouse

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (!Keyboard.IsKeyDown(Key.Space)) return;
            _dragOperation = new Pan(this);
            e.Handled = true;
        }

        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            // Prevents popup menu from opening during DnD operations, which may result in e.g. stuck selection rectangles
            if (_dragOperation == null)
                base.OnPreviewMouseRightButtonUp(e);
            else
                e.Handled = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            _dragOperation = new SelectCards(this);
            e.Handled = true;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.MiddleButton == MouseButtonState.Pressed && _dragOperation == null)
            {
                _dragOperation = new Pan(this);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (_dragOperation != null)
                _dragOperation.Dragging(e);
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                case MouseButton.Middle:
                    if (_dragOperation != null)
                    {
                        _dragOperation.EndDrag();
                        _dragOperation = null;
                    }
                    e.Handled = true;
                    break;
            }
            base.OnMouseUp(e);
        }

        public Point ContextMenuNotesMousePosition;

        public Point MousePosition()    
        {
            return Mouse.GetPosition(cardsView);
        }

        #endregion

        #region Zoom and pan

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register("Zoom", typeof (double), typeof (TableControl),
                                        new PropertyMetadata(1.0, ZoomChanged));

        public static readonly DependencyProperty OffsetProperty =
            DependencyProperty.Register("Offset", typeof (Vector), typeof (TableControl),
                                        new PropertyMetadata(new Vector(), OffsetChanged));

        public static readonly DependencyProperty YCenterOffsetProperty =
            DependencyProperty.Register("YCenterOffset", typeof (double), typeof (TableControl));

        private DispatcherOperation _updateYCenterOperation;

        public double Zoom
        {
            get { return (double) GetValue(ZoomProperty); }
            set { SetValue(ZoomProperty, value); }
        }

        public Vector Offset
        {
            get { return (Vector) GetValue(OffsetProperty); }
            set { SetValue(OffsetProperty, value); }
        }

        public double YCenterOffset
        {
            get { return (double) GetValue(YCenterOffsetProperty); }
            set { SetValue(YCenterOffsetProperty, value); }
        }

        protected static void ZoomChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((TableControl) sender).ZoomChanged();
        }

        protected void ZoomChanged()
        {
            IsCardSizeValid = false;
            UpdateYCenterOffset();
        }

        protected static void OffsetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((TableControl) sender).OffsetChanged();
        }

        protected void OffsetChanged()
        {
            UpdateYCenterOffset();
        }

        protected void UpdateYCenterOffset()
        {
            // Don't queue the update multiple times (could happen e.g. when updating the Offset and then the Zoom property)
            if (_updateYCenterOperation != null && _updateYCenterOperation.Status == DispatcherOperationStatus.Pending)
                return;

            // Bug fix: if done immediately, the layout is slightly incorrect (e.g. in the case of mouse wheel zoom).
            // so we dispatch the update until all transforms are updated.         
            _updateYCenterOperation = Dispatcher.BeginInvoke(new Action(delegate
                                                                            {
                                                                                Point pt =
                                                                                    cardsView.TransformToAncestor(this).
                                                                                        Transform(new Point());
                                                                                YCenterOffset = pt.Y;
                                                                            }), DispatcherPriority.ContextIdle);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;
            IsCardSizeValid = false;

            Point center = e.GetPosition(cardsView);
            double oldZoom = Zoom; // May be animated

            // Set the new zoom level
            if (e.Delta > 0)
                Zoom = oldZoom + 0.125;
            else if (oldZoom > 0.15)
                Zoom = oldZoom - 0.125;
            BeginAnimation(ZoomProperty, null); // Stop any animation, which could override the current zoom level

            // Adjust the offset to center the zoom on the mouse pointer
            double ratio = oldZoom - Zoom;
            if (Player.LocalPlayer.InvertedTable) ratio = -ratio;
            Offset += new Vector(center.X*ratio, center.Y*ratio);
            BeginAnimation(OffsetProperty, null); // Stop any animation, which could override the current Offset

            base.OnMouseWheel(e);
        }

        #endregion

        #region Table extents

        private double _logicalRatio;
        private Size _stretchMargins;

        protected void AspectRatioChanged()
        {
            if (Program.GameEngine == null) return; // Bug fix: Program.Game can be null when closing the game window.

            int tWidth = Program.GameEngine.Table.Definition.Width, tHeight = Program.GameEngine.Table.Definition.Height;
            double wRatio = ActualWidth/tWidth, hRatio = ActualHeight/tHeight;

            if (wRatio < hRatio)
            {
                _logicalRatio = wRatio;
                _stretchMargins.Width = 0;
                _stretchMargins.Height = Math.Max((ActualHeight/wRatio - tHeight)/2, 0);
            }
            else
            {
                _logicalRatio = hRatio;
                _stretchMargins.Width = Math.Max((ActualWidth/_logicalRatio - tWidth)/2, 0);
                _stretchMargins.Height = 0;
            }

            UpdateYCenterOffset();
        }

        protected void BringCardIntoView(Card card)
        {
            // Get the current target viewport (bypass animations in progress)
            GeneralTransform transform = TransformToDescendant(cardsView);
            Rect visibleBounds = transform.TransformBounds(new Rect(0, 0, ActualWidth, ActualHeight));

            var cardRect = new Rect(card.X, card.Y, Program.GameEngine.Definition.CardWidth,
                                    Program.GameEngine.Definition.CardHeight);
            if (visibleBounds.Contains(cardRect)) return; // okay, already completely into view

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
                (Program.GameEngine.Table.Definition.Width + 2*_stretchMargins.Width)/newBounds.Width,
                (Program.GameEngine.Table.Definition.Height + 2*_stretchMargins.Height)/newBounds.Height);
            var newOffset = new Vector(
                -_stretchMargins.Width - newBounds.X*newZoom,
                -_stretchMargins.Height - newBounds.Y*newZoom);

            // Combine new values with the current ones 
            // (bypassing animations, e.g. when moving several cards outside the bounds at the same time
            var realZoom = (double) GetAnimationBaseValue(ZoomProperty);
            //var realOffset = (Vector)GetAnimationBaseValue(OffsetProperty);
            if (newZoom > realZoom) newZoom = realZoom;
            //if (newOffset.X < realOffset.X) newOffset.X = realOffset.X;
            //if (newOffset.Y < realOffset.Y) newOffset.Y = realOffset.Y;

            double oldZoom = Zoom;
            Vector oldOffset = Offset;
            Zoom = newZoom;
            Offset = newOffset;
            // Animate the table with the result
            AnimationTimeline anim = new DoubleAnimation
                                         {
                                             From = oldZoom,
                                             To = newZoom,
                                             Duration = TimeSpan.FromMilliseconds(150),
                                             FillBehavior = FillBehavior.Stop
                                         };
            BeginAnimation(ZoomProperty, anim);
            anim = new VectorAnimation
                       {
                           From = oldOffset,
                           To = newOffset,
                           Duration = TimeSpan.FromMilliseconds(150),
                           FillBehavior = FillBehavior.Stop
                       };
            BeginAnimation(OffsetProperty, anim);

            // Note: the new visibleBounds may end up bigger than newBounds, 
            // because the window aspect ratio may be different
        }

        #endregion

        #region Focus and keyboard

        public static readonly RoutedEvent TableKeyEvent = EventManager.RegisterRoutedEvent("TableKey",
                                                                                            RoutingStrategy.Bubble,
                                                                                            typeof (
                                                                                                EventHandler
                                                                                                <TableKeyEventArgs>),
                                                                                            typeof (TableControl));

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
            if (e.Key == Key.F1 && Program.GameEngine.Definition.Id.ToInt() == 1803)
            {
                Program.GameEngine.GlobalVariables.Clear();
                //var task = new Task(
                //    () =>
                //    {
                //        for (int i = 0; i < 360; i++)
                //        {
                //            Angle = i;
                //            Thread.Sleep(1);
                //        }
                //        Angle = 0;
                //    });
                //task.ContinueWith(
                //    (x) =>
                //    {
                //        Dispatcher.BeginInvoke(new Action(
                //            () => this.SetBackground(Program.GameEngine.Definition.Table)));
                //    });
                //task.Start();
            }
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

        protected Point ContextMenuPosition;

        private string manipulationString;

        private double angle;

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            ContextMenuNotesMousePosition = Mouse.GetPosition(NoteCanvas);
            ContextMenuPosition = MousePosition();
            base.OnContextMenuOpening(e);
        }

        internal override void ShowContextMenu(Card card, bool showGroupActions = true)
        {
            ContextMenuNotesMousePosition = Mouse.GetPosition(NoteCanvas);
            ContextMenuPosition = MousePosition();
            base.ShowContextMenu(card, card == null); // Don't show group actions when a card is selected on table
        }

        protected override List<Control> CreateCardMenuItems(DataNew.Entities.Group def)
        {
            List<Control> items = base.CreateCardMenuItems(def);

            var item = new MenuItem {Header = "Move to"};
            var subItem = new MenuItem
                              {
                                  Header = Program.GameEngine.Definition.Player.Hand.Name,
                                  InputGestureText = Program.GameEngine.Definition.Player.Hand.Shortcut
                              };
            subItem.Click += delegate { Selection.Do(c => c.MoveTo(Player.LocalPlayer.Hand, true,false), ContextCard); };
            item.Items.Add(subItem);
            var groupDefs = Program.GameEngine.Definition.Player.Groups.ToArray();
            var moveToBottomItems = new List<MenuItem>();
            for (int i = 0; i < groupDefs.Length; ++i)
            {
                var groupDef = groupDefs[i];
                Group indexedGroup = Player.LocalPlayer.IndexedGroups[i + 1]; // 0 is hand
                subItem = new MenuItem {Header = groupDef.Name, InputGestureText = groupDef.Shortcut};
                subItem.Click += delegate { Selection.Do(c => c.MoveTo(indexedGroup, true,false), ContextCard); };
                item.Items.Add(subItem);
                subItem = new MenuItem
                              {
                                  Header = string.Format("Bottom of {0}", groupDef.Name),
                                  InputGestureText =
                                      string.IsNullOrEmpty(groupDef.Shortcut) ? null : "Alt+" + groupDef.Shortcut
                              };
                subItem.Click +=
                    delegate { Selection.Do(c => c.MoveTo(indexedGroup, true, indexedGroup.Count,false), ContextCard); };
                moveToBottomItems.Add(subItem);
            }
            if (moveToBottomItems.Count > 0) item.Items.Add(new Separator());
            foreach (MenuItem x in moveToBottomItems) item.Items.Add(x);
            items.Add(item);

            item = new MenuItem {Header = "Bring to front", InputGestureText = "PgUp"};
            item.Click += delegate { Selection.Do(c => Program.GameEngine.Table.BringToFront(c), ContextCard); };
            items.Add(item);

            item = new MenuItem {Header = "Send to back", InputGestureText = "PgDn"};
            item.Click += delegate { Selection.Do(c => Program.GameEngine.Table.SendToBack(c), ContextCard); };
            items.Add(item);            

            return items;
        }

        protected override void GroupActionClicked(object sender, RoutedEventArgs e)
        {
            var action = (DataNew.Entities.GroupAction) ((MenuItem) sender).Tag;
            if (action.Execute != null)
                ScriptEngine.ExecuteOnGroup(action.Execute, group, ContextMenuPosition);
        }

        protected override void CardActionClicked(object sender, RoutedEventArgs e)
        {
            var action = (DataNew.Entities.GroupAction)((MenuItem)sender).Tag;
            if (action.Execute != null)
                ScriptEngine.ExecuteOnCards(action.Execute, Selection.ExtendToSelection(ContextCard),
                                            ContextMenuPosition);
            else if (action.BatchExecute != null)
                ScriptEngine.ExecuteOnBatch(action.BatchExecute, Selection.ExtendToSelection(ContextCard),
                                            ContextMenuPosition);
        }

        #endregion

        #region Drag operations

        #region Nested type: Pan

        private sealed class Pan : DragOperation<TableControl>
        {
            public Pan(TableControl table)
                : base(table)
            {
            }

            protected override void StartDragCore(Point position)
            {
            }

            protected override void DraggingCore(Point position, Vector delta)
            {
                Target.Offset += delta/Target._logicalRatio;
                Target.BeginAnimation(OffsetProperty, null);
                // Stop any animation, which could override the current Offset
            }

            protected override void EndDragCore()
            {
            }
        }

        #endregion

        #region Nested type: SelectCards

        private sealed class SelectCards : DragOperation<TableControl>
        {
            private SelectAdorner _adorner;
            private Point _fromPt;

            public SelectCards(TableControl table)
                : base(table)
            {
            }

            protected override void StartDragCore(Point position)
            {
                _fromPt = position;
                _adorner = new SelectAdorner(Target) {Rectangle = new Rect(position, position)};
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(Target);
                layer.Add(_adorner);
            }

            protected override void DraggingCore(Point position, Vector delta)
            {
                // Update adorner
                var rect = new Rect(_fromPt, position);
                _adorner.Rectangle = rect;

                // Get position in table space
                GeneralTransform transform = Target.TransformToDescendant(Target.cardsView);
                rect = transform.TransformBounds(rect);
                if (Program.GameEngine == null) return; //Means that the game has ended and the user hasn't gotten over it yet.
                int width = Program.GameEngine.Definition.CardWidth;
                int height = Program.GameEngine.Definition.CardHeight;

                // Remove cards which are not in the selection anymore
                Selection.RemoveAll(card => !rect.IntersectsWith(ComputeCardBounds(card, width, height)));

                // Add cards which entered the selection rectangle
                ObservableCollection<Card> allCards = Target.group.Cards;
                IEnumerable<Card> cards = from card in allCards
                                          where card.Controller == Player.LocalPlayer
                                                && !card.Selected
                                                && rect.IntersectsWith(ComputeCardBounds(card, width, height))
                                          select card;
                foreach (Card card in cards) Selection.Add(card);
            }

            protected override void EndDragCore()
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(Target);
                layer.Remove(_adorner);
            }

            private static Rect ComputeCardBounds(Card c, int w, int h)
            {
                Rect result =
                    // Case 1: straight card
                    (c.Orientation & CardOrientation.Rot90) == 0
                        ? new Rect(c.X, c.Y, w, h)
                        : // Case 2: rotated card on a 1-sided table
                    !Program.GameSettings.UseTwoSidedTable
                        ? new Rect(c.X, c.Y + h - w, h, w)
                        : // Case 3: rotated card on a 2-sided table, the card is not inversed
                    !IsInInvertedZone(c.Y)
                        ? new Rect(c.X, c.Y + h - w, h, w)
                        : // Case 4: rotated and inversed card on a 2-sided table
                    new Rect(c.X - h + w, c.Y, h, w);
                return result;
            }
        }

        #endregion

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void TableControl_OnManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this.adDecorator;
        }
    }

    public class TableKeyEventArgs : RoutedEventArgs
    {
        public readonly KeyEventArgs KeyEventArgs;

        public TableKeyEventArgs(object source, KeyEventArgs keyEventArgs)
            : base(TableControl.TableKeyEvent, source)
        {
            KeyEventArgs = keyEventArgs;
        }
    }
}