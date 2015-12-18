using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Octgn.Controls;
using Octgn.Data;
using Octgn.Utils;


namespace Octgn.Scripting.Controls
{
    using System.Linq.Expressions;

    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;
    using Octgn.Play;

    public partial class SelectMultiCardsDlg
    {
        public static readonly DependencyProperty AllowSelectProperty = DependencyProperty.Register(
            "AllowSelect", typeof(bool), typeof(SelectMultiCardsDlg), new UIPropertyMetadata(false));

        public List<int> allCards;
        public List<int> allCards2;
        private string _filterText = "";
        private string _filter2Text = "";
        private int? _min;
        private int? _max;
        private IEnumerable<string> textProperties = Program.GameEngine.Definition.CustomProperties
                    .Where(p => p.Type == DataNew.Entities.PropertyType.String && !p.IgnoreText)
                    .Select(p => p.Name);

        public SelectMultiCardsDlg(List<int> cardList, List<int> cardList2, string prompt, string title, int? minValue, int? maxValue, string boxLabel, string boxLabel2)
        {
            InitializeComponent();
            Title = title;
            promptLbl.Text = prompt;
            if (string.IsNullOrEmpty(prompt)) promptBox.Visibility = Visibility.Collapsed;
            boxLbl.Text = boxLabel;
            boxLbl2.Text = boxLabel2;

            Task.Factory.StartNew(() =>
            {
                if (cardList == null) cardList = new List<int>();
                _min = minValue;
                _max = maxValue;
                allCards = cardList.ToList();
                if (cardList2 != null) allCards2 = cardList2.ToList();


                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //additional drag/drop style for list boxes
                    var style = new Style(typeof(ListBox));
                    style.BasedOn = allList.Style;
                    
                    style.Setters.Add(
                        new EventSetter(
                            ListBox.PreviewMouseMoveEvent,
                            new MouseEventHandler(DragDropMove)));
                    style.Setters.Add(
                        new Setter(
                            ListBox.AllowDropProperty,
                            true));
                    style.Setters.Add(
                        new EventSetter(
                            ListBox.PreviewMouseLeftButtonDownEvent,
                            new MouseButtonEventHandler(DragDropDown)));
                    style.Setters.Add(
                        new EventSetter(
                            ListBox.PreviewMouseLeftButtonUpEvent,
                            new MouseButtonEventHandler(DragDropUp)));
                    style.Setters.Add(
                          new EventSetter(
                            ListBox.DropEvent,
                            new DragEventHandler(DragDropDrop)));
                    style.Setters.Add(
                           new EventSetter(
                               ListBoxItem.PreviewDragOverEvent,
                               new DragEventHandler(DragDropOver)));

 

                    allList.ItemsSource = allCards;
                    allList2.ItemsSource = allCards2;
                    if (allCards2 != null) // multi-box will always have drag/drop
                    {
                        if (_max == null) _max = allCards.Count + allCards2.Count; // max value will be the total count of both lists
                        if (_min == null) _min = 0; // min value will be the lowest value possible
                        if (_min > _max) _min = _max; // prevent oddities where user set the min value higher than max
                        allList.Style = style;
                        allList2.Style = style;
                        AllowSelect = (_min <= allCards.Count && allCards.Count <= _max);
                    }
                    else // only one box, check if drag/drop is allowed
                    {
                        if (_max == null) _max = 1;
                        if (_min == null) _min = 1;
                        if (_min > _max) _min = _max; // prevent oddities where user set the min value higher than max
                        allList2.Visibility = Visibility.Collapsed; // hides the second box
                        box2GridRow.Height = new GridLength(0); // hides the second box
                        if (_max <= 0) // a maximum value of 0 means that we want to reorganize the group, not select cards from it
                        {
                            allList.Style = style;
                            selectButton.IsEnabled = true;
                            AllowSelect = true;
                        }
                        else if (_min == 1 && _max == 1) // only allow a single choice
                        {
                            allList.SelectionChanged += CardSelected;
                            allList.MouseDoubleClick += SelectClicked; // double clicking the card will auto-confirm it
                        }
                        else //allow multiple choice
                        {
                            allList.SelectionChanged += CardSelected;
                            allList.SelectionMode = SelectionMode.Multiple;
                            if (_min == 0) AllowSelect = true;
                        }
                    }

                }));
            });
        }

        public bool AllowSelect
        {
            get { return (bool)GetValue(AllowSelectProperty); }
            set { SetValue(AllowSelectProperty, value); }
        }

        public List<int> selectedCards { get; private set; }

        private void SelectClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            // A double-click can only select a marker in its own list
            // (Little bug here: double-clicking in the empty zone of a list with a selected marker adds it)
