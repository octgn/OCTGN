using System;
using Octgn.Data;

namespace Octgn
{
    public class DefaultMarkerModel : MarkerModel
    {
        private readonly string key;

        public DefaultMarkerModel(string key, Guid id)
            : base(id)
        {
            this.key = key;
        }

        public override string Picture
        {
            get { return "pack://application:,,,/Resources/Markers/" + key + ".png"; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as DefaultMarkerModel;
            if (other == null) return false;
            return other.id == id && other.name == name;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode() ^ (name != null ? name.GetHashCode() : 0);
        }

        public void SetName(string lName)
        {
            name = lName;
        }

        public DefaultMarkerModel Clone()
        {
            var result = new DefaultMarkerModel(key, id);
            result.name = name;
            return result;
        }
    }
}