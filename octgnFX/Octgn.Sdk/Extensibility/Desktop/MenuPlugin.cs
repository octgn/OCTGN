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
        public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

        [XmlAttribute("Game")]
        public string Game { get; set; }

        public override void Initialize(Package package) {
            base.Initialize(package);

            foreach (var menuItem in MenuItems) {
                menuItem.Menu = this;
            }
        }
    }
}
