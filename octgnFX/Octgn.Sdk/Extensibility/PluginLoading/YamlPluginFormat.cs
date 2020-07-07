using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    [PluginDetails(IdString)]
    public class YamlPluginFormat : Plugin, IPluginFormat
    {
        public const string IdString = "octgn.plugin.format.yaml";

        public string Id => base.Details.Id;

        private readonly IDeserializer _deserializer;

        public YamlPluginFormat() {
            var builder = new DeserializerBuilder()
                .WithNamingConvention(LowerCaseNamingConvention.Instance)
            ;

            _deserializer = builder.Build();
        }

        public IPlugin Load(Package package, IPluginDetails pluginDetails, Type pluginType) {
            if (pluginDetails.Format != IdString)
                throw new UnsupportedPluginFormatException($"{nameof(YamlPluginFormat)} does not support the format {pluginDetails.Format}");

            IPlugin plugin;
            using (var reader = new StreamReader(File.OpenRead(pluginDetails.Path))) {
                plugin = (IPlugin)_deserializer.Deserialize(reader, pluginType);
            }

            plugin.Details = pluginDetails;

            plugin.Initialize(package);

            return plugin;
        }
    }
}
