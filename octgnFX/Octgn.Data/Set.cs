using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Xml;

namespace Octgn.Data
{
    public class Set
    {
        private List<Pack> cachedPacks;

        internal Set()
        {
        }

        public Set(string packageName, XmlReader reader, GamesRepository repository)
        {
            PackageName = packageName;
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

        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
        public Game Game { get; internal set; }
        public Version GameVersion { get; internal set; }
        public Version Version { get; internal set; }
        public string Filename { get; internal set; }
        public string PackageName { get; internal set; }

        public List<Pack> Packs
        {
            get
            {
                if (cachedPacks == null) LoadPacks();
                return cachedPacks;
            }
        }

        public override string ToString()
        {
            return Name + " " + "(" + Version + ")";
        }

        public string GetPackUri()
        {
            return "pack://file:,,," + PackageName.Replace('\\', ',');
        }

        private void LoadPacks()
        {
            cachedPacks = new List<Pack>();
            bool wasDbOpen = Game.IsDatabaseOpen;
            if (!Game.IsDatabaseOpen)
                Game.OpenDatabase(true);
            try
            {
                using (SQLiteCommand com = Game.dbc.CreateCommand())
                {
                    com.CommandText = "SELECT [xml] FROM [packs] WHERE [set_id]=@set_id ORDER BY [name]";
                    com.Parameters.AddWithValue("@set_id", Id.ToString());
                    using (SQLiteDataReader reader = com.ExecuteReader())
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