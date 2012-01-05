using System;

namespace Octgn
{
	public class DefaultMarkerModel : Data.MarkerModel
	{
		private string key;

		public override string Picture
		{
			get
			{ return "pack://application:,,,/Resources/Markers/" + key + ".png"; }
		}

		public DefaultMarkerModel(string key, Guid id)
			: base(id)
		{
			this.key = key;
		}

		public override bool Equals(object obj)
		{
			DefaultMarkerModel other = obj as DefaultMarkerModel;
			if (other == null) return false;
			return other.id == id && other.name == name;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode() ^ (name != null ? name.GetHashCode() : 0);
		}

		public void SetName(string name)
		{ this.name = name; }

		public DefaultMarkerModel Clone()
		{
			DefaultMarkerModel result = new DefaultMarkerModel(key, id);
			result.name = this.name;
			return result;
		}
	}
}
