using System.Xml.Serialization;

namespace Octgn.Sdk.Packaging
{
    public class DependencyFile : IDependency
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlAttribute("Version")]
        public string Version { get; set; }
    }
}