using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Octgn.Play.Gui.Adorners;
using Octgn.Scripting;
using Octgn.Utils;

namespace Octgn.Play.Gui
{
    using System.Reflection;

    using Octgn.Core.DataExtensionMethods;

    using log4net;

    public partial class CardControl
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


#pragma warning disable 649   // Unassigned variable: it's initialized by MEF

        [Import]
        protected Engine ScriptEngine;

#pragma warning restore 649

        #region Dependency properties

        public static readonly DependencyProperty DisplayedPictureProperty =
            DependencyProperty.Register("DisplayedPicture", typeof(ImageSource), typeof(CardControl));

        public static readonly DependencyProperty IsUpProperty = DependencyProperty.Register("IsUp", typeof(bool),
                                                                                             typeof(CardControl),
                                                                                             new PropertyMetadata(
                                                                                                 IsUpChanged));

        public static readonly DependencyProperty IsAlwaysUpProperty = DependencyProperty.Register("IsAlwaysUp",
                                                                                                   typeof(bool),
                                                                                                   typeof(CardControl),
                                                                                                   new PropertyMetadata(
                                                                                                       IsAlwaysUpChanged));

        private static readonly DependencyPropertyKey IsInvertedPropertyKey =
            DependencyProperty.RegisterReadOnly("IsInverted", typeof(bool), typeof(CardControl),
                                                new PropertyMetadata());

        public static readonly DependencyProperty IsInvertedProperty = IsInvertedPropertyKey.DependencyProperty;

        public static readonly DependencyProperty MultipleCardsProperty = DependencyProperty.Register("MultipleCards",
                                                                                                      typeof(
                                                                                                          ReadOnlyObservableCollection
                                                                                                          <object>),
                                                                                                      typeof(
                                                                                                          CardControl));

        public static readonly DependencyProperty AnimateLoadProperty =
            DependencyProperty.RegisterAttached("AnimateLoad", typeof(bool), typeof(CardControl),
                                                new FrameworkPropertyMetadata(true,
                                                                              FrameworkPropertyMetadataOptions.Inherits));

        public static bool GetAnimateLoad(DependencyObject obj)
        {
            return (bool)obj.GetValue(AnimateLoadProperty);
        }

        public static void SetAnimateLoad(DependencyObject obj, bool value)
        {
            obj.SetValue(AnimateLoadProperty, value);
        }

        #endregion

        private readonly Window _mainWin;
        private ScaleTransform _invertTransform;

        public CardControl()
        {
            InitializeComponent();
            if (mouseClickHandler == null)
                mouseClickHandler = new MouseClickHandler(
                    this.Dispatcher,
                    MouseButtonUpAction,
                    MouseButtonDoubleClickAction);
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            Program.GameEngine.ComposeParts(this);

            //fix MAINWINDOW bug
            _mainWin = WindowManager.PlayWindow;
            int markerSize = Program.GameEngine.Definition.MarkerSize;
            if (markerSize == 0) markerSize = 20;
            markers.Margin = new Thickness(markerSize / 8);
            peekEyeIcon.Width = peekers.MinHeight = markerSize;
            peekers.SetValue(TextBlock.FontSizeProperty, markerSize * 0.8);
            if (Program.GameEngine.Definition.CardCornerRadius > 0)
                img.Clip = new RectangleGeometry();
            AddHandler(MarkerControl.MarkerDroppedEvent, new EventHandler<MarkerEventArgs>(MarkerDropped));
            AddHandler(TableControl.TableKeyEvent, new EventHandler<TableKeyEventArgs>(TableKeyDown));
            DataContextChanged += CardChangedHandler;
            Unloaded += RemoveCardHandler;
            Loaded += RestoreCardHandler;
            Loaded += AnimateLoad;
            SizeChanged += delegate
                               {
                                   // if only height is set, set width accordingly so that slight differences in scans size
                                   // don't propagate to the layout
                                   if (double.IsNaN(Width) && !double.IsNaN(Height))
                                   {
                                       Width = Program.GameEngine.Definition.CardWidth * Height / Program.GameEngine.Definition.CardHeight;
                                   }
                                   target.Height = target.Width = Math.Min(Height, Width);
                                   peekers.Margin = new Thickness(ActualWidth - 1, 8, -200, 0);
                               };
        }

