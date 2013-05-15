namespace Octgn
{
    using Octgn.DeckBuilder;
    using Octgn.Play;
    using Octgn.Windows;

    public static class WindowManager
    {
        public static Main Main { get; set; }
        public static DeckBuilderWindow DeckEditor { get; set; }
        public static PlayWindow PlayWindow { get; set; }
        public static PreGameLobbyWindow PreGameLobbyWindow { get; set; }
    }
}