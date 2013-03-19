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
using Octgn.Definitions;
using Octgn.Utils;

namespace Octgn.Scripting.Controls
{
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Core.DataManagers;

    public partial class CardDlg
    {
        public static readonly DependencyProperty IsCardSelectedProperty = DependencyProperty.Register(
            "IsCardSelected", typeof (bool), typeof (CardDlg), new UIPropertyMetadata(false));

        private List<DataNew.Entities.Card> _allCards;
        private string _filterText = "";

        public CardDlg(string where)
        {
            InitializeComponent();
            // Async load the cards (to make the GUI snappier with huge DB)
            //TODO [DB MIGRATION] Fuck card dlg
            throw new NotImplementedException("GAALSDKJFWE");
            Task.Factory.StartNew(() =>
                                      {
                                          //_allCards = Database.GetCards(where).ToList();
                                          //TODO [DB MIGRATION]  FIX THIS SHIT
                                          var game = GameManager.Get().GetById(Program.GameEngine.Definition.Id);
                                          foreach (string s in where.Split(','))
                                          {
                                              string name = s.Split('=')[0].Trim();
                                              string value = s.Split('=')[1].Trim();
                                              _allCards = _allCards.Concat(game.AllCards().Where(x => (x.Properties.Any(t => (t.Key.Name == name & t.Value.ToString().Equals(value)))))).Distinct().ToList();
                                          }
                                          Dispatcher.BeginInvoke(new Action(() => allList.ItemsSource = _allCards));
                                      });
            recentList.ItemsSource = Program.GameEngine.RecentCards;
        }

        public bool IsCardSelected
        {
            get { return (bool) GetValue(IsCardSelectedProperty); }
            set { SetValue(IsCardSelectedProperty, value); }
        }

        public DataNew.Entities.Card SelectedCard { get; private set; }

        public int Quantity
        {
            get { return int.Parse(quantityBox.Text); }
        }

        private void CreateClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            // A double-click can only select a marker in its own list
            // (Little bug here: double-clicking in the empty zone of a list with a selected marker adds it)
            if (sender is ListBox && ((ListBox) sender).SelectedIndex == -1) return;

            if (recentList.SelectedIndex != -1) SelectedCard = (DataNew.Entities.Card)recentList.SelectedItem;
            if (allList.SelectedIndex != -1) SelectedCard = (DataNew.Entities.Card)allList.SelectedItem;

            if (SelectedCard == null) return;

            int qty;
            if (!int.TryParse(quantityBox.Text, out qty) || qty < 1)
            {
                var anim = new ColorAnimation(Colors.Red, new Duration(TimeSpan.FromMilliseconds(800)))
                               {AutoReverse = true};
                validationBrush.BeginAnimation(SolidColorBrush.ColorProperty, anim, HandoffBehavior.Compose);
                return;
            }

            Program.GameEngine.AddRecentCard(SelectedCard);
            DialogResult = true;
        }

        private void CardSelected(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            var list = (ListBox) sender;
            if (list.SelectedIndex != -1)
            {
                if (list != recentList) recentList.SelectedIndex = -1;
                if (list != allList) allList.SelectedIndex = -1;
            }
            IsCardSelected = recentList.SelectedIndex != -1 || allList.SelectedIndex != -1;
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
                                                 var search = (string) searchObj;
                                                 List<DataNew.Entities.Card> filtered =
                                                     _allCards.Where(
                                                         m =>
                                                         m.Name.IndexOf(search,
                                                                        StringComparison.CurrentCultureIgnoreCase) >= 0)
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
            var model = img.DataContext as DataNew.Entities.Card;
            if (model != null) ImageUtils.GetCardImage(new Uri(model.GetPicture()), x => img.Source = x);
        }

        private void ComputeChildWidth(object sender, RoutedEventArgs e)
        {
            var panel = sender as VirtualizingWrapPanel;
            if (panel != null) panel.ChildWidth = panel.ChildHeight * Program.GameEngine.Definition.CardWidth / Program.GameEngine.Definition.CardHeight;
        }
    }
}