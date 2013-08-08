using System.Windows.Input;

namespace Octgn.Controls
{
    public static class TabCommands
    {
        public static readonly RoutedUICommand NewTabCommand = new RoutedUICommand("New tab", "NewTabCommand",
                                                                                   typeof (TabCommands));

        public static readonly RoutedUICommand CloseTabCommand = new RoutedUICommand("Close tab", "CloseTabCommand",
                                                                                     typeof (TabCommands));

        static TabCommands()
        {
            CloseTabCommand.InputGestures.Add(new KeyGesture(Key.F4, ModifierKeys.Control));
            CloseTabCommand.InputGestures.Add(new KeyGesture(Key.W, ModifierKeys.Control));
        }
    }
}