using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Xml;

namespace Octgn.Data
{
    public class Set
    {
        private List<Pack> _cachedPacks;

        internal Set()
        {
        }

        public Set(string packageName, XmlReader reader, GamesRepository repository)
        {
            PackageName = packageName;
            Name = reader.GetAttribute("name");
            string gaid = reader.GetAttribute("id");
            if (gaid != null) Id = new Guid(gaid);
            string gi = reader.GetAttribute("gameId");
            if (gi != null)
            {
                var gameId = new Guid(gi);
                Game = repository.Games.First(g => g.Id == gameId);
            }
            string gv = reader.GetAttribute("gameVersion");
            if (gv != null) GameVersion = new Version(gv);
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
                if (_cachedPacks == null) LoadPacks();
                return _cachedPacks;
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
            _cachedPacks = new List<Pack>();
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText = "SELECT [xml] FROM [packs] JOIN [sets] ON [packs].[set_real_id]=[sets].[real_id] WHERE [sets].[id]=@id ORDER BY [packs].[name]";
                com.Parameters.AddWithValue("@id", Id.ToString());
                using (SQLiteDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                        _cachedPacks.Add(new Pack(this, reader.GetString(0)));
                }
            }
        }
    }
}