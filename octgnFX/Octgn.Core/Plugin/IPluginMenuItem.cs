namespace Octgn.Core.Plugin
{
    /// <summary>
    /// Interface for creating plugin menu items.
    /// </summary>
    public interface IPluginMenuItem
    {
        /// <summary>
        /// Name to display for the menu item.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Fired when this menu item is clicked.
        /// </summary>
        void OnClick(IDeckBuilderPluginController controller);
    }
}