using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Octgn.Sdk.Extensibility.Desktop
{
    [XmlRoot("MenuItem")]
    public class MenuItem
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlAttribute("Text")]
        public string Text { get; set; }

        [XmlAttribute("Click")]
        public string Click { get; set; }

        public virtual Task OnClick() {
            return Task.CompletedTask;
        }
    }
}
