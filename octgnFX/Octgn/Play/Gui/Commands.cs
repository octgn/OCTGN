using System.Windows.Input;

namespace Octgn.Play.Gui
{
    public static class Commands
    {
        public readonly static RoutedUICommand Quit, FullScreen, ResetGame, Chat;
        public readonly static RoutedUICommand LoadDeck, NewDeck, SaveDeck, SaveDeckAs;
        public readonly static RoutedUICommand LimitedGame;

        static Commands()
        {
            Quit = new RoutedUICommand("Quit game", "Quit", typeof(Commands));

            LoadDeck = new RoutedUICommand("Load a deck", "LoadDeck", typeof(Commands));
            LoadDeck.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control));

            NewDeck = new RoutedUICommand("Create a new empty deck", "NewDeck", typeof(Commands));
            NewDeck.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));

            SaveDeck = new RoutedUICommand("Save the deck", "SaveDeck", typeof(Commands));
            SaveDeck.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));

            SaveDeckAs = new RoutedUICommand("Save the deck under a new name", "SaveDeckAs", typeof(Commands));
            SaveDeckAs.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control | ModifierKeys.Shift));

            FullScreen = new RoutedUICommand("Full Screen", "FullScreen", typeof(Commands));
            FullScreen.InputGestures.Add(new KeyGesture(Key.F11));

            ResetGame = new RoutedUICommand("Reset game", "ResetGame", typeof(Commands));

            Chat = new RoutedUICommand("Activate chat textbox", "Chat", typeof(Commands));
            Chat.InputGestures.Add(new KeyGesture(Key.T, ModifierKeys.Control));

            LimitedGame = new RoutedUICommand("Start a limited game", "LimitedGame", typeof(Commands));
            LimitedGame.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control | ModifierKeys.Shift));
        }
    }
}