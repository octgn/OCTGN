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

namespace Octgn.Scripting.Controls
{
    using Octgn.DataNew.Entities;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Windows.Media.Imaging;

    public partial class MarkerDlg
    {
        public static readonly DependencyProperty IsModelSelectedProperty =
            DependencyProperty.Register("IsModelSelected", typeof (bool), typeof (MarkerDlg),
                                        new UIPropertyMetadata(false));

        private readonly ICollectionView _allMarkersView;
        private string _filterText = "";

        public MarkerDlg()
        {
            InitializeComponent();
            _allMarkersView = CollectionViewSource.GetDefaultView(Program.GameEngine.Definition.Markers.Values.ToList());
            _allMarkersView.Filter =
                marker =>
                ((GameMarker)marker).Name.IndexOf(_filterText, StringComparison.CurrentCultureIgnoreCase) >= 0;
            _allMarkersView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            allList.ItemsSource = _allMarkersView;
            defaultList.ItemsSource = Marker.DefaultMarkers;
            recentList.ItemsSource = Program.GameEngine.RecentMarkers;
        }

        public bool IsModelSelected
        {
            get { return (bool) GetValue(IsModelSelectedProperty); }
            set { SetValue(IsModelSelectedProperty, value); }
        }

        public GameMarker MarkerModel { get; private set; }

        public int Quantity
        {
            get { return int.Parse(quantityBox.Text); }
        }

        private void AddClicked(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            // A double-click can only select a marker in its own list
            // (TODO bug - double-clicking in the empty zone of a list with a selected marker adds it)
            if (sender is ListBox && ((ListBox) sender).SelectedIndex == -1) return;

            if (recentList.SelectedIndex != -1) MarkerModel = (GameMarker)recentList.SelectedItem;
            if (allList.SelectedIndex != -1) MarkerModel = (GameMarker)allList.SelectedItem;
            if (defaultList.SelectedIndex != -1)
            {
                var m = ((DefaultMarkerModel) defaultList.SelectedItem);
                m.Name = nameBox.Text;
                MarkerModel = (DefaultMarkerModel)m.Clone();
            }

            if (MarkerModel == null) return;

            int qty;
            if (!int.TryParse(quantityBox.Text, out qty) || qty < 0)
            {
                var anim = new ColorAnimation(Colors.Red, new Duration(TimeSpan.FromMilliseconds(800)))
                               {AutoReverse = true};
                validationBrush.BeginAnimation(SolidColorBrush.ColorProperty, anim, HandoffBehavior.Compose);
                return;
            }

            Program.GameEngine.AddRecentMarker(MarkerModel);
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
            _filterText = filterBox.Text;
            _allMarkersView.Refresh();
        }

        private void PreviewFilterKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Escape || filterBox.Text.Length <= 0) return;
            filterBox.Clear();
            e.Handled = true;
        }
    }
}