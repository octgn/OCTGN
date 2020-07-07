using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Octgn.Sdk.Extensibility.PluginLoading
{
    [PluginDetails(IdString)]
    public class XmlPluginFormat : Plugin, IPluginFormat, ITypeRegistration
    {
        public const string IdString = "octgn.plugin.format.xml";

        public string Id => base.Details.Id;

        public IEnumerable<(Type Type, Type[] AdditionalTypes)> Registrations => _serializers.Select(x => (x.Key, x.Value.additionalTypes)).ToArray();

        private readonly Dictionary<Type, (XmlSerializer serializer, Type[] additionalTypes)> _serializers
             = new Dictionary<Type, (XmlSerializer serializer, Type[] additionalTypes)>();

        public void Register(Type pluginType, params Type[] relatedTypes) {
            _serializers[pluginType] = (new XmlSerializer(pluginType, relatedTypes), relatedTypes);
        }

        public void Unregister(Type pluginType) {
            _serializers.Remove(pluginType);
        }

        public IPlugin Load(Package package, IPluginDetails pluginDetails, Type pluginType) {
            if (pluginDetails.Format != IdString)
                throw new UnsupportedPluginFormatException($"{nameof(XmlPluginFormat)} does not support the format {pluginDetails.Format}");

            if (!_serializers.TryGetValue(pluginType, out var serializerRegistration))
                throw new UnsupportedPluginTypeException($"{nameof(XmlPluginFormat)} does not support plugin type {pluginType.Name}");

            var serializer = serializerRegistration.serializer;

            IPlugin plugin;
            using (var stream = File.OpenRead(pluginDetails.Path)) {
                plugin = (IPlugin)serializer.Deserialize(stream);
            }

            plugin.Details = pluginDetails;


            plugin.Initialize(package);

            return plugin;
        }
    }
}
