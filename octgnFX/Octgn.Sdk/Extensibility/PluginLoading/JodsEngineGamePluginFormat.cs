using Octgn.Sdk.Data;
using System;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    [PluginDetails(IdString)]
    public class JodsEngineGamePluginFormat : Plugin, IPluginFormat
    {
        public const string IdString = "octgn.jodsengine.gameformat";

        public string Id => Details.Id;

        public JodsEngineGamePluginFormat() {
            Details = new PluginRecord() {
                Id = IdString,
                Name = "Jods Engine Game Format",
                Description = "asdf",
                Format = IntegratedPluginFormat.IdString,
                PackageId = "octgn.sdk",
                PackageVersion = "3.4.0.0",
                Path = "integrated",
                Type = "octgn.plugin"
            };
        }

        public IPlugin Load(Package package, IPluginDetails pluginDetails, Type pluginType) {
            if (pluginDetails.Format != IdString)
                throw new UnsupportedPluginFormatException($"{typeof(JodsEngineGamePluginFormat).Name} does not support plugin format {pluginDetails.Format}");

            //TODO
            return new Plugin();
        }
    }
}