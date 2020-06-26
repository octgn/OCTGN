using System.Collections.Generic;
using System.Xml.Serialization;

namespace Octgn.Sdk.Packaging
{
    [XmlRoot("Package")]
    public class PackageFile
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlAttribute("Description")]
        public string Description { get; set; }

        [XmlIgnore]
        public string Path { get; set; }

        [XmlAttribute("Website")]
        public string Website { get; set; }

        [XmlAttribute("Icon")]
        public string Icon { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }

        [XmlAttribute("OctgnVersion")]
        public string OctgnVersion { get; set; }

        [XmlArray("Plugins")]
        [XmlArrayItem("Plugin")]
        public List<PluginFile> Plugins { get; set; }
    }
}
