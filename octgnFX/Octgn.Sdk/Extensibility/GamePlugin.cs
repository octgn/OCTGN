using System;
using System.Xml.Serialization;

namespace Octgn.Sdk.Extensibility
{
    [PluginDetails(PluginTypeName)]
    [XmlRoot("Game")]
    public class GamePlugin : Plugin
    {
        public const string PluginTypeName = "octgn.plugin.game";
    }
}
