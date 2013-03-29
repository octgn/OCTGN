namespace Octgn.Core.Plugin
{
    using Octgn.Core.DataManagers;
    using Octgn.DataNew.Entities;

    /// <summary>
    /// Interface for controlling the deck editor.
    /// </summary>
    public interface IDeckBuilderPluginController
    {
        /// <summary>
        /// Game repository of installed games.
        /// </summary>
        GameManager Games { get; }
        /// <summary>
        /// Set the loaded game in the Deck Editor
        /// </summary>
        /// <param name="game"></param>
        void SetLoadedGame(Game game);
        /// <summary>
        /// Get the loaded game in the Deck Editor
        /// </summary>
        /// <param name="game"></param>
        Game GetLoadedGame();
        /// <summary>
        /// Load a deck into the Deck Editor
        /// </summary>
        /// <param name="deck"></param>
        void LoadDeck(IDeck deck);
        /// <summary>
        /// Gets the deck currently being edited.
        /// </summary>
        /// <returns></returns>
        IDeck GetLoadedDeck();
    }
}