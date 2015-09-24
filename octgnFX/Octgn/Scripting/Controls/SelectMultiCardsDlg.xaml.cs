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
        private int? _min;
        private int? _max;
        private IEnumerable<string> textProperties = Program.GameEngine.Definition.CustomProperties
                    .Where(p => p.Type == DataNew.Entities.PropertyType.String && !p.IgnoreText)
                    .Select(p => p.Name);

        public SelectMultiCardsDlg(List<int> cardList, List<int> cardList2, string prompt, string title, int? minValue, int? maxValue)
        {
            InitializeComponent();
            Title = title;
            promptLbl.Text = prompt;
            Task.Factory.StartNew(() =>
            {
                if (cardList == null) cardList = new List<int>();
                _min = minValue;
                _max = maxValue;
                allCards = cardList.ToList();
                allCards2 = cardList2.ToList();


                Dispatcher.BeginInvoke(new Action(() =>
                {
                    //additional drag/drop style for list boxes
                    var style = new Style(typeof(ListBoxItem));
                    style.Setters.Add(
                        new EventSetter(
                            ListBoxItem.PreviewMouseMoveEvent,
                            new MouseEventHandler(DragDropMove)));
                    style.Setters.Add(
                        new Setter(
                            ListBoxItem.AllowDropProperty,
                            true));
                    style.Setters.Add(
                        new EventSetter(
                            ListBoxItem.PreviewMouseLeftButtonDownEvent,
                            new MouseButtonEventHandler(DragDropDown)));
                    style.Setters.Add(
                          new EventSetter(
                            ListBoxItem.DropEvent,
                            new DragEventHandler(DragDropDrop)));
 

                    allList.ItemsSource = allCards;
                    allList2.ItemsSource = allCards2;
                    if (allCards2 != null) // activate multi-box drag/drop
                    {
                        if (_max == null) _max = allCards.Count + allCards2.Count;
                        if (_min == null) _min = 0;
                        allList.ItemContainerStyle = style;
                        allList2.ItemContainerStyle = style;
                        AllowSelect = (_min <= allCards.Count && allCards.Count <= _max);
                    }
                    else // only one box, check if drag/drop is allowed
                    {
                        if (_max == null) _max = 1;
                        if (_min == null) _min = 1;
                        allList2.Visibility = Visibility.Collapsed;
                        box2GridRow.Height = new GridLength(0);
                        if (_max <= 0) // a maximum value of 0 means that we want to reorganize the group, not select cards from it
                        {
                            allList.ItemContainerStyle = style;
                            selectButton.IsEnabled = true;
                        }
                        else if (_min == 1 && _max == 1) // only allow a single choice
                        {
                            allList.SelectionChanged += CardSelected;
                            allList.MouseDoubleClick += SelectClicked; // double clicking the card will auto-confirm it
                        }
                        else
                        {
                            allList.SelectionChanged += CardSelected;
                            allList.SelectionMode = SelectionMode.Multiple; //allow multiple choice
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

        private Point _startPoint;

        private void DragDropDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
        }

        ListBox dragSource = null;

        private void DragDropMove(object sender, MouseEventArgs e)
        {
            Point point = e.GetPosition(null);
            Vector diff = _startPoint - point;

            if (e.LeftButton == MouseButtonState.Pressed &&
                (
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                )
            {
                dragSource = FindVisualParent<ListBox>(((DependencyObject)sender));

                var listBoxItem = FindVisualParent<ListBoxItem>(((DependencyObject)e.OriginalSource));
                if (listBoxItem != null)
                {
                    DragDrop.DoDragDrop(listBoxItem, listBoxItem.DataContext, DragDropEffects.Move);
                }
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

        private void DragDropDrop(object sender, DragEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                Dispatcher.Invoke(new Action(() => 
                {
                    var source = (int)e.Data.GetData(typeof(int));
                    var target = (int)((ListBoxItem)(sender)).DataContext;

                    var sourceList = allCards;
                    if (dragSource.Name == "allList2") sourceList = allCards2;

                    var targetBox = FindVisualParent<ListBox>(((DependencyObject)sender));
                    var targetList = allCards;
                    if (targetBox.Name == "allList2") targetList = allCards2;

                    int sourceIndex = dragSource.Items.IndexOf(source);
                    int targetIndex = targetBox.Items.IndexOf(target);

                    if (dragSource.Name != targetBox.Name)
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
                        if (allCards.Count + 1 > removeIndex)
                        {
                            targetList.Insert(targetIndex, source);
                            sourceList.RemoveAt(removeIndex);
                        }
                    }
                    allList.ItemsSource = allCards.ToList();
                    if (allCards2 != null) allList2.ItemsSource = allCards2.ToList();
                    e.Handled = true;
                    AllowSelect = (_min <= allCards.Count && allCards.Count <= _max);
                }));
            }
        }


    }
}