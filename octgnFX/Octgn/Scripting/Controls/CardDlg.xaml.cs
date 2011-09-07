using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media.Animation;
using Octgn.Data;
using Octgn.Controls;
using System.Threading.Tasks;

namespace Octgn.Script
{
	public partial class CardDlg : Window
	{
		public static readonly DependencyProperty IsCardSelectedProperty = DependencyProperty.Register("IsCardSelected", typeof(bool), typeof(CardDlg), new UIPropertyMetadata(false));
		public bool IsCardSelected
		{
			get { return (bool)GetValue(IsCardSelectedProperty); }
			set { SetValue(IsCardSelectedProperty, value); }
		}

		private Data.CardModel result;
		private string filterText = "";
    private List<CardModel> allCards;

		public Data.CardModel SelectedCard
		{ get { return result; } }

		public int Quantity
		{ get { return int.Parse(quantityBox.Text); } }

    public CardDlg(string where)
    {
      InitializeComponent();
      // Async load the cards (to make the GUI snappier with huge DB)
      Task.Factory.StartNew(() =>
      {
        allCards = Database.GetCards(where).ToList();
        Dispatcher.BeginInvoke(new System.Action(() => allList.ItemsSource = allCards));
      });
      recentList.ItemsSource = Program.Game.RecentCards;
    }

		private void CreateClicked(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			// A double-click can only select a marker in its own list
			// (Little bug here: double-clicking in the empty zone of a list with a selected marker adds it)
			if (sender is ListBox && ((ListBox)sender).SelectedIndex == -1) return;

			if (recentList.SelectedIndex != -1) result = (Data.CardModel)recentList.SelectedItem;
			if (allList.SelectedIndex != -1) result = (Data.CardModel)allList.SelectedItem;

			if (result == null) return;

			int qty;
			if (!int.TryParse(quantityBox.Text, out qty) || qty < 1)
			{
				var anim = new ColorAnimation(Colors.Red, new Duration(TimeSpan.FromMilliseconds(800))) { AutoReverse = true };
				validationBrush.BeginAnimation(SolidColorBrush.ColorProperty, anim, HandoffBehavior.Compose);
				return;
			}

			Program.Game.AddRecentCard(result);
			DialogResult = true;
		}

		private void CardSelected(object sender, SelectionChangedEventArgs e)
		{
			e.Handled = true;
			ListBox list = (ListBox)sender;
			if (list.SelectedIndex != -1)
			{
				if (list != recentList) recentList.SelectedIndex = -1;
				if (list != allList) allList.SelectedIndex = -1;
			}
			IsCardSelected = recentList.SelectedIndex != -1 || allList.SelectedIndex != -1;
		}

		private void FilterChanged(object sender, EventArgs e)
		{
			filterText = filterBox.Text;
      if (string.IsNullOrEmpty(filterText))
      {
        allList.ItemsSource = allCards;
        return;
      }
      // Filter asynchronously (so the UI doesn't freeze on huge lists)
      // TODO .NET 4: use PLINQ to make this filter more efficient, and include cancellation of unrequired work
      if (allCards == null) return;
      System.Threading.ThreadPool.QueueUserWorkItem((searchObj) =>
        {
          string search = (string)searchObj;         
          var filtered = allCards.Where(m => m.Name.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0).ToList();
          if (search == filterText)
            Dispatcher.Invoke(new System.Action(() => allList.ItemsSource = filtered));
        }, filterText);
		}

		private void PreviewFilterKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape && filterBox.Text.Length > 0)
			{
				filterBox.Clear();
				e.Handled = true;
			}
		}

    private void SetPicture(object sender, RoutedEventArgs e)
    {
      var img = sender as Image;
      var model = img.DataContext as CardModel;
      ImageUtils.GetCardImage(new Uri(model.Picture), x => img.Source = x);
    }

    private void ComputeChildWidth(object sender, RoutedEventArgs e)
    {
      var panel = sender as VirtualizingWrapPanel;
      var cardDef = Program.Game.Definition.CardDefinition;
      panel.ChildWidth = panel.ChildHeight * cardDef.Width / cardDef.Height;
    }
	}
}
