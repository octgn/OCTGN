namespace Octgn.Plugin
{
    using System;

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
        Action<IDeckBuilderPluginController> OnClick { get; }
    }
}