using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

    public partial class SelectCardsDlg
    {
        public static readonly DependencyProperty IsCardSelectedProperty = DependencyProperty.Register(
            "IsCardSelected", typeof(bool), typeof(SelectCardsDlg), new UIPropertyMetadata(false));

        private List<Card> _allCards;
        private string _filterText = "";
        private IEnumerable<string> textProperties = Program.GameEngine.Definition.CustomProperties
                    .Where(p => !p.IgnoreText)
                    .Select(p => p.Name);

        public SelectCardsDlg(List<Card> cardList, string prompt, string title)
        {
            InitializeComponent();
            Title = title;
            this.Height = Math.Min(System.Windows.SystemParameters.PrimaryScreenHeight * 0.80, 660);
            this.Width = Math.Min(System.Windows.SystemParameters.PrimaryScreenWidth * 0.80, 660);
            promptLbl.Text = prompt;
            Task.Factory.StartNew(() =>
            {
                var game = GameManager.Get().GetById(Program.GameEngine.Definition.Id);
                if (cardList == null) cardList = new List<Card>();

                _allCards = cardList;
 
                Dispatcher.BeginInvoke(new Action(() => allList.ItemsSource = _allCards));
            });
        }

        public bool IsCardSelected
        {
            get { return (bool)GetValue(IsCardSelectedProperty); }
            set { SetValue(IsCardSelectedProperty, value); }
        }

        public Card SelectedCard { get; private set; }

        public int returnIndex { get; private set; }

        private void SelectClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            // A double-click can only select a marker in its own list
            // (Little bug here: double-clicking in the empty zone of a list with a selected marker adds it)
            if (sender is ListBox && ((ListBox)sender).SelectedIndex == -1) return;

            allList.ItemsSource = _allCards;

            if (allList.SelectedIndex != -1) SelectedCard = (Card)allList.SelectedItem;
            returnIndex = allList.SelectedIndex;

            if (SelectedCard == null) return;
            
            DialogResult = true;
        }

        private void CardSelected(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            var list = (ListBox)sender;
            if (list.SelectedIndex != -1)
            {
                if (list != allList) allList.SelectedIndex = -1;
            }
            IsCardSelected = allList.SelectedIndex != -1;
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            _filterText = filterBox.Text;
            if (string.IsNullOrEmpty(_filterText))
            {
                allList.ItemsSource = _allCards;
                return;
            }
            // Filter asynchronously (so the UI doesn't freeze on huge lists)
            if (_allCards == null) return;
            ThreadPool.QueueUserWorkItem(searchObj =>
                                             {
                                                 var search = (string)searchObj;
                                                 List<Card> filtered =
                                                     _allCards.Where(
                                                         m =>
                                                         m.RealName.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                                                         textProperties.Select(property => m.GetProperty(property)).
                                                            Where(propertyValue => propertyValue != null).Any(
                                                            propertyValue => propertyValue.ToString().IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0)
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
            var model = (img.DataContext as Card).Type.Model;
            if (model != null) ImageUtils.GetCardImage(model, x => img.Source = x);
        }

        private void ComputeChildWidth(object sender, RoutedEventArgs e)
        {
            var panel = sender as VirtualizingWrapPanel;
            if (panel != null) panel.ChildWidth = panel.ChildHeight * Program.GameEngine.Definition.CardSize.Width / Program.GameEngine.Definition.CardSize.Height;
        }
    }
}