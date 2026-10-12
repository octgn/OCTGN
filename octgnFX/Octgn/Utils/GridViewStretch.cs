using System;
using System.Windows;
using System.Windows.Controls;

namespace Octgn.Utils
{
    /// <summary>
    /// Lets one column of a <see cref="GridView"/> absorb whatever width the ListView has left,
    /// so tables use the full width of the window instead of leaving a gap on the right.
    /// </summary>
    public static class GridViewStretch
    {
        /// <summary>Zero-based index of the column that should stretch. -1 turns the behaviour off.</summary>
        public static readonly DependencyProperty StretchColumnProperty = DependencyProperty.RegisterAttached(
            "StretchColumn", typeof(int), typeof(GridViewStretch), new PropertyMetadata(-1, OnStretchColumnChanged));

        public static int GetStretchColumn(DependencyObject obj) => (int)obj.GetValue(StretchColumnProperty);

        public static void SetStretchColumn(DependencyObject obj, int value) => obj.SetValue(StretchColumnProperty, value);

        // Room taken by the row margins, the scroll bar and the header gripper
        private const double Reserved = 44;
        private const double MinColumnWidth = 120;

        private static void OnStretchColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var list = d as ListView;
            if (list == null) return;

            list.SizeChanged -= OnSizeChanged;
            list.Loaded -= OnLoaded;
            if ((int)e.NewValue < 0) return;

            list.SizeChanged += OnSizeChanged;
            list.Loaded += OnLoaded;
        }

        private static void OnLoaded(object sender, RoutedEventArgs e) => Apply((ListView)sender);

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e) => Apply((ListView)sender);

        private static void Apply(ListView list)
        {
            var grid = list.View as GridView;
            var index = GetStretchColumn(list);
            if (grid == null || index < 0 || index >= grid.Columns.Count || list.ActualWidth <= 0) return;

            double others = 0;
            for (var i = 0; i < grid.Columns.Count; i++)
            {
                if (i == index) continue;
                var width = grid.Columns[i].ActualWidth;
                others += double.IsNaN(width) ? 0 : width;
            }

            var target = Math.Max(MinColumnWidth, list.ActualWidth - others - Reserved);
            if (Math.Abs(grid.Columns[index].Width - target) > 0.5)
                grid.Columns[index].Width = target;
        }
    }
}