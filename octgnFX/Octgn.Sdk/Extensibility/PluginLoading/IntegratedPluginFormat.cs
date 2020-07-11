using System;
using System.Linq;
using System.Reflection;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    public class IntegratedPluginFormat : IPluginFormat
    {
        public const string IdString = "octgn.plugin.format.integrated";

        public string Id { get; }

        public IntegratedPluginFormat() {
            Id = IdString;
        }

        public IPlugin Load(Package package, IPluginDetails pluginDetails, Type pluginType) {
            if (pluginDetails.Format != IdString)
                throw new UnsupportedPluginFormatException($"{nameof(IntegratedPluginFormat)} does not support the format {pluginDetails.Format}");

            var ass = typeof(IntegratedPluginFormat).Assembly;

            var types = ass
                .GetTypes()
                .Where(x => pluginType.IsAssignableFrom(x))
            ;

            foreach (var type in types) {
                var pluginDetailsAttribute = type.GetCustomAttribute<PluginDetailsAttribute>();

                if (pluginDetailsAttribute == null) {
                    continue;
                }

                if (pluginDetailsAttribute.Id != pluginDetails.Id) {
                    continue;
                }

                var plugin = (IPlugin)Activator.CreateInstance(type);

                plugin.Details = pluginDetails;

                plugin.Initialize(package);

                return plugin;
            }

            throw new PluginNotFoundException($"Could not load find integrated plugin {pluginDetails.Id}");
        }
    }
}