        public bool IsInverted
        {
            get { return (bool)GetValue(IsInvertedProperty); }
            private set { SetValue(IsInvertedPropertyKey, value); }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            IsOnTableCanvas = IsInverted = false;
            if (_invertTransform != null)
            {
                RenderTransform = null;
                _invertTransform = null;
            }
            DependencyObject iter = this;
            while (iter != null)
            {
                if (iter is TableCanvas)
                {
                    IsOnTableCanvas = true;
                    if (Program.GameSettings.UseTwoSidedTable)
                    {
                        _invertTransform = new ScaleTransform();
                        UpdateInvertedTransform();
                        RenderTransform = _invertTransform;
                        RenderTransformOrigin = new Point(0.5, 0.5);
                    }
                    break;
                }
                iter = VisualTreeHelper.GetParent(iter);
            }
        }

        public bool IsAlwaysUp
        {
            get { return (bool)GetValue(IsAlwaysUpProperty); }
            set { SetValue(IsAlwaysUpProperty, value); }
        }

        protected bool IsUp
        {
            get { return (bool)GetValue(IsUpProperty); }
            set { SetValue(IsUpProperty, value); }
        }

        public ReadOnlyObservableCollection<object> MultipleCards
        {
            get { return (ReadOnlyObservableCollection<object>)GetValue(MultipleCardsProperty); }
            set { SetValue(MultipleCardsProperty, value); }
        }

        public ImageSource DisplayedPicture
        {
            get { return (ImageSource)GetValue(DisplayedPictureProperty); }
            private set { SetValue(DisplayedPictureProperty, value); }
        }

        private void SetDisplayedPicture(string value)
        {
            if (value == null)
            {
                DisplayedPicture = null;
                return;
            }

            // Shortcut: always reuse the same bitmap images for default face up and down
            if (value == Card.DefaultFront)
            {
                DisplayedPicture = Program.GameEngine.CardFrontBitmap;
                return;
            }
            if (value == Card.DefaultBack)
            {
                DisplayedPicture = Program.GameEngine.CardBackBitmap;
                return;
            }

            ImageUtils.GetCardImage(new Uri(value), x => DisplayedPicture = x);
        }

