using System;
using Octgn.Data;

namespace Octgn.Play
{
    public class DefaultMarkerModel : DataNew.Entities.Marker
    {
        private readonly string _key;

        public DefaultMarkerModel(string key, Guid id)
        {
            Id = id;
            _key = key;
            this.IconUri = "pack://application:,,,/Resources/Markers/" + _key + ".png";
        }

        public string Picture
        {
            get { return "pack://application:,,,/Resources/Markers/" + _key + ".png"; }
            
        }



        public override bool Equals(object obj)
        {
            var other = obj as DefaultMarkerModel;
            if (other == null) return false;
            return other.Id == Id && other.Name == Name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ (Name != null ? Name.GetHashCode() : 0);
        }

        public void SetName(string lName)
        {
            Name = lName;
        }

        public DefaultMarkerModel Clone()
        {
            var result = new DefaultMarkerModel(_key, Id) {Name = Name};
            return result;
        }
    }
}