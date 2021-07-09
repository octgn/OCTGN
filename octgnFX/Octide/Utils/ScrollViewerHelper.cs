// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Octide
{
    public static class ScrollViewerHelper
    {
        #region autoscroller
        public static bool GetScrollSelectedIntoView(ItemsControl control)
        {
            return (bool)control.GetValue(ScrollSelectedIntoViewProperty);
        }

        public static void SetScrollSelectedIntoView(ItemsControl control, bool value)
        {
            control.SetValue(ScrollSelectedIntoViewProperty, value);
        }

        public static readonly DependencyProperty ScrollSelectedIntoViewProperty =
            DependencyProperty.RegisterAttached("ScrollSelectedIntoView", typeof(bool), typeof(ScrollViewerHelper),
                                                new UIPropertyMetadata(false, OnScrollSelectedIntoViewChanged));

        private static void OnScrollSelectedIntoViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Selector selector)) return;

            if (e.NewValue is bool == false)
                return;

            if ((bool)e.NewValue)
            {
                selector.AddHandler(Selector.SelectionChangedEvent, new RoutedEventHandler(SelectionChangedHandler));
            }
            else
            {
                selector.RemoveHandler(Selector.SelectionChangedEvent, new RoutedEventHandler(SelectionChangedHandler));
            }
        }

        private static void SelectionChangedHandler(object sender, RoutedEventArgs e)
        {
            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItem != null)
                {
                    listBox.Dispatcher.BeginInvoke(
                        (Action)(() =>
                        {
                            listBox.UpdateLayout();
                            if (listBox.SelectedItem != null)
                                listBox.ScrollIntoView(listBox.SelectedItem);
                        }));
                }
            }
        }
        #endregion

        #region horizontalscrollwheel
        public static readonly DependencyProperty ShiftWheelScrollsHorizontallyProperty
            = DependencyProperty.RegisterAttached("ShiftWheelScrollsHorizontally",
                typeof(bool),
                typeof(ScrollViewerHelper),
                new PropertyMetadata(false, UseHorizontalScrollingChangedCallback));

        private static void UseHorizontalScrollingChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is UIElement element))
                throw new Exception("Attached property must be used with UIElement.");

            if ((bool)e.NewValue)
                element.PreviewMouseWheel += OnPreviewMouseWheel;
            else
                element.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args)
        {
            var scrollViewer = ((UIElement)sender).FindDescendant<ScrollViewer>();

            if (scrollViewer == null)
                return;

            if (args.Delta < 0)
                scrollViewer.LineRight();
            else
                scrollViewer.LineLeft();

            args.Handled = true;
        }

        public static void SetShiftWheelScrollsHorizontally(ItemsControl element, bool value) => element.SetValue(ShiftWheelScrollsHorizontallyProperty, value);
        public static bool GetShiftWheelScrollsHorizontally(ItemsControl element) => (bool)element.GetValue(ShiftWheelScrollsHorizontallyProperty);


        private static T FindDescendant<T>(this DependencyObject d) where T : DependencyObject
        {
            if (d == null)
                return null;

            var childCount = VisualTreeHelper.GetChildrenCount(d);

            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(d, i);

                var result = child as T ?? FindDescendant<T>(child);

                if (result != null)
                    return result;
            }

            return null;
        }
        #endregion
    }
}
