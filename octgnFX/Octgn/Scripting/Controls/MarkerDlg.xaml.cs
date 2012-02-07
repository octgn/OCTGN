using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Octgn.Data;
using Octgn.Play;

namespace Octgn.Script
{
    public partial class MarkerDlg : Window
    {
        public static readonly DependencyProperty IsModelSelectedProperty =
            DependencyProperty.Register("IsModelSelected", typeof (bool), typeof (MarkerDlg),
                                        new UIPropertyMetadata(false));

        private readonly ICollectionView allMarkersView;
        private string filterText = "";
        private MarkerModel result;

        public MarkerDlg()
        {
            InitializeComponent();
            allMarkersView = CollectionViewSource.GetDefaultView(Program.Game.Markers);
            allMarkersView.Filter =
                marker =>
                ((MarkerModel) marker).Name.IndexOf(filterText, StringComparison.CurrentCultureIgnoreCase) >= 0;
            allList.ItemsSource = allMarkersView;
            defaultList.ItemsSource = Marker.DefaultMarkers;
            recentList.ItemsSource = Program.Game.RecentMarkers;
        }

        public bool IsModelSelected
        {
            get { return (bool) GetValue(IsModelSelectedProperty); }
            set { SetValue(IsModelSelectedProperty, value); }
        }

        public MarkerModel MarkerModel
        {
            get { return result; }
        }

        public int Quantity
        {
            get { return int.Parse(quantityBox.Text); }
        }

        private void AddClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            // A double-click can only select a marker in its own list
            // (Little bug here: double-clicking in the empty zone of a list with a selected marker adds it)
            if (sender is ListBox && ((ListBox) sender).SelectedIndex == -1) return;

            if (recentList.SelectedIndex != -1) result = (MarkerModel) recentList.SelectedItem;
            if (allList.SelectedIndex != -1) result = (MarkerModel) allList.SelectedItem;
            if (defaultList.SelectedIndex != -1)
            {
                var m = ((DefaultMarkerModel) defaultList.SelectedItem);
                m.SetName(nameBox.Text);
                result = m.Clone();
            }

            if (result == null) return;

            int qty;
            if (!int.TryParse(quantityBox.Text, out qty) || qty < 0)
            {
                var anim = new ColorAnimation(Colors.Red, new Duration(TimeSpan.FromMilliseconds(800)))
                               {AutoReverse = true};
                validationBrush.BeginAnimation(SolidColorBrush.ColorProperty, anim, HandoffBehavior.Compose);
                return;
            }

            Program.Game.AddRecentMarker(result);
            DialogResult = true;
        }

        private void MarkerSelected(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
            var list = (ListBox) sender;
            if (list.SelectedIndex != -1)
            {
                if (list != recentList) recentList.SelectedIndex = -1;
                if (list != allList) allList.SelectedIndex = -1;
                if (list != defaultList) defaultList.SelectedIndex = -1;
            }
            IsModelSelected = recentList.SelectedIndex != -1 || allList.SelectedIndex != -1 ||
                              defaultList.SelectedIndex != -1;
        }

        private void FilterChanged(object sender, EventArgs e)
        {
            filterText = filterBox.Text;
            allMarkersView.Refresh();
        }

        private void PreviewFilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && filterBox.Text.Length > 0)
            {
                filterBox.Clear();
                e.Handled = true;
            }
        }
    }
}