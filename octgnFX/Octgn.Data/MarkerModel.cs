using System;
using System.Xml;

namespace Octgn.Data
{
    public class MarkerModel
    {
        private readonly string _iconUri;
        private readonly Set _set;
        public Guid Id;
        // TODO: Should be renamed at some point
        protected string name;

        public MarkerModel(XmlReader reader, Set set)
        {
            reader.MoveToAttribute("name");
            name = reader.Value;
            reader.MoveToAttribute("id");
            Id = new Guid(reader.Value);
            reader.Read(); // <marker />
            _set = set;
        }

        protected MarkerModel(Guid id)
        {
            Id = id;
        }

        public MarkerModel(Guid id, string name, string icon, Set set)
        {
            _set = set;
            _iconUri = icon;
            this.name = name;
            Id = id;
        }

        public virtual string Picture
        {
            get { return _set.GetPackUri() + _iconUri; }
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