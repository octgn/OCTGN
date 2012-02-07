using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data.SQLite;
using System.Data;

namespace Octgn.Data
{
    public class Game
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Filename { get; set; }
        public Version Version { get; set; }
        public int CardWidth { get; set; }
        public int CardHeight { get; set; }
        public string CardBack { get; set; }
        public Boolean Warning { get; set; }
        public string FileHash { get; set; }
        public IEnumerable<string> DeckSections { get; set; }
        public IEnumerable<string> SharedDeckSections { get; set; }

        internal string basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn");
        public GamesRepository repository;

        public SQLiteConnection dbc;

        public Uri GetCardBackUri()
        {
            String s = Path.Combine(basePath, Filename).Replace('\\', ',');
            Uri u = new Uri("pack://file:,,," + s + CardBack);
            return u;
        }

        public ObservableCollection<Set> Sets
        {
            get
            {
                return GetAllSets();
            }
        }

        public IList<PropertyDef> CustomProperties
        {
            get
            {
                if (cacheCustomProperties == null)
                    cacheCustomProperties = GetCustomProperties();
                return cacheCustomProperties;
            }
        }
        private IList<PropertyDef> cacheCustomProperties;
        public IEnumerable<PropertyDef> AllProperties
        {
            get
            {
                return Enumerable.Union(Enumerable.Repeat(PropertyDef.NameProperty, 1), CustomProperties);
            }
        }

        public string DefaultDecksPath
        { get { return Path.Combine(basePath, "Decks"); } }

        public bool IsDatabaseOpen
        { get; private set; }

        public void OpenDatabase(bool readOnly)
        {
            if (!IsDatabaseOpen)
            {
                try
                {
                    string conString = "URI=file:" + Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn", "Database", "master.db3");
                    dbc = new SQLiteConnection(conString);
                    dbc.Open();
                    using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                    {
                        com.CommandText = "PRAGMA locking_mode=EXCLUSIVE; PRAGMA automatic_index=FALSE; PRAGMA synchronous=OFF; PRAGMA auto_vacuum=INCREMENTAL; PRAGMA foreign_keys=ON; PRAGMA encoding='UTF-8';";
                        com.ExecuteScalar();
                    }
                    IsDatabaseOpen = true;
                }
                catch
                {
                    throw;
                }
            }

        }

        public void CloseDatabase()
        {
            if (IsDatabaseOpen)
            {
                dbc.Close();
                dbc.Dispose();
                IsDatabaseOpen = false;
            }
        }