        private static void IsUpChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = (CardControl)sender;
            if (ctrl.Card != null)
                ctrl.SetDisplayedPicture(ctrl.Card.GetPicture((bool)e.NewValue));
        }

        private static void IsAlwaysUpChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var cardCtrl = (CardControl)sender;
            cardCtrl.IsUp = (bool)e.NewValue || cardCtrl.Card != null && cardCtrl.Card.FaceUp;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            img.Measure(constraint);
            if (img.Clip != null)
            {
                var clipRect = ((RectangleGeometry)img.Clip);
                clipRect.Rect = new Rect(img.DesiredSize);
                clipRect.RadiusX = clipRect.RadiusY = Program.GameEngine.Definition.CardCornerRadius * clipRect.Rect.Height / Program.GameEngine.Definition.CardHeight;
            }
            return img.DesiredSize;
        }

        #region Card changes and animation

        public Card Card { get; private set; }

        private void CardChangedHandler(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Card != null)
                Card.PropertyChanged -= PropertyChangeHandler;

            Card = DataContext as Card;

            if (Card == null) return;
            bool oldIsUp = IsUp;
            IsUp = IsAlwaysUp || Card.FaceUp;
            // If IsUp changes, it automatically updates the picture. 
            // Otherwise do it explicitely
            if (oldIsUp == IsUp)
                SetDisplayedPicture(Card.GetPicture(IsUp));
            AnimateOrientation(Card.Orientation);
            UpdateInvertedTransform();
            Card.PropertyChanged += PropertyChangeHandler;
        }

        private void RemoveCardHandler(object sender, RoutedEventArgs e)
        {
            if (Card == null) return;

            // Three cases are possible.
            // 1. The container is unloaded (e.g. one closes a GroupWindow). 
            //    This case is recognizable because GroupControl.IsLoaded is false.
            //    We have to remove our listeners.
            // 2. The card is been moved to another group.
            //    This is recognizable because GroupControl is null.
            //    We have to remove our listeners.
            // 3. The card index changes (e.g. it's moved to top/bottom of a pile).
            //    In this case WPF seems to unload and then reload the control at the correct position.
            //    But in some weird cases WPF seems to call Unload WITHOUT a matching Load, 
            //    although the control ends up in the visual tree. E.g. when Moving several cards to the 
            //    top of a pile at once, with the GroupWindow open.
            //    We can recognize this case because GroupControl.IsLoaded is true.
            //    In this case we *keep* the listeners attached!			
            GroupControl groupCtrl = GroupControl;
            if (groupCtrl != null && groupCtrl.IsLoaded) return;

            Card.PropertyChanged -= PropertyChangeHandler;
            img = null;
            this.DisplayedPicture = null;
            Card = null;
        }

        private static void RestoreCardHandler(object sender, RoutedEventArgs e)
        {
            // Fix: This code is called during Loaded event and is useful if WPF
            // detaches and then re-attaches the control, e.g. when we change Z-order
            // by calling Move on an ObservableCollection.
            /*	if (_card == null)
              {
                _card = DataContext as Card;
                if (_card != null) _card.PropertyChanged += PropertyChangeHandler;
              }*/
        }

        private void PropertyChangeHandler(object sender, PropertyChangedEventArgs e)
        {
            if (Card.Group == null) return;
            switch (e.PropertyName)
            {
                case "Orientation":
                    AnimateOrientation(Card.Orientation);
                    break;
                case "FaceUp":
                    if (!IsAlwaysUp)
                        AnimateTurn(Card.FaceUp);
                    break;
                case "Picture":
                    if (IsUp) SetDisplayedPicture(Card.GetPicture(true));
                    break;
                case "Y":
                    UpdateInvertedTransform();
                    break;
            }
        }

        private void UpdateInvertedTransform()
        {
            if (_invertTransform == null || Card == null) return;
            IsInverted = TableControl.IsInInvertedZone(Card.Y);
            _invertTransform.ScaleX = _invertTransform.ScaleY = IsInverted ? -1 : 1;
        }

        private void AnimateLoad(object sender, RoutedEventArgs e)
        {
            // Fix: Card may sometime be null (e.g. moving a card from Top X window to the bottom of the same pile)
            if (Card == null) return;
            if (!IsAlwaysUp || Card.FaceUp || !GetAnimateLoad(this)) return;
            IsUp = false;
            AnimateTurn(true);
            SetAnimateLoad(this, false);
        }

        private void AnimateOrientation(CardOrientation newOrientation)
        {
            double target90 = (newOrientation & CardOrientation.Rot90) != 0 ? 90 : 0;
            double target180 = (newOrientation & CardOrientation.Rot180) != 0 ? 180 : 0;
            if (Math.Abs(target90 - rotate90.Angle) > double.Epsilon)
            {
                var anim = new DoubleAnimation(target90, TimeSpan.FromMilliseconds(300), FillBehavior.HoldEnd) { EasingFunction = new ExponentialEase() };
                rotate90.BeginAnimation(RotateTransform.AngleProperty, anim);
            }
            if (Math.Abs(target180 - rotate180.Angle) <= double.Epsilon) return;
            var animation = new DoubleAnimation(target180, TimeSpan.FromMilliseconds(600), FillBehavior.HoldEnd) { EasingFunction = new ExponentialEase() };
            rotate180.BeginAnimation(RotateTransform.AngleProperty, animation);
        }

        private void AnimateTurn(bool newIsUp)
        {
            TimeSpan delay = TimeSpan.Zero;
            GroupControl group = GroupControl;
            if (group != null)
                delay = TimeSpan.FromMilliseconds(group.GetTurnAnimationDelay());
            var animY = new DoubleAnimation(1.1, new Duration(TimeSpan.FromMilliseconds(150)), FillBehavior.HoldEnd) { BeginTime = delay };
            var anim = new DoubleAnimation(0, new Duration(TimeSpan.FromMilliseconds(150)), FillBehavior.HoldEnd) { BeginTime = delay };
            anim.Completed += Turned;
            turn.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
            turn.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        }

        protected void Turned(object sender, EventArgs e)
        {
            if (Card == null) return;

            IsUp = Card.FaceUp || IsAlwaysUp;
            var animY = new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(150)), FillBehavior.Stop);
            var anim = new DoubleAnimation(1, new Duration(TimeSpan.FromMilliseconds(150)), FillBehavior.Stop);
            turn.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
            turn.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
        }

        #endregion

        #region Drag and Drop

        public static readonly RoutedEvent CardOutEvent = EventManager.RegisterRoutedEvent("CardOut",
                                                                                           RoutingStrategy.Bubble,
                                                                                           typeof(CardsEventHandler),
                                                                                           typeof(CardControl));

        public static readonly RoutedEvent CardOverEvent = EventManager.RegisterRoutedEvent("CardOver",
                                                                                            RoutingStrategy.Bubble,
                                                                                            typeof(CardsEventHandler),
                                                                                            typeof(CardControl));

        public static readonly RoutedEvent CardDroppedEvent = EventManager.RegisterRoutedEvent("CardDropped",
                                                                                               RoutingStrategy.Bubble,
                                                                                               typeof(CardsEventHandler
                                                                                                   ),
                                                                                               typeof(CardControl));

        // One can only drag one card at a time -> make everything static. It reduces memory usage.
        private enum DragSource
        {
            None,
            Card,
            Marker,
            Target
        };

        private static DragSource _dragSource = DragSource.None;
        private static Point _mousePt; // mousePt is useful to determine the mouse pointer offset inside the card

        private static Point _mouseWindowPt;
        // mouesWindowPt should be use to determine movement, because the card origin itself may move (e.g. scroll in a list)

        private static ArrowAdorner _draggedArrow;
        private static bool _isDragging, _isOverCount;
        private static IInputElement _lastDragTarget;

        private static readonly List<Card> DraggedCards = new List<Card>(4);
        private static readonly List<CardDragAdorner> OverlayElements = new List<CardDragAdorner>(4);
        private static Vector _mouseOffset;
        internal static Size ScaleFactor;

        protected void LeftButtonDownOverImage(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            //if (_isDragging) return;
            _isOverCount = false;
            _isDragging = false;
            if (Card == null) return;
            if (!Card.Selected) Selection.Clear();
            _mousePt = e.GetPosition(this);
            Window window = Window.GetWindow(this);
            if (window != null)
                _mouseWindowPt = TranslatePoint(_mousePt, (UIElement)window.Content);
            _dragSource = Keyboard.Modifiers == ModifierKeys.Shift ? DragSource.Target : DragSource.Card;
            CaptureMouse();
        }

        protected void LeftButtonDownOverCount(object sender, MouseEventArgs e)
        {
            LeftButtonDownOverImage(sender, e);
            _isOverCount = true;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (Card == null)
            {
                return;
            }

            // Clear or modify selection
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                if (Program.GameEngine == null || Program.GameEngine.Table == null)
                {
                    return;
                }

                // Add/Remove from selection (currently only on table and hand)
                if (Card.Group == Program.GameEngine.Table || Card.Group is Hand)
                {
                    if (Card.Selected)
                    {
                        mouseClickHandler.AutoFireNext();
                        Selection.Remove(Card);
                    }
                    else if (Card.Controller == Player.LocalPlayer)
                    {
                        mouseClickHandler.AutoFireNext();
                        Selection.Add(Card);
                    }
                }

                e.Handled = true;
            }
            else
            {
                if (!Card.Selected)
                {
                    Selection.Clear();
                }
            }

            // Targetting is always allowed
            if (Keyboard.Modifiers == ModifierKeys.Shift && img.IsMouseDirectlyOver)
            {
                mouseClickHandler.AutoFireNext();
                return;
            }

            // otherwise check controlship
            if (!Card.TryToManipulate())
            {
                //e.Handled = true;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (Card == null) return;
            if (Card.Controller != Player.LocalPlayer) return;
            base.OnMouseMove(e);
            e.Handled = true;
            Point windowPt = e.GetPosition(Window.GetWindow(this));
            Point pt = e.GetPosition(this);
            if (!_isDragging)
            {
                // Check if the button was pressed over the card, and was not release on another control in the meantime 
                // (possible if the cursor is near the border of the card)
                if (Mouse.LeftButton == MouseButtonState.Pressed &&
                    // Check if has moved enough to start a drag and drop
                    (Math.Abs(windowPt.X - _mouseWindowPt.X) > SystemParameters.MinimumHorizontalDragDistance ||
                     Math.Abs(windowPt.Y - _mouseWindowPt.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    if (_dragSource == DragSource.Card)
                    {
                        DragCardStarted();
                    }
                    // Fix: Card could be null if a keyboard shortut was used when the mouse button was pressed.
                    else if (_dragSource == DragSource.Target && Card != null && Card.Group is Table)
                    {
                        _isDragging = true;
                        RaiseEvent(new CardEventArgs(CardHoveredEvent, this));
                        AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
                        _draggedArrow = new ArrowAdorner(Player.LocalPlayer, this);
                        layer.Add(_draggedArrow);
                    }
                }
            }
            else
            {
                switch (_dragSource)
                {
                    case DragSource.Card:
                        DragMouseDelta(windowPt.X - _mouseWindowPt.X, windowPt.Y - _mouseWindowPt.Y);
                        break;
                    case DragSource.Target:
                        DragTargetDelta(pt);
                        break;
                }
            }
        }

        private readonly MouseClickHandler mouseClickHandler;

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            mouseClickHandler.OnMouseUp(e);
        }

        private void MouseButtonUpAction(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            var shouldFireEvent = true;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (IsMouseCaptured) ReleaseMouseCapture();

                    if (_dragSource == DragSource.Card)
                    {
                        shouldFireEvent = false;
                        e.Handled = true;
                        _dragSource = DragSource.None;
                        if (!_isDragging)
                        {
                            Program.GameEngine.EventProxy.OnCardClick(Card, (int)e.ChangedButton, downKeys);
                        }
                        DragCardCompleted();
                        break;
                    }

                    if (_dragSource == DragSource.Target)
                    {
                        shouldFireEvent = false;
                        e.Handled = true;
                        _dragSource = DragSource.None;
                        if (_draggedArrow != null)
                        {
                            _draggedArrow.RemoveFromLayer();
                            _draggedArrow = null;
                        }

                        if (_isDragging)
                        {
                            _isDragging = false;
                        }

                        var dependencyObject = Mouse.DirectlyOver as DependencyObject;
                        while (dependencyObject != null && !(dependencyObject is CardControl))
                        {
                            DependencyObject parent = LogicalTreeHelper.GetParent(dependencyObject) ??
                                                      VisualTreeHelper.GetParent(dependencyObject);
                            dependencyObject = parent;
                        }

                        if (dependencyObject == this)
                            Card.ToggleTarget();
                        else if (dependencyObject != null && ((CardControl)dependencyObject).Card.Group is Table)
                            Card.Target(((CardControl)dependencyObject).Card);
                    }
                    break;
            }
            if (shouldFireEvent)
                Program.GameEngine.EventProxy.OnCardClick(Card, (int)e.ChangedButton, downKeys);
        }

        private void MouseButtonDoubleClickAction(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            // Double-click ends any manipulation which may be in progress. 
            // Otherwise bugs may happen (e.g. if the default action moves the card)
            if (IsMouseCaptured) ReleaseMouseCapture();
            _dragSource = DragSource.None;


            Program.GameEngine.EventProxy.OnCardDoubleClick(Card, (int)e.ChangedButton, downKeys);
            if (e.ChangedButton == MouseButton.Left)
            {
                e.Handled = true;
                if (GroupControl != null) GroupControl.ExecuteDefaultAction(Card);
            }
        }

        protected void DragCardStarted()
        {
            DraggedCards.Clear();
            // in theory draggedCards should already be empty, but it's better to recover if there was a problem during last DnD						
            if (!Selection.IsEmpty())
                DraggedCards.AddRange(Selection.Cards);
            else if (_isOverCount)
                DraggedCards.AddRange(MultipleCards.Cast<Card>());
            else if (Card != null)
                DraggedCards.Add(Card);
            else
                return;
            // Fix: Card == null can occur, e.g. hold the mouse down but don't move, dismiss the card with a keyboard shortcut (e.g. Delete) and move the mouse after that (with the left button still down).

            _isDragging = true;
            mouseClickHandler.AutoFireNext();

            // Keep control of the card and the group it's in
            foreach (Card c in DraggedCards) c.KeepControl();
            Card.Group.KeepControl();

            // Hides the card view
            RaiseEvent(new CardEventArgs(CardHoveredEvent, this));

            // Starts the drag-and-drop            
            ScaleFactor = TransformToAncestor(_mainWin).TransformBounds(new Rect(0, 0, 1, 1)).Size;
            //bool rot90 = (Card.Orientation & CardOrientation.Rot90) != 0;
            _mouseOffset =
                new Vector(_mousePt.X * Program.GameEngine.Definition.CardWidth / ActualWidth,
                           _mousePt.Y * Program.GameEngine.Definition.CardHeight / ActualHeight);

            // Create adorners
            var mwn = _mainWin.Content as Visual;
            AdornerLayer layer = null;
            if (mwn != null)
                layer = AdornerLayer.GetAdornerLayer(mwn);
            double offset = 0;
            double step = ActualWidth * 1.05;
            // HACK: if the selected card is in HandControl, its ContentPresenter has a RenderTransform, 
            // which we must account for
            if (GroupControl is HandControl)
            {
                var parent = VisualTreeHelper.GetParent(this) as ContentPresenter;
                if (parent != null)
                    step = 1.05 * parent.RenderTransform.TransformBounds(new Rect(0, 0, ActualWidth, 0)).Width;
            }
            foreach (CardControl cardCtrl in Selection.GetCardControls(GroupControl, this))
            {
                // Create an adorner                
                if (Card.Group is Table)
                {
                    var overlay = new CardDragAdorner(cardCtrl, _mouseOffset);
                    OverlayElements.Add(overlay);
                    if (layer != null) layer.Add(overlay);
                }
                else
                {
                    // If there are multiple cards but they don't come from the table, 
                    // layout the adorners properly
                    var overlay = new CardDragAdorner(this, cardCtrl, _mouseOffset);
                    OverlayElements.Add(overlay);
                    if (cardCtrl != this)
                    {
                        offset += step;
                        overlay.OffsetBy(offset, 0);
                        overlay.CollapseTo(0, 0, false);
                    }
                    if (layer != null) layer.Add(overlay);
                }

                // Make the card translucent
                cardCtrl.Opacity = 0.4;
            }
            if (!Selection.IsEmpty() || !_isOverCount) return;
            // Create additional fake adorners when dragging from the count label
            offset = 0;
            foreach (Card c in MultipleCards.Skip(1))
            {
                offset += step;
                var overlay = new CardDragAdorner(this, _mouseOffset);
                overlay.OffsetBy(offset, 0);
                overlay.CollapseTo(0, 0, false);
                OverlayElements.Add(overlay);
                if (layer != null) layer.Add(overlay);
            }
        }

        protected void DragCardCompleted()
        {
            if (!_isDragging) return;
            _isDragging = false;
            if (Card.Controller != Player.LocalPlayer) return;
            // Release the card and its group
            foreach (Card c in DraggedCards)
            {
                if(c != null)
                    c.ReleaseControl();
            }
            Card.Group.ReleaseControl();

            // Remove the visual feedback
            var mwc = _mainWin.Content as Visual;
            if (mwc != null)
            {
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(mwc);
                foreach (CardDragAdorner overlay in OverlayElements)
                {
                    layer.Remove(overlay);
                    overlay.Dispose();
                }
            }
            OverlayElements.Clear();

            // Raise CardOutEvent
            if (_lastDragTarget != null)
            {
                _lastDragTarget.RaiseEvent(new CardsEventArgs(Card, DraggedCards, CardOutEvent, this));
                _lastDragTarget = null;
            }

            // Raise CardDroppedEvent
            IInputElement res = Mouse.DirectlyOver;
            if (res != null)
            {
                var args = new CardsEventArgs(Card, DraggedCards, CardDroppedEvent, this)
                               {
                                   MouseOffset = _mouseOffset,
                                   FaceUp = !(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                               };
                res.RaiseEvent(args);
            }

            // Restore full opacity
            // FIX (jods): if the cards have been moved to another group, groupCtrl is null.
            //					 	 But in this case nothing has to be done opacity-wise since 
            //					   the CardControls have been unloaded.
            GroupControl groupCtrl = GroupControl;
            if (groupCtrl != null)
                foreach (CardControl cardCtrl in Selection.GetCardControls(groupCtrl, this))
                    cardCtrl.Opacity = 1;

            DraggedCards.Clear();
        }

        protected void DragMouseDelta(double dx, double dy)
        {
            IInputElement res = _mainWin.InputHitTest(Mouse.GetPosition(_mainWin));

            // Raise CardOverEvent
            if (res != null)
            {
                foreach (CardDragAdorner overlay in OverlayElements) overlay.OnHoverRequestInverted = false;
                var overArgs = new CardsEventArgs(Card, DraggedCards, CardOverEvent, this) { MouseOffset = _mouseOffset, Adorners = OverlayElements };
                res.RaiseEvent(overArgs);

                // Collapse/Expand multiple cards when inside/outside of table
                DragCollapseSelection(_lastDragTarget, overArgs.Handler);

                // Keep track of previous target and raise CardOutEvent
                if (overArgs.Handler != _lastDragTarget && _lastDragTarget != null)
                    _lastDragTarget.RaiseEvent(new CardsEventArgs(Card, DraggedCards, CardOutEvent, this));
                _lastDragTarget = overArgs.Handler;

                if (overArgs.CanDrop)
                {
                    if (!overArgs.FaceUp.HasValue)
                        overArgs.FaceUp = (Keyboard.Modifiers & ModifierKeys.Shift) == 0;

                    foreach (CardDragAdorner overlay in OverlayElements)
                        overlay.SetState(dx, dy, true, overArgs.CardSize, overArgs.FaceUp.Value,
                                         overlay.OnHoverRequestInverted);

                    return;
                }
            }
            else if (_lastDragTarget != null)
            {
                _lastDragTarget.RaiseEvent(new CardsEventArgs(Card, DraggedCards, CardOutEvent, this));
                _lastDragTarget = null;
            }

            // Update the visual feedback (can't drop)
            foreach (CardDragAdorner overlay in OverlayElements)
                overlay.SetState(dx, dy, false, Size.Empty, false, false);
        }

        private void DragCollapseSelection(IInputElement previous, IInputElement current)
        {
            if (current == previous) return;
            if (DraggedCards.Count <= 1) return;

            bool isFirst = true;
            // Collapse if we leave the table
            if (previous is TableControl)
            {
                foreach (CardDragAdorner overlay in OverlayElements)
                {
                    if (overlay.SourceCard == this && isFirst)
                    {
                        isFirst = false;
                        continue;
                    }
                    CardControl cardCtrl = overlay.SourceCard;
                    double dx = Card.X - cardCtrl.Card.X, dy = Card.Y - cardCtrl.Card.Y;
                    overlay.CollapseTo(dx * ScaleFactor.Width, dy * ScaleFactor.Height);
                }
            }
            // Expand if we enter the table 			
            else if (current is TableControl)
            {
                foreach (CardDragAdorner overlay in OverlayElements)
                {
                    if (overlay.SourceCard == this && isFirst)
                    {
                        isFirst = false;
                        continue;
                    }
                    overlay.Expand();
                }
            }
        }

        protected void DragTargetDelta(Point pt)
        {
            _draggedArrow.UpdateToPoint(pt);
        }

        public void CreateArrowTo(Player player, CardControl toCard)
        {
            AdornerLayer layer = AdornerLayer.GetAdornerLayer(this);
            var arrow = new ArrowAdorner(player, this);
            layer.Add(arrow);
            arrow.LinkToCard(toCard);
        }

        #endregion

        private static readonly Key[] AllKeys = Enum.GetValues(typeof(Key)).OfType<Key>().ToArray();

        public static List<Key> GetDownKeys()
        {
            var ret = new List<Key>();
            foreach (var currentKey in AllKeys)
            {
                var key = currentKey;
                if (key == Key.None)
                    continue;
                if (Keyboard.IsKeyDown(currentKey))
                    ret.Add(currentKey);
            }
            return ret;
        }

        private string[] downKeys = new string[0];

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            downKeys = GetDownKeys().Select(x => x.ToString()).ToArray();

        }

        #region Card hovering

        public static readonly RoutedEvent CardHoveredEvent = EventManager.RegisterRoutedEvent("CardHovered",
                                                                                               RoutingStrategy.Bubble,
                                                                                               typeof(CardEventHandler),
                                                                                               typeof(CardControl));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property != IsMouseOverProperty) return;
            RaiseEvent(IsMouseOver
                           ? new CardEventArgs(Card, CardHoveredEvent, this)
                           : new CardEventArgs(CardHoveredEvent, this));
        }

        #endregion

        #region Card actions

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            e.Handled = true;
            if (Mouse.Captured != null) return; // don't open during drag and drop operations
            base.OnContextMenuOpening(e);
            if (GroupControl != null)
                GroupControl.ShowContextMenu(Card);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            e.Handled = true;
            mouseClickHandler.OnDoubleClick(e);
        }

        private void TableKeyDown(object source, TableKeyEventArgs te)
        {
            try
            {
                // Fix: keyboard shortcuts are forbidden during a DnD
                if (_isDragging)
                {
                    te.Handled = te.KeyEventArgs.Handled = true;
                    return;
                }

                KeyEventArgs e = te.KeyEventArgs;
                switch (e.Key)
                {
                    case Key.PageUp:
                        Program.GameEngine.Table.BringToFront(Card);
                        e.Handled = te.Handled = true;
                        break;
                    case Key.PageDown:
                        Program.GameEngine.Table.SendToBack(Card);
                        e.Handled = te.Handled = true;
                        break;
                    case Key.P:
                        if (e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control) && !Card.FaceUp)
                        {
                            if (Card != null)
                                Card.Peek();
                            break;
                        }
                        goto default;
                    default:
                        // Look for a custom shortcut in the game definition
                        ActionShortcut[] shortcuts = Card.Group.CardShortcuts;
                        ActionShortcut match =
                            shortcuts.FirstOrDefault(shortcut => shortcut.Key.Matches(this, te.KeyEventArgs));
                        if (match != null && Card.Group.CanManipulate())
                        {
                            // Look for cards to execute it upon, shortcuts are applied to selection first
                            IEnumerable<Card> targets;
                            if (!Selection.IsEmpty())
                                targets = Selection.Cards;
                            else if (Card.CanManipulate())
                                targets = Selection.ExtendToSelection(Card);
                            else
                                break;
                            // If the card is on the table, extract the cursor position
                            Point? pos = GroupControl is TableControl
                                             ? ((TableControl)GroupControl).MousePosition()
                                             : (Point?)null;
                            if (match.ActionDef.AsAction().Execute != null)
                                ScriptEngine.ExecuteOnCards(match.ActionDef.AsAction().Execute, targets, pos);
                            else if (match.ActionDef.AsAction().BatchExecute != null)
                                ScriptEngine.ExecuteOnBatch(match.ActionDef.AsAction().BatchExecute, targets, pos);
                            e.Handled = te.Handled = true;
                            break;
                        }

                        // Look for a "Move to" shortcut
                        Group group =
                            Player.LocalPlayer.Groups.FirstOrDefault(
                                g => g.MoveToShortcut != null && g.MoveToShortcut.Matches(this, te.KeyEventArgs));
                        bool toBottom = false;
                        // If no group is found, try to match a shortcut with "Alt" and use it as "Move to bottom"
                        if (group == null)
                        {
                            group =
                                Player.LocalPlayer.Groups.FirstOrDefault(
                                    g =>
                                    g.MoveToShortcut != null &&
                                    new KeyGesture(g.MoveToShortcut.Key, g.MoveToShortcut.Modifiers | ModifierKeys.Alt).
                                        Matches(this, te.KeyEventArgs));
                            if (group is Pile) toBottom = true;
                        }
                        if (group != null && group.CanManipulate())
                        {
                            Action<Card> moveAction = toBottom
                                                          ? (c => c.MoveTo(@group, true, @group.Count, false))
                                                          : new Action<Card>(c => c.MoveTo(group, true, false));
                            if (!Selection.IsEmpty())
                                Selection.ForEachModifiable(moveAction);
                            else if (count.IsMouseOver)
                            {
                                for (int i = MultipleCards.Count - 1; i >= 0; --i)
                                {
                                    var c = (Card)MultipleCards[i];
                                    if (c.CanManipulate()) moveAction(c);
                                }
                            }
                            else if (Card.CanManipulate())
                                moveAction(Card);
                            else
                                break;
                            e.Handled = te.Handled = true;
                            break;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Log.Warn("TableKeyDown Error",e);
            }
        }

        #endregion

        #region Markers

        private void MarkerDropped(object sender, MarkerEventArgs e)
        {
            e.Handled = true;
            if (e.Marker.Card == Card) return;
            if (Keyboard.IsKeyUp(Key.LeftAlt))
            {
                Program.Client.Rpc.TransferMarkerReq(e.Marker.Card, Card, e.Marker.Model.Id, e.Marker.Model.Name,
                                                     e.Count);
                e.Marker.Card.RemoveMarker(e.Marker, e.Count);
            }
            else
                Program.Client.Rpc.AddMarkerReq(Card, e.Marker.Model.Id, e.Marker.Model.Name, e.Count);
            Card.AddMarker(e.Marker.Model, e.Count);
        }

        #endregion

        #region Internals

        internal GroupControl GroupControl
        {
            get
            {
                FrameworkElement iter = ItemsControl.ItemsControlFromItemContainer(VisualTreeHelper.GetParent(this)) ??
                                        Parent as FrameworkElement;
                while (iter != null)
                {
                    iter = iter.Parent as FrameworkElement;
                    var groupControl = iter as GroupControl;
                    if (groupControl != null)
                        return groupControl;
                }
                return null;
            }
        }

        internal Visual GetCardVisual()
        {
            return contentCtrl.Content as Visual;
        }

        internal Point GetMiddlePoint(bool invertRotation)
        {
            var middlePt = new Point(ActualWidth / 2, ActualHeight / 2);
            Point rotatedPt = rotate90.Transform(middlePt);
            if (invertRotation && _invertTransform != null)
                rotatedPt = new Point(2 * middlePt.X - rotatedPt.X, 2 * middlePt.Y - rotatedPt.Y);
            return rotatedPt;
        }

        internal bool IsOnTableCanvas { get; private set; }

        #endregion
    }

    internal class CenterConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var axis = parameter as string;
            if (axis == "X")
                return ((double)values[0]) / 2;
            return ((double)values[1]) - ((double)values[0]) / 2;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    [ValueConversion(typeof(object), typeof(Visibility))]
    internal class NullToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    internal class VisibleAndNullConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = Visibility.Collapsed;

            var vlist = values.ToList();
            var val1 = (Visibility)vlist.First(x => x is Visibility);
            vlist.Remove(val1);
            var val2 = vlist.First();

            if (val1 == Visibility.Visible)
            {
                if (val2 != null && val2 is Player)
                {
                    visibility = Visibility.Visible;
                }
            }
            return visibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(int), typeof(Visibility))]
    internal class CountToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int && ((int)value) > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    internal class HighlightColorConverter : IMultiValueConverter
    {
        #region IMultiValueConverter Members

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 || values[0] == DependencyProperty.UnsetValue) return DependencyProperty.UnsetValue;
            var selected = (bool)values[0];
            var color = (Color?)values[1];

            if (selected) return Brushes.Yellow;

            if (!color.HasValue)
                return Brushes.White;
            // White should never appear on screen -> the card is neither selected nor has a highlight      
            var brush = new SolidColorBrush(color.Value);
            brush.Freeze();
            return brush;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}