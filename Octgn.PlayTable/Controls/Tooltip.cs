namespace Octgn.PlayTable.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;
    using System.Windows.Threading;

    internal static class Tooltip
    {
        private static readonly ToolTip Ttip = new ToolTip();
        private static readonly DispatcherTimer Timer;

        static Tooltip()
        {
            Timer = new DispatcherTimer(TimeSpan.FromMilliseconds(1500), DispatcherPriority.Normal,
                                        delegate
                                            {
                                                Timer.Stop();
                                                Ttip.IsOpen = false;
                                            },
                                        Application.Current.Dispatcher);

            var fillBrush = new LinearGradientBrush(Color.FromRgb(223, 20, 20), Color.FromRgb(255, 116, 116), 90);
            fillBrush.Freeze();

            Ttip.BorderBrush = Brushes.Black;
            Ttip.BorderThickness = new Thickness(1);
            Ttip.Background = fillBrush;
            Ttip.Placement = PlacementMode.MousePoint;
            Ttip.Padding = new Thickness(20, 8, 20, 8);
        }

        public static void PopupError(string msg)
        {
            Timer.Stop();
            Ttip.IsOpen = false;
            Ttip.Content = msg;
            Ttip.IsOpen = true;
            Timer.Start();
        }
    }
}