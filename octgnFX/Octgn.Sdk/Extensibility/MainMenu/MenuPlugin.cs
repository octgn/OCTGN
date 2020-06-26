using Octgn.Sdk.Packaging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Octgn.Sdk.Extensibility.MainMenu
{
    [XmlRoot("Menu")]
    public class MenuPlugin : IPlugin
    {
        [XmlIgnore]
        public IPackage Package { get; set; }

        [XmlElement("MenuItem")]
        public List<MenuItem> MenuItems { get; set; }

        public Task OnStart() {
            return Task.CompletedTask;
        }
    }
}
