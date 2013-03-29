namespace Octgn.Core.Plugin
{
    using System.Collections.Generic;

    using Octgn.Core.DataManagers;

    ///<summary>
    /// Base interface for creating a Deck Builder plugin for OCTGN.
    /// </summary>
    public interface IDeckBuilderPlugin: IPlugin
    {
        /// <summary>
        /// Menu items to add for the plugin.
        /// </summary>
        IEnumerable<IPluginMenuItem> MenuItems { get; }
        /// <summary>
        /// Happens when the Deck Editor is opened.
        /// </summary>
        /// <param name="gameManager">Game repository.</param>
        void OnLoad(GameManager gameManager);
    }
}