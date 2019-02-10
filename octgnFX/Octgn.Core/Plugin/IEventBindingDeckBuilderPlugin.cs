using Octgn.Core.DataManagers;

namespace Octgn.Core.Plugin
{
    public interface IEventBindingDeckBuilderPlugin : IDeckBuilderPlugin
    {
        /// <summary>
        /// Overload the OnLoad from IDeckBuilderPlugin also taking in the calling object, so that we can bind to its events.
        /// </summary>
        /// <param name="caller">Octgn.DeckBuilder.DeckBuilderWindow</param>
        /// <param name="gameManager"></param>
        void OnLoad(object caller, GameManager gameManager);
    }

}
