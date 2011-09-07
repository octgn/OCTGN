using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;

namespace Octgn.Play.Gui
{
  public partial class CardListControl : GroupControl
  {
    private WrapPanel wrapPanel;

    public bool IsAlwaysUp
    {
      get { return (bool)GetValue(IsAlwaysUpProperty); }
      set { SetValue(IsAlwaysUpProperty, value); }
    }

    public static readonly DependencyProperty IsAlwaysUpProperty =
        DependencyProperty.Register("IsAlwaysUp", typeof(bool), typeof(CardListControl), new UIPropertyMetadata(false));

    private ObservableCollection<Card> _cards;
    public ObservableCollection<Card> Cards 
    {
      get { return _cards; }
      set
      {
        _cards = value;
        list.ItemsSource = View = new ListCollectionView(_cards);
      }
    }

    private Predicate<Card> _filterCards;
    public Predicate<Card> FilterCards
    {
      get { return _filterCards; }
      set
      {
        _filterCards = value;
        View.Filter = value == null ? (Predicate<object>)null : (object o) => _filterCards((Card)o);
        View.Refresh();
      }
    }

    public bool RestrictDrop { get; set; }

    private bool _sort;
    public bool SortByName
    {
      get { return _sort; }
      set
      {
        if (_sort == value) return;
        _sort = value;
        using (View.DeferRefresh())
        {
          if (value)
          {
            View.CustomSort = IsAlwaysUp ? (System.Collections.IComparer)new Card.RealNameComparer() : new Card.NameComparer();
            View.GroupDescriptions.Add(new PropertyGroupDescription(IsAlwaysUp ? "RealName" : "Name"));
          }
          else
          {
            View.CustomSort = null;
            View.GroupDescriptions.Clear();
          }
        }
      }
    }

    private ListCollectionView View;

    public CardListControl()
    {
      InitializeComponent();
    }
    
    private void SaveWrapPanel(object sender, RoutedEventArgs e)
    {
      wrapPanel = (WrapPanel)sender;
      wrapPanel.AdornerLayerVisual = scroller;    // So that adorners are not clipped by the scrollviewer
      wrapPanel.ClippingVisual = scroller;        // but rather with the better suited InsertAdorner clipping behavior
    }

    internal override void ShowContextMenu(Card card, bool showGroupActions = true)
    {
        // Don't show the group context menu in card lists.
    }

    #region Card DnD

    protected override void OnCardOut(object sender, CardsEventArgs e)
    {
      e.Handled = true;
      wrapPanel.HideInsertIndicator();
      StopDragScroll();
    }

    protected override void OnCardOver(object sender, CardsEventArgs e)
    {
      base.OnCardOver(sender, e);

      // Set overlay card size
      var cardDef = Program.Game.Definition.CardDefinition;
      e.CardSize = new Size(cardDef.Width * 100 / cardDef.Height, 100);
      if (IsAlwaysUp) e.FaceUp = true;

      // Drop is forbidden when not ordered by position
      if (SortByName)
      { e.CanDrop = false; return; }

      // When the list is restricted to some cards only, 
      // one cannot drop cards from outside this list
      if (RestrictDrop && !e.Cards.All(c => Cards.Contains(c)))
      { e.CanDrop = false; return; }

      // Display insert indicator
      wrapPanel.DisplayInsertIndicator(e.ClickedCard, wrapPanel.GetIndexFromPoint(Mouse.GetPosition(wrapPanel)));

      // Scroll the scroll viewer if required
      var pos = Mouse.GetPosition(scroller).Y;
      if (pos <= ScrollMargin || pos >= scroller.ActualHeight - ScrollMargin)
      {
        if (scrollTimer == null)
        {
          scrollSpeed = ScrollInitialSpeed;
          scrollDirectionUp = pos <= ScrollMargin;
          scrollTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(ScrollTimeInterval) };
          scrollTimer.Tick += DragScroll;
          scrollTimer.Start();
        }
      }
      else
        StopDragScroll();
    }

