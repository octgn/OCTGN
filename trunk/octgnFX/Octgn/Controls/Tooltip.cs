using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;

namespace Octgn.Controls
{
    internal static class Tooltip
    {
        private static ToolTip ttip = new ToolTip();

        private static DispatcherTimer timer;            

        static Tooltip()
        {
            timer = new DispatcherTimer(TimeSpan.FromMilliseconds(1500), DispatcherPriority.Normal,
                delegate { timer.Stop(); ttip.IsOpen = false; },
                Application.Current.Dispatcher);

            var fillBrush = new LinearGradientBrush(Color.FromRgb(223, 20, 20), Color.FromRgb(255, 116, 116), 90);
            fillBrush.Freeze();

            ttip.BorderBrush = Brushes.Black;
            ttip.BorderThickness = new Thickness(1);
            ttip.Background = fillBrush;
            ttip.Placement = PlacementMode.MousePoint;
            ttip.Padding = new Thickness(20, 8, 20, 8);
        }

        public static void PopupError(string msg)
        {
            timer.Stop();
            ttip.IsOpen = false;
            ttip.Content = msg;            
            ttip.IsOpen = true;
            timer.Start();
        }
    }
}
