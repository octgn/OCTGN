using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using VistaDB.DDA;

namespace Octgn.Data
{
	public class Set
	{
		public Guid Id { get; internal set; }
		public string Name { get; internal set; }
		public Game Game { get; internal set; }
		public Version GameVersion { get; internal set; }
        public Version Version { get; internal set; }
        public string Filename { get; internal set; }
        public string PackageName { get; internal set; }
    private List<Pack> cachedPacks;

    public List<Pack> Packs
    {
      get
      {
        if (cachedPacks == null) LoadPacks();
        return cachedPacks;
      }
    }

		internal Set()
		{ }

		public Set(string packageName, XmlReader reader, GamesRepository repository)
		{
			this.PackageName = packageName;
			Name = reader.GetAttribute("name");
			Id = new Guid(reader.GetAttribute("id"));
			var gameId = new Guid(reader.GetAttribute("gameId"));
			Game = repository.Games.First(g => g.Id == gameId);
			GameVersion = new Version(reader.GetAttribute("gameVersion"));
      Version ver;
      Version.TryParse(reader.GetAttribute("version"), out ver);
      Version = ver ?? new Version(0, 0);
			reader.ReadStartElement("set");
		}

		public string GetPackUri()
		{
			return "pack://file:,,," + PackageName.Replace('\\', ',');
		}

		internal static Set FromDataRow(Game game, IVistaDBRow row)
		{
			return new Set
			{
				Id = (Guid)row["id"].Value,
				Name = (string)row["name"].Value,
				Game = game,
				GameVersion = new Version((string)row["gameVersion"].Value),
        Version = new Version((string)row["version"].Value),
				PackageName = (string)row["package"].Value
			};
		}

    private void LoadPacks()
    {
      cachedPacks = new List<Pack>();
      bool wasDbOpen = Game.IsDatabaseOpen;
      if (!wasDbOpen)
        Game.OpenDatabase(true);
      try
      {
        using (var conn = new VistaDB.Provider.VistaDBConnection(Game.db))
        {
          var cmd = conn.CreateCommand();
          cmd.CommandText = "SELECT [xml] FROM Pack WHERE setId = @setId ORDER BY name";
          cmd.Parameters.Add("setId", Id);
          using (var reader = cmd.ExecuteReader())
          {
            while (reader.Read())
              cachedPacks.Add(new Pack(this, reader.GetString(0)));
          }
        }
      }
      finally
      {
        if (!wasDbOpen)
          Game.CloseDatabase();
      }
    }
	}
}
