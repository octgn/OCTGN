using System;
using System.Linq;
using System.Reflection;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    [PluginDetails(IdString)]
    public class DotNetPluginFormat : Plugin, IPluginFormat
    {
        public const string IdString = "octgn.plugin.format.dotnet";

        public string Id => base.Details.Id;

        public IPlugin Load(Package package, IPluginDetails pluginDetails, Type pluginType) {
            if (pluginDetails.Format != IdString)
                throw new UnsupportedPluginFormatException($"{typeof(DotNetPluginFormat).Name} does not support plugin format {pluginDetails.Format}");

            var ass = package.LoadAssembly(pluginDetails.Path);

            var allTypes = ass.GetExportedTypes().ToArray();

            var assemblyPluginTypes = allTypes
                .Where(x => !x.IsAbstract && !x.IsInterface)
                .Where(x => pluginType.IsAssignableFrom(x))
                .ToArray()
            ;

            foreach (var assemblyPluginType in assemblyPluginTypes) {
                var pluginDetailsAttribute = assemblyPluginType.GetCustomAttribute<PluginDetailsAttribute>();

                if (pluginDetailsAttribute == null) {
                    continue;
                }

                if (pluginDetailsAttribute.Id != pluginDetails.Id) {
                    continue;
                }

                var plugin = (IPlugin)Activator.CreateInstance(assemblyPluginType);

                plugin.Details = pluginDetails;

                plugin.Initialize(package);

                return plugin;
            }

            throw new PluginNotFoundException($"Could not load find dotnet plugin {pluginDetails.Id} in {pluginDetails.Path}");
        }
    }
}
