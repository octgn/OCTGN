using System;
using System.Xml;
using VistaDB.DDA;

namespace Octgn.Data
{
	public class MarkerModel
	{		
		public Guid id;
		protected string name;
		private string iconUri;
		private Set set;

		public virtual string Picture
		{
			get
			{
				return set.GetPackUri() + iconUri;
			}
		}

		public string Name
		{
			get
			{ return name; }
		}

		public MarkerModel(XmlReader reader, Set set)
		{
			reader.MoveToAttribute("name");
			name = reader.Value;
			reader.MoveToAttribute("id");
			id = new Guid(reader.Value);
			reader.Read();  // <marker />
			this.set = set;
		}

		protected MarkerModel(Guid id)
		{ this.id = id; }

		private MarkerModel()
		{ }

		public override string ToString()
		{ return name; }

		internal static MarkerModel FromDataRow(Game game, IVistaDBRow row)
		{
			var result = new MarkerModel
			{
				id = (Guid)row["id"].Value,
				name = (string)row["name"].Value,
				iconUri = (string)row["icon"].Value,
				set = game.GetSet((Guid)row["setId"].Value)
			};
			return result;
		}
	}	
}
