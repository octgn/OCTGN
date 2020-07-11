using System;

namespace Octgn.Play
{
    public class DefaultMarkerModel : DataNew.Entities.GameMarker
    {
        private readonly string _key;

        public DefaultMarkerModel(string key, string id)
        {
            Id = id;
            _key = key;
            this.Source = "pack://application:,,,/Resources/Markers/" + _key + ".png";
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

        public override object Clone()
        {
            var result = new DefaultMarkerModel(_key, Id) {Name = Name != null ? Name.Clone() as string : null};
            return result;
        }
    }
}