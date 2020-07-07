using System.Collections.Generic;
using System.Xml.Serialization;

namespace Octgn.Sdk.Extensibility.Desktop
{
    [PluginDetails(PluginTypeName)]
    [XmlRoot("Menu")]
    public class MenuPlugin : Plugin
    {
        public const string PluginTypeName = "octgn.plugin.menu";

        [XmlElement("MenuItem")]
        public List<MenuItem> MenuItems { get; set; }

        [XmlAttribute("Game")]
        public string Game { get; set; }
    }
}
