using Octgn.Sdk.Data;
using Octgn.Sdk.Extensibility.MainMenu;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Octgn.Sdk.Extensibility
{
    public class PluginIntegration
    {
        private readonly XmlSerializer _serializer;

        public PluginIntegration() {
            _serializer = new XmlSerializer(typeof(MenuPlugin));
        }

        public T Load<T>(PackageRecord packageRecord, PluginRecord pluginRecord) where T : IPlugin {
            var rootPath = Path.GetDirectoryName(packageRecord.Path);

            var path = Path.Combine(rootPath, pluginRecord.Path);

            if (!File.Exists(path))
                throw new FileNotFoundException($"File {path} not found");

            IPlugin menuPlugin;
            using (var stream = File.OpenRead(path)) {
                menuPlugin = (MenuPlugin)_serializer.Deserialize(stream);
            }

            var t = (T)menuPlugin;

            return t;
        }
    }
}