    protected override void OnCardDropped(object sender, CardsEventArgs e)
    {
      // Drop is forbidden when not ordered by position
      if (SortByName)
      { e.Handled = true; e.CanDrop = false; return; }

      // When the list is restricted to some cards only, 
      // one cannot drop cards from outside this list
      if (RestrictDrop && !e.Cards.All(c => Cards.Contains(c)))
      { e.Handled = true; e.CanDrop = false; return; }

      StopDragScroll();
      e.Handled = e.CanDrop = true;
      if (group.TryToManipulate())
      {
        int idx = wrapPanel.GetIndexFromPoint(Mouse.GetPosition(wrapPanel));

        // When the list is restricted, real index may be different from index in the GUI
        if (RestrictDrop)
        {
          Card c = null;
          bool after = false;
          if (idx < View.Count)
            c = (Card)View.GetItemAt(idx);
          else if (View.Count > 0)
          {
            c = (Card)View.GetItemAt(View.Count - 1); after = true;
          }

          if (c != null) idx = group.Cards.IndexOf(c) + (after ? 1 : 0);
        }

        foreach (Card c in e.Cards)
        {
          // Fix the target index if the card is already in the group at a lower index
          if (c.Group == group && c.GetIndex() < idx) --idx;
          c.MoveTo(group, e.FaceUp.Value, idx++);
        }
      }
    }

    #endregion

    #region Drag scrolling

    private DispatcherTimer scrollTimer;
    private double scrollSpeed;
    private bool scrollDirectionUp;
    private const int ScrollMargin = 10;
    private const double ScrollTimeInterval = 10;       // ms
    private const double ScrollInitialSpeed = 0.05;     // px per ms
    private const double ScrollAcceleration = 0.0005;   // px per ms^2
    private const double ScrollMaxSpeed = 2;            // px per ms


    private void DragScroll(object sender, EventArgs e)
    {
      if (!scroller.IsLoaded) return;
      scroller.ScrollToVerticalOffset(scroller.VerticalOffset + (scrollDirectionUp ? -scrollSpeed * ScrollTimeInterval : scrollSpeed * ScrollTimeInterval));
      scrollSpeed = Math.Min(ScrollMaxSpeed, scrollSpeed + ScrollAcceleration * ScrollTimeInterval);
    }

    private void StopDragScroll()
    {
      if (scrollTimer == null) return;
      scrollTimer.Stop();
      scrollTimer = null;
    }

    #endregion

    #region Smooth scrolling

    private static readonly Duration SmoothScrollDuration = TimeSpan.FromMilliseconds(500);
    private double scrollTarget;
    private int scrollDirection = 0;
    private DoubleAnimation scrollAnimation;

    private double AnimatableVerticalOffset
    {
      get { return (double)GetValue(AnimatableVerticalOffsetProperty); }
      set { SetValue(AnimatableVerticalOffsetProperty, value); }
    }

    private static readonly DependencyProperty AnimatableVerticalOffsetProperty =
        DependencyProperty.Register("AnimatableVerticalOffset", typeof(double), typeof(CardListControl), 
          new UIPropertyMetadata(0.0, AnimatableVerticalOffsetChanged));

    private static void AnimatableVerticalOffsetChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      var ctrl = sender as CardListControl;
      ctrl.scroller.ScrollToVerticalOffset((double)e.NewValue);
    }

    private void SmoothScroll(object sender, MouseWheelEventArgs e)
    {
      e.Handled = true;
      // Add inerita to scrolling for a very smooth effect      
      int sign = Math.Sign(e.Delta);
      double offset = -sign * 48.0;
      if (sign == scrollDirection)
        scrollTarget += offset;
      else
      {
        scrollDirection = sign;
        scrollTarget = scroller.VerticalOffset + offset;
      }
      EnsureScrollAnimation();
      scrollAnimation.From = scroller.VerticalOffset;
      scrollAnimation.To = scrollTarget;
      BeginAnimation(AnimatableVerticalOffsetProperty, scrollAnimation);
    }

    private void EnsureScrollAnimation()
    {
      scrollAnimation = new DoubleAnimation { Duration = SmoothScrollDuration, DecelerationRatio = 0.5 };
      scrollAnimation.Completed += delegate { scrollAnimation = null; scrollDirection = 0; };
    }

    #endregion
  }
}
