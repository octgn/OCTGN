using System;
using System.Xml;

namespace Octgn.Data
{
    public class MarkerModel
    {
        private readonly string iconUri;
        private readonly Set set;
        public Guid id;
        protected string name;

        public MarkerModel(XmlReader reader, Set set)
        {
            reader.MoveToAttribute("name");
            name = reader.Value;
            reader.MoveToAttribute("id");
            id = new Guid(reader.Value);
            reader.Read(); // <marker />
            this.set = set;
        }

        protected MarkerModel(Guid id)
        {
            this.id = id;
        }

        public MarkerModel(Guid id, string name, string icon, Set set)
        {
            this.set = set;
            iconUri = icon;
            this.name = name;
            this.id = id;
        }

        public virtual string Picture
        {
            get { return set.GetPackUri() + iconUri; }
        }

        public string Name
        {
            get { return name; }
        }

        public override string ToString()
        {
            return name;
        }
    }
}