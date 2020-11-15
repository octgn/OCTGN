using System.Windows.Input;

namespace Octgn.Play.Gui
{
    public static class Commands
    {
        public static readonly RoutedUICommand Quit, FullScreen, ResetGame, SoftResetGame, Chat, ResetScreen;
        public static readonly RoutedUICommand LoadDeck, LoadPrebuiltDeck, NewDeck, SaveDeck, SaveDeckAs, ExportDeckAs;
        public static readonly RoutedUICommand LimitedGame, AlwaysShowProxy;

        static Commands()
        {
            Quit = new RoutedUICommand("Quit game", "Quit", typeof (Commands));

            LoadDeck = new RoutedUICommand("Load a deck", "LoadDeck", typeof (Commands));
            LoadDeck.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control));

            LoadPrebuiltDeck = new RoutedUICommand("Load a pre-built deck", "LoadPrebuiltDeck", typeof(Commands));

            NewDeck = new RoutedUICommand("Create a new empty deck", "NewDeck", typeof (Commands));
            NewDeck.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));

            SaveDeck = new RoutedUICommand("Save the deck", "SaveDeck", typeof (Commands));
            SaveDeck.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));

            SaveDeckAs = new RoutedUICommand("Save the deck under a new name", "SaveDeckAs", typeof (Commands));
            SaveDeckAs.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));

            ExportDeckAs = new RoutedUICommand("Export the deck as a txt file", "ExportDeckAs", typeof(Commands));
            ExportDeckAs.InputGestures.Add(new KeyGesture(Key.X, ModifierKeys.Control | ModifierKeys.Shift));

            FullScreen = new RoutedUICommand("Full Screen", "FullScreen", typeof (Commands));
            FullScreen.InputGestures.Add(new KeyGesture(Key.F11));

            ResetScreen = new RoutedUICommand("Reset Screen Position", "ResetScreen", typeof(Commands));
            ResetScreen.InputGestures.Add(new KeyGesture(Key.F10));

            AlwaysShowProxy = new RoutedUICommand("Always Show Proxy", "AlwaysShowProxy", typeof(Commands));
            AlwaysShowProxy.InputGestures.Add(new KeyGesture(Key.F12));

            ResetGame = new RoutedUICommand("Reset game", "ResetGame", typeof (Commands));
            SoftResetGame = new RoutedUICommand("Soft-reset game", "SoftResetGame", typeof (Commands));

            Chat = new RoutedUICommand("Activate chat textbox", "Chat", typeof (Commands));
            Chat.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));

            LimitedGame = new RoutedUICommand("Start a limited game", "LimitedGame", typeof (Commands));
            LimitedGame.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Shift));
        }
    }
}