        public Set GetSet(Guid id)
        {
            if (!IsDatabaseOpen)
                OpenDatabase(false);
            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {
                    com.CommandText = "SElECT id, name, game_version, version, package FROM [sets] WHERE [id]=@id;";

                    com.Parameters.AddWithValue("@id", id.ToString());
                    using (SQLiteDataReader dr = com.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            Set s = new Set
                            {
                                Id = Guid.Parse(dr["id"] as string),
                                Name = (string)dr["name"],
                                Game = this,
                                GameVersion = new Version((string)dr["game_version"]),
                                Version = new Version((string)dr["version"]),
                                PackageName = (string)dr["package"]
                            };
                            return s;
                        }
                    }
                }
            }
            catch { throw; }
            return null;
        }

        public CardModel GetCardByName(string name)
        {
            if (!IsDatabaseOpen)
                OpenDatabase(false);

            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {
                    com.CommandText = "SElECT id, name, image, (SELECT id FROM sets WHERE real_id=cards.[set_real_id]) as set_id WHERE [name]=@name;";

                    com.Parameters.AddWithValue("@name", name);
                    using (SQLiteDataReader dr = com.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            var result = new CardModel
                            {
                                Id = Guid.Parse(dr["id"] as string),
                                Name = (string)dr["name"],
                                ImageUri = (string)dr["image"],
                                set = GetSet(Guid.Parse(dr["set_id"] as string)),
                                Properties = GetCardProperties(Guid.Parse(dr["id"] as string))
                            };
                            return result;
                        }
                    }
                }
            }
            catch { throw; }
            return null;
        }
        public SortedList<string, object> GetCardProperties(Guid cardId)
        {
            if (!IsDatabaseOpen)
                OpenDatabase(false);
            SortedList<string, object> ret = new SortedList<string, object>();
            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {
                    com.CommandText = "SElECT type, name, vstr, vint FROM [custom_properties] WHERE [card_real_id]=(SELECT real_id FROM cards WHERE id = @card_id LIMIT 1);";

                    com.Parameters.AddWithValue("@card_id", cardId.ToString());
                    using (SQLiteDataReader dr = com.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int vt = (int)((long)dr["type"]);
                            switch (vt)
                            {
                                case 0: // String
                                    {
                                        string name = dr["name"] as string;
                                        string val = dr["vstr"] as string;
                                        ret.Add(name, val);
                                        break;
                                    }
                                case 1: // int
                                    {
                                        string name = dr["name"] as string;
                                        int val = (int)((long)dr["vint"]);
                                        ret.Add(name, val);
                                        break;
                                    }
                                case 2: //char
                                    {
                                        string name = dr["name"] as string;
                                        string val = dr["vstr"] as string;
                                        ret.Add(name, val);
                                        break;
                                    }
                            }
                        }
                    }
                }
            }
            catch { throw; }
            return ret;
        }

        public CardModel GetCardById(Guid id)
        {
            if (!IsDatabaseOpen)
                OpenDatabase(false);

            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {
                    com.CommandText = "SElECT id, name, image, (SELECT id FROM sets WHERE real_id=cards.[set_real_id]) as set_id FROM [cards] WHERE [id]=@id;";

                    com.Parameters.AddWithValue("@id", id.ToString());
                    using (SQLiteDataReader dr = com.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            var result = new CardModel
                            {
                                Id = Guid.Parse(dr["id"] as string),
                                Name = (string)dr["name"],
                                ImageUri = (string)dr["image"],
                                set = GetSet(Guid.Parse(dr["set_id"] as string)),
                                Properties = GetCardProperties(id)
                            };
                            return result;
                        }
                    }
                }
            }
            catch { throw; }
            return null;
        }

        public IEnumerable<MarkerModel> GetAllMarkers()
        {
            if (!IsDatabaseOpen)
                OpenDatabase(false);
            List<MarkerModel> ret = new List<MarkerModel>();
            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {
                    com.CommandText = "SElECT id, name, icon, (SELECT id FROM sets WHERE real_id=markers.[set_real_id]) as set_id FROM [markers] WHERE [game_id]=@game_id;";

                    com.Parameters.AddWithValue("@game_id", this.Id.ToString());
                    using (SQLiteDataReader dr = com.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var result = new MarkerModel(
                                Guid.Parse(dr["id"] as string),
                                (string)dr["name"],
                                (string)dr["icon"],
                                GetSet(Guid.Parse(dr["set_id"] as string))
                            );
                            ret.Add(result);
                        }
                    }
                }
            }
            catch { throw; }
            return ret;
        }

        public Pack GetPackById(Guid id)
        {
            if (!IsDatabaseOpen)
                OpenDatabase(false);

            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {
                    com.CommandText = "SElECT xml, (SELECT id FROM sets WHERE real_id=packs.[set_real_id]) as set_id FROM [packs] WHERE [id]=@id;";

                    com.Parameters.AddWithValue("@id", id.ToString());
                    using (SQLiteDataReader dr = com.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            var set = GetSet(Guid.Parse(dr["set_id"] as string));
                            string xml = dr["xml"] as string;
                            return new Pack(set, xml);
                        }
                    }
                }
            }
            catch { throw; }
            return null;
        }

        public void InstallSet(string filename)
        {
            OpenDatabase(false);
            SQLiteTransaction trans = dbc.BeginTransaction();
            try
            {

                using (var package = Package.Open(filename, FileMode.Open, FileAccess.Read))
                {
                    var defRelationship = package.GetRelationshipsByType("http://schemas.octgn.org/set/definition").First();
                    var definition = package.GetPart(defRelationship.TargetUri);

                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.ValidationType = ValidationType.Schema;
                    settings.IgnoreWhitespace = true;
                    using (Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(GamesRepository), "CardSet.xsd"))
                    using (XmlReader reader = XmlReader.Create(s))
                        settings.Schemas.Add(null, reader);

                    Set set;

                    // Read the cards
                    using (var reader = XmlReader.Create(definition.GetStream(), settings))
                    {
                        reader.ReadToFollowing("set");  // <?xml ... ?>

                        set = new Set(filename, reader, repository);
                        if (set.Game != this)
                            throw new ApplicationException(string.Format("The set '{0}' is not built for the game '{1}'.", set.Name, Name));
                        if (set.GameVersion.Major != Version.Major || set.GameVersion.Minor != Version.Minor)
                            throw new ApplicationException(string.Format("The set '{0}' is incompatible with the installed game version.\nGame version: {1:2}\nSet made for version: {2:2}.", set.Name, Version, set.GameVersion));

                        InsertSet(set);

                        if (reader.IsStartElement("packaging"))
                        {
                            reader.ReadStartElement();  // <packaging>
                            while (reader.IsStartElement("pack"))
                            {
                                string xml = reader.ReadOuterXml();
                                var pack = new Pack(set, xml);
                                InsertPack(pack, xml, set.Id);
                            }
                            reader.ReadEndElement();  // </packaging>
                        }

                        if (reader.IsStartElement("markers"))
                        {
                            reader.ReadStartElement();  // <markers>
                            while (reader.IsStartElement("marker"))
                            {
                                reader.MoveToAttribute("name");
                                var markerName = reader.Value;
                                reader.MoveToAttribute("id");
                                var markerId = new Guid(reader.Value);
                                var markerImageUri = definition.GetRelationship("M" + markerId.ToString("N")).TargetUri;
                                var markerUri = markerImageUri.OriginalString;
                                if (!package.PartExists(markerImageUri))
                                    throw new ApplicationException(string.Format("Image for marker '{0}', with URI '{1}' was not found in the package.", markerName, markerUri));
                                reader.Read();  // <marker />
                                InsertMarker(markerId, markerName, markerUri, set.Id);
                            }
                            reader.ReadEndElement();    // </markers>
                        }

                        if (reader.IsStartElement("cards"))
                        {
                            reader.ReadStartElement();  // <cards>
                            while (reader.IsStartElement("card"))
                                InsertCard(new CardModel(reader, this, set, definition, package));
                            reader.ReadEndElement();    // </cards>
                        }

                        reader.ReadEndElement();
                    }

                    CopyDecks(package);
                    package.Close();

                    // Commit the changes
                    trans.Commit();

                }
            }
            catch
            {
                trans.Rollback();
                throw;
            }
            finally
            { CloseDatabase(); }
        }

        public void DeleteSet(Set set)
        {
            bool wasdbopen = IsDatabaseOpen;
            if (!IsDatabaseOpen)
                OpenDatabase(false);
            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {
                    com.CommandText = "DELETE FROM [sets] WHERE [id]=@id;";
                    com.Parameters.AddWithValue("@id", set.Id.ToString());
                    com.ExecuteNonQuery();
                }
            }
            catch { throw; }
            if (!wasdbopen)
                CloseDatabase();
        }

        private void InsertSet(Set set)
        {
            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {

                    //Build Query
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT OR REPLACE INTO [sets](");
                    sb.Append("[id],[name],[game_real_id],[game_version],[version],[package]");
                    sb.Append(") VALUES(");
                    sb.Append("@id,@name,(SELECT real_id FROM games WHERE id = @game_id LIMIT 1),@game_version,@version,@package");
                    sb.Append(");\n");
                    com.CommandText = sb.ToString();

                    com.Parameters.AddWithValue("@id", set.Id.ToString());
                    com.Parameters.AddWithValue("@name", set.Name);
                    com.Parameters.AddWithValue("@game_id", set.Game.Id.ToString());
                    com.Parameters.AddWithValue("@game_version", set.GameVersion.ToString());
                    com.Parameters.AddWithValue("@version", set.Version.ToString());
                    com.Parameters.AddWithValue("@package", set.PackageName);
                    com.ExecuteNonQuery();
                }
            }
            catch { throw; }
        }

        private void InsertPack(Pack pack, string xml, Guid setId)
        {
            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {

                    //Build Query
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT OR REPLACE INTO [packs](");
                    sb.Append("[id],[set_real_id],[name],[xml]");
                    sb.Append(") VALUES(");
                    sb.Append("@id,(SELECT real_id FROM sets WHERE id = @set_id LIMIT 1),@name,@xml");
                    sb.Append(");\n");
                    com.CommandText = sb.ToString();

                    com.Parameters.AddWithValue("@id", pack.Id.ToString());
                    com.Parameters.AddWithValue("@set_id", setId.ToString());
                    com.Parameters.AddWithValue("@name", pack.Name);
                    com.Parameters.AddWithValue("@xml", xml);
                    com.ExecuteNonQuery();
                }
            }
            catch { throw; }
        }

        private void InsertMarker(Guid id, string name, string iconUri, Guid setId)
        {
            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {

                    //Build Query
                    StringBuilder sb = new StringBuilder();
                    sb.Append("INSERT OR REPLACE INTO [markers](");
                    sb.Append("[id],[set_real_id],[game_id],[name],[icon]");
                    sb.Append(") VALUES(");
                    sb.Append("@id,(SELECT real_id FROM sets WHERE id = @set_id LIMIT 1),@game_id,@name,@icon");
                    sb.Append(");\n");
                    com.CommandText = sb.ToString();

                    com.Parameters.AddWithValue("@id", id.ToString());
                    com.Parameters.AddWithValue("@set_id", setId.ToString());
                    com.Parameters.AddWithValue("@game_id", Id.ToString());
                    com.Parameters.AddWithValue("@name", name);
                    com.Parameters.AddWithValue("@icon", iconUri);
                    com.ExecuteNonQuery();
                }
            }
            catch { throw; }
        }

        private void InsertCard(CardModel card)
        {
            bool wasdbopen = IsDatabaseOpen;
            if (!IsDatabaseOpen)
                OpenDatabase(false);
            try
            {
                StringBuilder sb = new StringBuilder();
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {

                    //Build Query
                    sb.Append("INSERT INTO [cards](");
                    sb.Append("[id],[game_id],[set_real_id],[name], [image]");
                    sb.Append(") VALUES(");
                    sb.Append("@id,@game_id,(SELECT real_id FROM sets WHERE id = @set_id LIMIT 1),@name,@image");
                    sb.Append(");\n");
                    com.CommandText = sb.ToString();

                    com.Parameters.AddWithValue("@id", card.Id.ToString());
                    com.Parameters.AddWithValue("@game_id", Id.ToString());
                    com.Parameters.AddWithValue("@set_id", card.set.Id.ToString());
                    com.Parameters.AddWithValue("@name", card.Name);
                    com.Parameters.AddWithValue("@image", card.ImageUri);
                    com.ExecuteNonQuery();
                }
                //Add custom properties for the card.
                sb = new StringBuilder();
                sb.Append("INSERT INTO [custom_properties](");
                sb.Append("[id],[card_real_id],[game_id],[name],[type],[vint],[vstr]");
                sb.Append(") VALUES(");
                sb.Append("@id,(SELECT real_id FROM cards WHERE id = @card_id LIMIT 1),@game_id,@name,@type,@vint,@vstr");
                sb.Append(");\n");
                string command = sb.ToString();
                foreach (KeyValuePair<string, object> pair in card.Properties)
                {
                    using (SQLiteCommand com = dbc.CreateCommand())
                    {
                        com.CommandText = command;
                        com.Parameters.AddWithValue("@id", pair.Key + card.Id.ToString());
                        com.Parameters.AddWithValue("@card_id", card.Id.ToString());
                        com.Parameters.AddWithValue("@game_id", Id.ToString());
                        com.Parameters.AddWithValue("@name", pair.Key);
                        if (pair.Value is string)
                        {
                            com.Parameters.AddWithValue("@type", 0);
                            com.Parameters.AddWithValue("@vstr", pair.Value);
                            com.Parameters.AddWithValue("@vint", null);
                        }
                        else if (pair.Value is int)
                        {
                            com.Parameters.AddWithValue("@type", 1);
                            com.Parameters.AddWithValue("@vstr", null);
                            com.Parameters.AddWithValue("@vint", (int)pair.Value);
                        }
                        else		// char
                        {
                            com.Parameters.AddWithValue("@type", 2);
                            com.Parameters.AddWithValue("@vstr", pair.Value.ToString());
                            com.Parameters.AddWithValue("@vint", null);
                        }
                        com.ExecuteNonQuery();
                    }
                }
            }
            catch { throw; }
            if (!wasdbopen)
                CloseDatabase();
        }

        private ObservableCollection<Set> GetAllSets()
        {
            var result = new ObservableCollection<Set>();
            bool wasdbopen = IsDatabaseOpen;
            if (!IsDatabaseOpen)
                OpenDatabase(true);

            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {
                    com.CommandText = "SElECT * FROM [sets] WHERE [game_real_id]=(SELECT real_id FROM games WHERE id = @game_id LIMIT 1);";

                    com.Parameters.AddWithValue("@game_id", Id.ToString());
                    using (SQLiteDataReader dr = com.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            Set s = new Set
                            {
                                Id = Guid.Parse(dr["id"] as string),
                                Name = (string)dr["name"],
                                Game = this,
                                GameVersion = new Version((string)dr["game_version"]),
                                Version = new Version((string)dr["version"]),
                                PackageName = (string)dr["package"]
                            };
                            result.Add(s);
                        }
                    }
                }
            }
            catch { throw; }

            if (!wasdbopen)
                CloseDatabase();
            return result;
        }

        private List<PropertyDef> GetCustomProperties()
        {
            bool wasdbopen = IsDatabaseOpen;
            if (!IsDatabaseOpen)
                OpenDatabase(false);
            List<PropertyDef> ret = new List<PropertyDef>();
            try
            {
                using (System.Data.SQLite.SQLiteCommand com = dbc.CreateCommand())
                {
                    com.CommandText = "SElECT DISTINCT name, type FROM [custom_properties] WHERE [game_id]=@game_id;";
                    //com.CommandText = "SElECT * FROM [custom_properties] WHERE [game_id]=@game_id AND [card_id]='';";

                    com.Parameters.AddWithValue("@game_id", Id.ToString());
                    using (SQLiteDataReader dr = com.ExecuteReader())
                    {
                        Dictionary<string, PropertyType> dl = new Dictionary<string, PropertyType>();
                        while (dr.Read())
                        {
                            string name = dr["name"] as string;
                            int t = (int)((long)dr["type"]);
                            PropertyType pt;
                            if (t == 0)
                                pt = PropertyType.String;
                            else if (t == 1)
                                pt = PropertyType.Integer;
                            else
                                pt = PropertyType.Char;
                            if (!dl.ContainsKey(name))
                                dl.Add(name, pt);
                        }
                        foreach (KeyValuePair<string, PropertyType> d in dl)
                        {
                            ret.Add(new PropertyDef(d.Key, d.Value));
                        }
                    }
                }
            }
            catch { throw; }

            if (!wasdbopen)
                CloseDatabase();
            return ret;
        }

        internal void CopyDecks(string filename)
        {
            using (Package package = Package.Open(filename, FileMode.Open, FileAccess.Read))
                CopyDecks(package);
        }

        internal void CopyDecks(Package package)
        {
            string path = Path.Combine(basePath, "Decks");
            var decks = package.GetRelationshipsByType("http://schemas.octgn.org/set/deck");
            byte[] buffer = new byte[0x1000];
            foreach (var deckRel in decks)
            {
                var deck = package.GetPart(deckRel.TargetUri);
                string deckFilename = Path.Combine(path, Path.GetFileName(deck.Uri.ToString()));
                using (var deckStream = deck.GetStream(FileMode.Open, FileAccess.Read))
                using (var targetStream = File.Open(deckFilename, FileMode.Create, FileAccess.Write))
                {
                    int read;
                    while (true)
                    {
                        read = deckStream.Read(buffer, 0, buffer.Length);
                        if (read == 0) break;
                        targetStream.Write(buffer, 0, read);
                    }
                }
            }
        }

        public System.Data.DataTable SelectCards(string[] conditions)
        {
            bool wasdbopen = IsDatabaseOpen;
            if (!IsDatabaseOpen)
                OpenDatabase(false);
            System.Data.DataTable ret = new System.Data.DataTable();
            try
            {
                using (SQLiteCommand com = dbc.CreateCommand())
                {
                    com.CommandText = "SELECT *, (SELECT id FROM sets WHERE real_id=cards.[set_real_id]) as set_id FROM cards WHERE game_id=@game_id;";
                    com.Parameters.AddWithValue("@game_id", Id.ToString());
                    ret.Load(com.ExecuteReader());
                }
            }
            catch
            {
                throw;
            }
            foreach (PropertyDef d in CustomProperties)
            {
                ret.Columns.Add(d.Name);
            }
            int i = 0;
            foreach (DataRow r in ret.Rows)
            {
                string cid = r["id"] as string;
                using (SQLiteCommand com = dbc.CreateCommand())
                {
                    com.CommandText = "SELECT * FROM [custom_properties] WHERE [card_real_id]=(SELECT real_id FROM cards WHERE id = @cid LIMIT 1)";
                    com.Parameters.AddWithValue("@cid", cid);
                    using (SQLiteDataReader dr = com.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            string cname = dr["name"] as string;
                            if (!ret.Columns.Contains(cname))
                                continue;
                            int t = (int)((long)dr["type"]);
                            if (t == 0)
                                ret.Rows[i][cname] = dr["vstr"] as string;
                            else if (t == 1)
                                ret.Rows[i][cname] = (int)((long)dr["vint"]);
                            else
                                ret.Rows[i][cname] = dr["vstr"] as string;
                        }
                    }
                }
                i++;
            }
            //Now apply the search query to it
            var sb = new StringBuilder();
            if (conditions != null && conditions.Length > 0)
            {
                string connector = "";
                foreach (string condition in conditions)
                {
                    sb.Append(connector);
                    sb.Append("(");
                    sb.Append(condition);
                    sb.Append(")");
                    connector = " AND ";
                }
                ret.CaseSensitive = false;

                DataTable dtnw = ret.Clone();
                dtnw.Rows.Clear();

                DataRow[] rows = (DataRow[])ret.Select(sb.ToString());

                foreach (DataRow r in rows)
                    dtnw.ImportRow(r);

                ret.Rows.Clear();
                ret = dtnw;
            }
            if (!wasdbopen)
                CloseDatabase();
            return ret;
        }

        public IEnumerable<CardModel> SelectCardModels(params string[] conditions)
        {
            return from System.Data.DataRow row in SelectCards(conditions).Rows
                   select CardModel.FromDataRow(this, row);
        }

        public IEnumerable<CardModel> SelectRandomCardModels(int count, params string[] conditions)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("count parameter must be greater or equal to 0.");
            if (count == 0) return Enumerable.Empty<CardModel>();

            var table = SelectCards(conditions);
            IEnumerable<System.Data.DataRow> candidates;
            if (table.Rows.Count <= count)
                candidates = table.Rows.Cast<System.Data.DataRow>();
            else
            {
                var rnd = new System.Random();
                var indexes = from i in Enumerable.Range(0, table.Rows.Count)
                              let pair = new KeyValuePair<int, double>(i, rnd.NextDouble())
                              orderby pair.Value
                              select pair.Key;
                candidates = from i in indexes.Take(count)
                             select table.Rows[i];
            }
            return candidates.Select(r => CardModel.FromDataRow(this, r));
        }

    }
}