using System;
using Octgn.Data;

namespace Octgn.Play
{
    public class DefaultMarkerModel : MarkerModel
    {
        private readonly string _key;

        public DefaultMarkerModel(string key, Guid id)
            : base(id)
        {
            _key = key;
        }

        public override string Picture
        {
            get { return "pack://application:,,,/Resources/Markers/" + _key + ".png"; }
        }

        public override bool Equals(object obj)
        {
            var other = obj as DefaultMarkerModel;
            if (other == null) return false;
            return other.Id == Id && other.name == name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ (name != null ? name.GetHashCode() : 0);
        }

        public void SetName(string lName)
        {
            name = lName;
        }

        public DefaultMarkerModel Clone()
        {
            var result = new DefaultMarkerModel(_key, Id) {name = name};
            return result;
        }
    }
}