//            if (sender is ListBox && ((ListBox)sender).SelectedIndex == -1) return;

            allList.ItemsSource = allCards;
            if (AllowSelect == false) return;

            selectedCards = new List<int>();

            foreach (int item in allList.SelectedItems)
                selectedCards.Add(item);
                        
            DialogResult = true;
        }

        private void CardSelected(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            AllowSelect = (_min <= allList.SelectedItems.Count && allList.SelectedItems.Count <= _max);
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            _filterText = filterBox.Text;
            if (string.IsNullOrEmpty(_filterText))
            {
                allList.ItemsSource = allCards;
                return;
            }
            // Filter asynchronously (so the UI doesn't freeze on huge lists)
            if (allCards == null) return;
            ThreadPool.QueueUserWorkItem(searchObj =>
                                    {
                                        var search = (string)searchObj;
                                        List<int> filtered =
                                            allCards.Where(
                                                m =>
                                                Card.Find(m).RealName.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                                textProperties.Select(property => (string) Card.Find(m).GetProperty(property)).
                                                Where(propertyValue => propertyValue != null).Any(
                                                propertyValue => propertyValue.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0)
                                                )
                                                .ToList();
                                        if (search == _filterText)
                                            Dispatcher.Invoke(new Action(() => allList.ItemsSource = filtered));
                                    }, _filterText);
        }

        private void PreviewFilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || filterBox.Text.Length <= 0) return;
            filterBox.Clear();
            e.Handled = true;
        }

        private void Filter2Changed(object sender, EventArgs e)
        {
            _filter2Text = filter2Box.Text;
            if (string.IsNullOrEmpty(_filter2Text))
            {
                allList2.ItemsSource = allCards2;
                return;
            }
            // Filter asynchronously (so the UI doesn't freeze on huge lists)
            if (allCards2 == null) return;
            ThreadPool.QueueUserWorkItem(searchObj =>
            {
                var search = (string)searchObj;
                List<int> filtered =
                    allCards2.Where(
                        m =>
                        Card.Find(m).RealName.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                        textProperties.Select(property => (string)Card.Find(m).GetProperty(property)).
                           Where(propertyValue => propertyValue != null).Any(
                           propertyValue => propertyValue.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        )
                        .ToList();
                if (search == _filter2Text)
                    Dispatcher.Invoke(new Action(() => allList2.ItemsSource = filtered));
            }, _filter2Text);
        }

        private void PreviewFilter2KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || filter2Box.Text.Length <= 0) return;
            filter2Box.Clear();
            e.Handled = true;
        }

        private void SetPicture(object sender, RoutedEventArgs e)
        {
            var img = sender as Image;
            if (img == null) return;
            var model = Card.Find((int)img.DataContext).Type.Model;
            if (model != null) ImageUtils.GetCardImage(model, x => img.Source = x);
        }

        private void ComputeChildWidth(object sender, RoutedEventArgs e)
        {
            var panel = sender as VirtualizingWrapPanel;
            if (panel != null) panel.ChildWidth = panel.ChildHeight * Program.GameEngine.Definition.CardSize.Width / Program.GameEngine.Definition.CardSize.Height;
        }

        private Point startPoint;
        private Window topWindow;
        private ListBox sourceBox = null;
        private FrameworkElement sourceItem;
        private DragAdorner adorner;
        private int? targetItem;
        private object draggedData;

        private void DragDropDown(object sender, MouseButtonEventArgs e)
        {
            this.sourceBox = (ListBox)sender;
            Visual visual = e.OriginalSource as Visual;

            this.topWindow = Window.GetWindow(this.sourceBox);
            this.startPoint = e.GetPosition(this.topWindow);

            this.sourceItem = sourceBox.ContainerFromElement(visual) as FrameworkElement;
            if (this.sourceItem != null)
            {
                this.draggedData = this.sourceItem.DataContext;
            }
            
        }

        private void DragDropUp(object sender, MouseButtonEventArgs e)
        {
            this.draggedData = null;
        }

        private void DragDropMove(object sender, MouseEventArgs e)
        {
            if (this.draggedData != null)
            {

                if (Math.Abs(this.startPoint.X - e.GetPosition(this.topWindow).X) >= SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(this.startPoint.Y - e.GetPosition(this.topWindow).Y) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    bool previousAllowDrop = this.topWindow.AllowDrop;
                    this.topWindow.AllowDrop = true;
                    this.topWindow.DragEnter += TopWindow_DragEnter;
                    this.topWindow.DragOver += TopWindow_DragOver;
                    this.topWindow.DragLeave += TopWindow_DragLeave;

                    DragDrop.DoDragDrop(this.sourceItem, this.draggedData, DragDropEffects.Move);

                    RemoveAdorner();

                    this.topWindow.AllowDrop = previousAllowDrop;
                    this.topWindow.DragEnter -= TopWindow_DragEnter;
                    this.topWindow.DragOver -= TopWindow_DragOver;
                    this.topWindow.DragLeave -= TopWindow_DragLeave;

                    this.draggedData = null;
                }
            }
        }

        private void TopWindow_DragEnter(object sender, DragEventArgs e)
        {
            ShowAdorner();
            adorner.UpdatePosition(e.GetPosition(this.topWindow));
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void TopWindow_DragOver(object sender, DragEventArgs e)
        {
            ShowAdorner();
            adorner.UpdatePosition(e.GetPosition(this.topWindow));
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        private void TopWindow_DragLeave(object sender, DragEventArgs e)
        {
            RemoveAdorner();
            e.Handled = true;
        }
        
        private void ShowAdorner()
        {
            if (adorner == null)
            {
                adorner = new DragAdorner(this.sourceItem, this.startPoint);
                AdornerLayer.GetAdornerLayer(this.sourceBox).Add(adorner);
            }
        }

        private void RemoveAdorner()
        {
            if (this.adorner != null)
            {
                AdornerLayer.GetAdornerLayer(this.sourceBox).Remove(adorner);
                this.adorner = null;
            }
        }
        
        private T FindVisualParent<T>(DependencyObject child)
            where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;
            T parent = parentObject as T;
            if (parent != null)
                return parent;
            return FindVisualParent<T>(parentObject);
        }

        private void DragDropOver(object sender, DragEventArgs e)
        {
            ShowAdorner();
            adorner.UpdatePosition(e.GetPosition(this.topWindow));

            e.Handled = true;
            var item = FindVisualParent<ListBoxItem>((DependencyObject)e.OriginalSource);
            if (item == null)
                targetItem = null;
            else
                targetItem = (int)item.DataContext;
        }

        private void DragDropDrop(object sender, DragEventArgs e)
        {
            if (sender is ListBox && e.Data.GetDataPresent(typeof(int)))
            {
                Dispatcher.Invoke(new Action(() => 
                {
                    var source = (int)e.Data.GetData(typeof(int));
                    var sourceList = allCards;
                    if (this.sourceBox.Name == "allList2") sourceList = allCards2;
                    int sourceIndex = this.sourceBox.Items.IndexOf(source);

                    var targetBox = (ListBox)sender;
                    var targetList = allCards;
                    if (targetBox.Name == "allList2") targetList = allCards2;
                    int targetIndex = targetBox.Items.IndexOf(targetItem);

                    if (targetIndex < 0)
                    {
                        targetList.Add(source);
                        sourceList.RemoveAt(sourceIndex);
                    }
                    else if (this.sourceBox.Name != targetBox.Name)
                    {
                        targetList.Insert(targetIndex, source);
                        sourceList.RemoveAt(sourceIndex);
                    }
                    else if (sourceIndex < targetIndex)
                    {
                        targetList.Insert(targetIndex + 1, source);
                        sourceList.RemoveAt(sourceIndex);
                    }
                    else
                    {
                        int removeIndex = sourceIndex + 1;
                        if (sourceList.Count + 1 > removeIndex)
                        {
                            targetList.Insert(targetIndex, source);
                            sourceList.RemoveAt(removeIndex);
                        }
                    }
                    allList.ItemsSource = allCards.ToList();
                    if (allCards2 != null)
                    {
                        allList2.ItemsSource = allCards2.ToList();
                        AllowSelect = (_min <= allCards.Count && allCards.Count <= _max);
                    }
                    e.Handled = true;
                }));
            }
        }
        
        public class DragAdorner : Adorner
        {
            private Brush vbrush;
            private Point location;
            private Point offset;

            public DragAdorner(UIElement adornedElement, Point offset)
                : base(adornedElement)
            {
                this.offset = offset;
                vbrush = new VisualBrush(AdornedElement);
                vbrush.Opacity = .7;
                this.IsHitTestVisible = false;
            }

            public void UpdatePosition(Point location)
            {
                this.location = location;
                this.InvalidateVisual();
            }
            
            protected override void OnRender(DrawingContext dc)
            {
                var p = location;
                p.Offset(-offset.X, -offset.Y);
                dc.DrawRectangle(vbrush, null, new Rect(p, this.RenderSize));
            }
        }


    }
}