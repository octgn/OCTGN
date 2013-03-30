namespace Octgn.Core.Plugin
{
    using System;

    /// <summary>
    /// Base interface for creating plugins.
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// Plugin ID
        /// </summary>
        Guid Id { get; }
        /// <summary>
        /// Name of the plugin
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Version of the plugin.
        /// </summary>
        Version Version { get; }
        /// <summary>
        /// Required minimum version of OCTGN for this plugin.
        /// </summary>
        Version RequiredByOctgnVersion { get; }
    }
}