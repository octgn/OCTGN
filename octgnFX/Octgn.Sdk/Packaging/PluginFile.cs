using System.Xml.Serialization;

namespace Octgn.Sdk.Packaging
{
    [XmlRoot("Plugin")]
    public class PluginFile : IPluginDetails
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Description")]
        public string Description { get; set; }

        [XmlAttribute("Path")]
        public string Path { get; set; }

        [XmlAttribute("Icon")]
        public string Icon { get; set; }

        [XmlAttribute("Type")]
        public string Type { get; set; }

        [XmlAttribute("Format")]
        public string Format { get; set; }
    }
}
