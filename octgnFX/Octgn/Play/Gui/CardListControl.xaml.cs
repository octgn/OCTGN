using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Octgn.Play.Gui
{
    public partial class CardListControl
    {
        public static readonly DependencyProperty IsAlwaysUpProperty =
            DependencyProperty.Register("IsAlwaysUp", typeof (bool), typeof (CardListControl),
                                        new UIPropertyMetadata(false));

        private ObservableCollection<Card> _cards;

        private Predicate<Card> _filterCards;
        private bool _sort;
        private ListCollectionView _view;
        private WrapPanel _wrapPanel;

        public CardListControl()
        {
            InitializeComponent();
        }

        public bool IsAlwaysUp
        {
            get { return (bool) GetValue(IsAlwaysUpProperty); }
            set { SetValue(IsAlwaysUpProperty, value); }
        }

        public ObservableCollection<Card> Cards
        {
            get { return _cards; }
            set
            {
                _cards = value;
                list.ItemsSource = _view = new ListCollectionView(_cards);
            }
        }

        public Predicate<Card> FilterCards
        {
            get { return _filterCards; }
            set
            {
                _filterCards = value;
                _view.Filter = value == null ? (Predicate<object>) null : o => _filterCards((Card) o);
                _view.Refresh();
            }
        }

        public bool RestrictDrop { get; set; }

        public bool SortByName
        {
            get { return _sort; }
            set
            {
                if (_sort == value) return;
                _sort = value;
                using (_view.DeferRefresh())
                {
                    if (value)
                    {
                        _view.CustomSort = IsAlwaysUp
                                               ? (IComparer) new Card.RealNameComparer()
                                               : new Card.NameComparer();
                        if (_view.GroupDescriptions != null)
                            _view.GroupDescriptions.Add(new PropertyGroupDescription(IsAlwaysUp ? "RealName" : "Name"));
                    }
                    else
                    {
                        _view.CustomSort = null;
                        if (_view.GroupDescriptions != null) _view.GroupDescriptions.Clear();
                    }
                }
            }
        }

        private void SaveWrapPanel(object sender, RoutedEventArgs e)
        {
            _wrapPanel = (WrapPanel) sender;
            _wrapPanel.AdornerLayerVisual = scroller; // So that adorners are not clipped by the scrollviewer
            _wrapPanel.ClippingVisual = scroller; // but rather with the better suited InsertAdorner clipping behavior
        }

        //internal override void ShowContextMenu(Card card, bool showGroupActions = true)
        //{
        //    // Don't show the group context menu in card lists.
        //}

        public override bool ExecuteDefaultGroupAction()
        {
            return false;
        }

        #region Card DnD

        protected override void OnCardOut(object sender, CardsEventArgs e)
        {
            e.Handled = true;
            _wrapPanel.HideInsertIndicator();
            StopDragScroll();
        }

        protected override void OnCardOver(object sender, CardsEventArgs e)
        {
            base.OnCardOver(sender, e);

            // Set overlay card size
            e.CardSize = new Size(Program.GameEngine.Definition.CardWidth * 100 / Program.GameEngine.Definition.CardHeight, 100);
            if (IsAlwaysUp) e.FaceUp = true;

            // Drop is forbidden when not ordered by position
            if (SortByName)
            {
                e.CanDrop = false;
                return;
            }

            // When the list is restricted to some cards only, 
            // one cannot drop cards from outside this list
            if (RestrictDrop && !e.Cards.All(c => Cards.Contains(c)))
            {
                e.CanDrop = false;
                return;
            }

            // Display insert indicator
            _wrapPanel.DisplayInsertIndicator(e.ClickedCard, _wrapPanel.GetIndexFromPoint(Mouse.GetPosition(_wrapPanel)));

            // Scroll the scroll viewer if required
            double pos = Mouse.GetPosition(scroller).Y;
            if (pos <= ScrollMargin || pos >= scroller.ActualHeight - ScrollMargin)
            {
                if (_scrollTimer == null)
                {
                    _scrollSpeed = ScrollInitialSpeed;
                    _scrollDirectionUp = pos <= ScrollMargin;
                    _scrollTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(ScrollTimeInterval)};
                    _scrollTimer.Tick += DragScroll;
                    _scrollTimer.Start();
                }
            }
            else
                StopDragScroll();
        }

        protected override void OnCardDropped(object sender, CardsEventArgs e)
        {
            // Drop is forbidden when not ordered by position
            if (SortByName)
            {
                e.Handled = true;
                e.CanDrop = false;
                return;
            }

            // When the list is restricted to some cards only, 
            // one cannot drop cards from outside this list
            if (RestrictDrop && !e.Cards.All(c => Cards.Contains(c)))
            {
                e.Handled = true;
                e.CanDrop = false;
                return;
            }

            StopDragScroll();
            e.Handled = e.CanDrop = true;
            if (!@group.TryToManipulate()) return;
            int idx = _wrapPanel.GetIndexFromPoint(Mouse.GetPosition(_wrapPanel));

            // When the list is restricted, real index may be different from index in the GUI
            if (RestrictDrop)
            {
                Card c = null;
                bool after = false;
                if (idx < _view.Count)
                    c = (Card) _view.GetItemAt(idx);
                else if (_view.Count > 0)
                {
                    c = (Card) _view.GetItemAt(_view.Count - 1);
                    after = true;
                }

                if (c != null) idx = @group.Cards.IndexOf(c) + (after ? 1 : 0);
            }

            foreach (Card c in e.Cards)
            {
                // Fix the target index if the card is already in the group at a lower index
                if (c.Group == @group && c.GetIndex() < idx) --idx;
                c.MoveTo(@group, e.FaceUp != null && e.FaceUp.Value, idx++,false);
            }
        }

        #endregion

        #region Drag scrolling

        private const int ScrollMargin = 10;
        private const double ScrollTimeInterval = 10; // ms
        private const double ScrollInitialSpeed = 0.05; // px per ms
        private const double ScrollAcceleration = 0.0005; // px per ms^2
        private const double ScrollMaxSpeed = 2; // px per ms
        private bool _scrollDirectionUp;
        private double _scrollSpeed;
        private DispatcherTimer _scrollTimer;


        private void DragScroll(object sender, EventArgs e)
        {
            if (!scroller.IsLoaded) return;
            scroller.ScrollToVerticalOffset(scroller.VerticalOffset +
                                            (_scrollDirectionUp
                                                 ? -_scrollSpeed*ScrollTimeInterval
                                                 : _scrollSpeed*ScrollTimeInterval));
            _scrollSpeed = Math.Min(ScrollMaxSpeed, _scrollSpeed + ScrollAcceleration*ScrollTimeInterval);
        }

        private void StopDragScroll()
        {
            if (_scrollTimer == null) return;
            _scrollTimer.Stop();
            _scrollTimer = null;
        }

        #endregion

        #region Smooth scrolling

        private static readonly Duration SmoothScrollDuration = TimeSpan.FromMilliseconds(500);

        private static readonly DependencyProperty AnimatableVerticalOffsetProperty =
            DependencyProperty.Register("AnimatableVerticalOffset", typeof (double), typeof (CardListControl),
                                        new UIPropertyMetadata(0.0, AnimatableVerticalOffsetChanged));

        private DoubleAnimation _scrollAnimation;
        private int _scrollDirection;
        private double _scrollTarget;

        private double AnimatableVerticalOffset
        {
            get { return (double) GetValue(AnimatableVerticalOffsetProperty); }
            set { SetValue(AnimatableVerticalOffsetProperty, value); }
        }

        private static void AnimatableVerticalOffsetChanged(DependencyObject sender,
                                                            DependencyPropertyChangedEventArgs e)
        {
            var ctrl = sender as CardListControl;
            if (ctrl != null) ctrl.scroller.ScrollToVerticalOffset((double) e.NewValue);
        }

        private void SmoothScroll(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            // Add inerita to scrolling for a very smooth effect      
            int sign = Math.Sign(e.Delta);
            double offset = -sign*48.0;
            if (sign == _scrollDirection)
                _scrollTarget += offset;
            else
            {
                _scrollDirection = sign;
                _scrollTarget = scroller.VerticalOffset + offset;
            }
            EnsureScrollAnimation();
            _scrollAnimation.From = scroller.VerticalOffset;
            _scrollAnimation.To = _scrollTarget;
            BeginAnimation(AnimatableVerticalOffsetProperty, _scrollAnimation);
        }

        private void EnsureScrollAnimation()
        {
            _scrollAnimation = new DoubleAnimation {Duration = SmoothScrollDuration, DecelerationRatio = 0.5};
            _scrollAnimation.Completed += delegate
                                              {
                                                  _scrollAnimation = null;
                                                  _scrollDirection = 0;
                                              };
        }

        #endregion
    }
}