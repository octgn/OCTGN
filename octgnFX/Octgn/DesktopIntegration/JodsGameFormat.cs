using Octgn.Sdk;
using Octgn.Sdk.Extensibility;
using Octgn.Sdk.Extensibility.PluginLoading;
using System;

namespace Octgn.DesktopIntegration
{
    public sealed class JodsGameFormat : Plugin, IPluginFormat
    {
        public const string FormatString = "octgn.jodsengine.gameformat";

        public IPlugin Load(Package package, IPluginDetails pluginDetails, Type pluginType) {
            if (pluginDetails.Format != FormatString)
                throw new UnsupportedPluginFormatException($"{typeof(JodsGameFormat).Name} does not support plugin format {pluginDetails.Format}");

            if (pluginType != typeof(JodsGamePlugin))
                throw new UnsupportedPluginTypeException($"{nameof(JodsGameFormat)} does not support plugin type {pluginType.Name}");

            return new JodsGamePlugin(pluginDetails);
        }
    }
}
