using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace Octgn.Data
{
    using System.Globalization;

    public class Game
    {
        public GamesRepository Repository;
        private IList<PropertyDef> _cacheCustomProperties;

        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Filename { get; set; }
        public Version Version { get; set; }
        public int CardWidth { get; set; }
        public int CardHeight { get; set; }
        public string CardBack { get; set; }
        public string FileHash { get; set; }
        public IEnumerable<string> DeckSections { get; set; }
        public IEnumerable<string> SharedDeckSections { get; set; }
        public string FullPath
        {
            get { return Path.Combine(GamesRepository.BasePath , "Games" , Id.ToString() , "Defs" , Filename); }
        }


        public ObservableCollection<Set> Sets
        {
            get { return GetAllSets(); }
        }

        public IList<PropertyDef> CustomProperties
        {
            get { return _cacheCustomProperties ?? (_cacheCustomProperties = GetCustomProperties()); }
        }

        public IEnumerable<PropertyDef> AllProperties
        {
            get { return Enumerable.Repeat(PropertyDef.NameProperty, 1).Union(CustomProperties); }
        }

        public string DefaultDecksPath
        {
            get { return Path.Combine(GamesRepository.BasePath, "Decks"); }
        }

        public Uri GetCardBackUri()
        {
            //String s = Path.Combine(GamesRepository.BasePath, Filename);//.Replace('\\', ',');
            var u = new Uri(CardBack); //new Uri("pack://file:,,," + s + CardBack);
            return u;
        }

        public List<String> GetAllXmls()
        {
            List<String> list = Octgn.Data.DatabaseHandler.GetAllXmls(Id.ToString());
            return list;
        }

        public String GetXmlByLink(string xml_link)
        {
            string str = Octgn.Data.DatabaseHandler.SearchXmlByLink(xml_link, Id.ToString());
            return str;
        }

        public void DeleteXmlByLink(string xml_link)
        {
            Octgn.Data.DatabaseHandler.DeleteXmlByLink(xml_link, Id.ToString());
        }

        public void AddXml(string xml_link)
        {
            Octgn.Data.DatabaseHandler.AddXmlString(xml_link, Id.ToString());
        }

        public void WriteOldXmlByLink(string xml_link, string old_xml)
        {
            Octgn.Data.DatabaseHandler.WriteOldXmlByLink(xml_link, old_xml, Id.ToString());
        }

        public String GetOldXmlByLink(string xml_link)
        {
            return Octgn.Data.DatabaseHandler.GetOldXmlByLink(xml_link, Id.ToString());
        }

        public Set GetSet(Guid id)
        {
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText = "SElECT id, name, game_version, version, package FROM [sets] WHERE [id]=@id;";

                com.Parameters.AddWithValue("@id", id.ToString());
                using (SQLiteDataReader dr = com.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        var did = dr["id"] as string;
                        if (did != null)
                        {
                            var s = new Set
                                        {
                                            Id = Guid.Parse(did),
                                            Name = (string) dr["name"],
                                            Game = this,
                                            GameVersion = new Version((string) dr["game_version"]),
                                            Version = new Version((string) dr["version"]),
                                            PackageName = (string) dr["package"]
                                        };
                            return s;
                        }
                    }
                }
            }
            return null;
        }

        public Set GetSetByRealId(long id)
        {
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText = "SElECT id, name, game_version, version, package FROM [sets] WHERE [real_id]=@id;";

                com.Parameters.AddWithValue("@id", id.ToString(CultureInfo.InvariantCulture));
                using (SQLiteDataReader dr = com.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        var did = dr["id"] as string;
                        if (did != null)
                        {
                            var s = new Set
                            {
                                Id = Guid.Parse(did),
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
            return null;
        }

        public CardModel GetCardByName(string name)
        {
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText =
                    "SELECT id, name, image, alternate, dependent, (SELECT id FROM sets WHERE real_id=cards.[set_real_id]) as set_id FROM cards WHERE [name]=@name;";

                com.Parameters.AddWithValue("@name", name);
                using (SQLiteDataReader dataReader = com.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        var did = dataReader["id"] as string;
                        var setID = dataReader["set_id"] as string;
                        if (did != null && setID != null)
                        {
                            var result = new CardModel
                                             {
                                                 Id = Guid.Parse(did),
                                                 Name = (string) dataReader["name"],
                                                 ImageUri = (string) dataReader["image"],
                                                 Set = GetSet(Guid.Parse(setID)),
                                                 Alternate = Guid.Parse((string) dataReader["alternate"]),
                                                 Dependent = (string) dataReader["dependent"],
                                                 Properties = GetCardProperties(Guid.Parse(did))
                                             };
                            return result;
                        }
                    }
                }
            }
            return null;
        }

        public SortedList<string, object> GetCardProperties(Guid cardId)
        {
            var ret = new SortedList<string, object>();

            //New way
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText = "SELECT * FROM [cards] WHERE [id]=@id;";
                com.Parameters.AddWithValue("@id", cardId.ToString());
                using (SQLiteDataReader reader = com.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        foreach (PropertyDef pDef in CustomProperties)
                        {
                            ret.Add(pDef.Name, reader[string.Format("{0}{1}",Id.ToString().Replace("-",""), pDef.Name)]);
                        }
                    }
                }   
            }
            return ret;
        }

        public CardModel GetCardById(Guid id)
        {
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText =
                    "SElECT id, name, image, alternate, dependent, (SELECT id FROM sets WHERE real_id=cards.[set_real_id]) as set_id FROM [cards] WHERE [id]=@id AND [game_id]=@gid;";

                com.Parameters.AddWithValue("@id", id.ToString());
                com.Parameters.AddWithValue("@gid", this.Id.ToString());
                using (SQLiteDataReader dataReader = com.ExecuteReader())
                {
                    if (dataReader.Read())
                    {
                        var did = dataReader["id"] as string;
                        var setID = dataReader["set_id"] as string;
                        if (setID != null && did != null)
                        {
                            var result = new CardModel
                                             {
                                                 Id = Guid.Parse(did),
                                                 Name = (string) dataReader["name"],
                                                 ImageUri = (string) dataReader["image"],
                                                 Set = GetSet(Guid.Parse(setID)),
                                                 Alternate = Guid.Parse(dataReader["alternate"] as string),
                                                 Dependent = (string) dataReader["dependent"],
                                                 Properties = GetCardProperties(id)
                                             };
                            return result;
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<MarkerModel> GetAllMarkers()
        {
            var ret = new List<MarkerModel>();
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText =
                    "SElECT id, name, icon, (SELECT id FROM sets WHERE real_id=markers.[set_real_id]) as set_id FROM [markers] WHERE [game_id]=@game_id;";

                com.Parameters.AddWithValue("@game_id", Id.ToString());
                using (SQLiteDataReader dr = com.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var did = dr["id"] as string;
                        var setID = dr["set_id"] as string;
                        if (setID == null || did == null) return ret;
                        var result = new MarkerModel(
                            Guid.Parse(did),
                            (string) dr["name"],
                            (string) dr["icon"],
                            GetSet(Guid.Parse(setID))
                            );
                        ret.Add(result);
                    }
                }
            }
            return ret;
        }

        public Pack GetPackById(Guid id)
        {
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText =
                    "SElECT id, xml, set_real_id FROM [packs] WHERE [id]=@id;";

                com.Parameters.AddWithValue("@id", id.ToString());
                using (SQLiteDataReader dr = com.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        var drinfo = dr["set_real_id"];

                        var setid = drinfo is long ? (long)drinfo : -1;
                        
                        if (setid != -1)
                        {
                            Set set = GetSetByRealId(setid);
                            var xml = dr["xml"] as string;
                            return new Pack(set, xml);
                        }
                        var t = drinfo.GetType();
                    }
                }
            }
            return null;
        }

        public void InstallSet(string filename)
        {
            SQLiteTransaction trans = GamesRepository.DatabaseConnection.BeginTransaction();
            try
            {
                using (Package package = Package.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    PackageRelationship defRelationship =
                        package.GetRelationshipsByType("http://schemas.octgn.org/set/definition").First();
                    PackagePart definition = package.GetPart(defRelationship.TargetUri);

                    var settings = new XmlReaderSettings
                                       {ValidationType = ValidationType.Schema, IgnoreWhitespace = true};
                    using (
                        Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof (GamesRepository),
                                                                                             "CardSet.xsd"))
                    //CardSet.xsd determines the "attributes" of a card (name, guid, alternate, dependent)
                        if (s != null)
                            using (XmlReader reader = XmlReader.Create(s))
                                settings.Schemas.Add(null, reader);

                    // Read the cards
                    using (XmlReader reader = XmlReader.Create(definition.GetStream(), settings))
                    {
                        reader.ReadToFollowing("set"); // <?xml ... ?>

                        var set = new Set(filename, reader, Repository);
                        if (set.Game != this)
                            throw new ApplicationException(
                                string.Format("The set '{0}' is not built for the game '{1}'.", set.Name, Name));
                        if (set.GameVersion.Major != Version.Major || set.GameVersion.Minor != Version.Minor)
                            throw new ApplicationException(
                                string.Format(
                                    "The set '{0}' is incompatible with the installed game version.\nGame version: \nSet made for version: {1:2}.",
                                    set.Name, set.GameVersion));

                        InsertSet(set);

                        if (reader.IsStartElement("packaging"))
                        {
                            reader.ReadStartElement(); // <packaging>
                            while (reader.IsStartElement("pack"))
                            {
                                string xml = reader.ReadOuterXml();
                                var pack = new Pack(set, xml);
                                InsertPack(pack, xml, set.Id);
                            }
                            reader.ReadEndElement(); // </packaging>
                        }

                        if (reader.IsStartElement("markers"))
                        {
                            reader.ReadStartElement(); // <markers>
                            while (reader.IsStartElement("marker"))
                            {
                                reader.MoveToAttribute("name");
                                string markerName = reader.Value;
                                reader.MoveToAttribute("id");
                                var markerId = new Guid(reader.Value);
                                Uri markerImageUri = definition.GetRelationship("M" + markerId.ToString("N")).TargetUri;
                                string markerUri = markerImageUri.OriginalString;
                                if (!package.PartExists(markerImageUri))
                                    throw new ApplicationException(
                                        string.Format(
                                            "Image for marker '{0}', with URI '{1}' was not found in the package.",
                                            markerName, markerUri));
                                reader.Read(); // <marker />
                                InsertMarker(markerId, markerName, markerUri, set.Id);
                            }
                            reader.ReadEndElement(); // </markers>
                        }

                        if (reader.IsStartElement("cards"))
                        {
                            reader.ReadStartElement(); // <cards>
                            while (reader.IsStartElement("card"))
                                InsertCard(new CardModel(reader, this, set, definition, package));
                            // cards are parsed through the CardModel constructor, which are then inserted individually into the database
                            reader.ReadEndElement(); // </cards>
                        }

                        reader.ReadEndElement();
                    }

                    CopyDecks(package);
                    package.Close();

                    // Commit the changes
                    trans.Commit();
                }
            }
            catch (Exception e)
            {

                trans.Rollback();
                throw e;
            }
            if (SimpleDataTableCache.CacheExists())
            {
                SimpleDataTableCache.ClearCache();
            }
        }

        public void DeleteSet(Set set)
        {
            if (set == null) return;
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText = "DELETE FROM [sets] WHERE [id]=@id;";
                com.Parameters.AddWithValue("@id", set.Id.ToString());
                com.ExecuteNonQuery();
            }
            if (SimpleDataTableCache.CacheExists())
            {
                SimpleDataTableCache.ClearCache();
            }
        }

        private static void InsertSet(Set set)
        {
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                //Build Query
                var sb = new StringBuilder();
                sb.Append("INSERT OR REPLACE INTO [sets](");
                sb.Append("[id],[name],[game_real_id],[game_version],[version],[package]");
                sb.Append(") VALUES(");
                sb.Append(
                    "@id,@name,(SELECT real_id FROM games WHERE id = @game_id LIMIT 1),@game_version,@version,@package");
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
            if (SimpleDataTableCache.CacheExists())
            {
                SimpleDataTableCache.ClearCache();
            }
        }

        private static void InsertPack(Pack pack, string xml, Guid setId)
        {
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                //Build Query
                var sb = new StringBuilder();
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
            if (SimpleDataTableCache.CacheExists())
            {
                SimpleDataTableCache.ClearCache();
            }
        }

        private void InsertMarker(Guid id, string name, string iconUri, Guid setId)
        {
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                //Build Query
                var sb = new StringBuilder();
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

        private void InsertCard(CardModel card)
        {
            StringBuilder fields = new StringBuilder();
            StringBuilder values = new StringBuilder();
            fields.Append("[id],[game_id],[set_real_id],[name],[image],[alternate],[dependent]");
            values.Append("@id,@game_id,(SELECT real_id FROM sets WHERE id = @set_id LIMIT 1),@name,@image,@alternate,@dependent");

            foreach (KeyValuePair<string, object> pair in card.Properties)
            {
                
                string name = string.Format("{0}{1}", Id.ToString().Replace("-",""), pair.Key);

                fields.Append(string.Format(",[{0}]", name));
                values.Append(string.Format(",@{0}", name.Replace(" ", "")));

                PropertyType type = PropertyType.String;
                object value = "";
                if (pair.Value is string)
                {
                    type = PropertyType.String;
                    value = (string)pair.Value;
                }
                else if (pair.Value is int)
                {
                    type = PropertyType.Integer;
                    value = (int)pair.Value;
                }
                else
                {
                    type = PropertyType.String;
                    value = (string)pair.Value;
                }

                if (!DatabaseHandler.ColumnExists("cards", name, GamesRepository.DatabaseConnection))
                {
                    DatabaseHandler.AddColumn("cards", name, type, GamesRepository.DatabaseConnection);
                }
            }

            StringBuilder query = new StringBuilder();
            query.Append("INSERT INTO [cards](");
            query.Append(fields.ToString());
            query.Append(") VALUES(");
            query.Append(values.ToString());
            query.Append(");");

            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText = query.ToString();
                com.Parameters.AddWithValue("@id", card.Id.ToString());
                com.Parameters.AddWithValue("@game_id", Id.ToString());
                com.Parameters.AddWithValue("@set_id", card.Set.Id.ToString());
                com.Parameters.AddWithValue("@name", card.Name);
                com.Parameters.AddWithValue("@image", card.ImageUri);
                com.Parameters.AddWithValue("@alternate", card.Alternate.ToString());
                com.Parameters.AddWithValue("@dependent", card.Dependent.ToString());
                foreach (KeyValuePair<string, object> pair in card.Properties)
                {
                    string name = string.Format("{0}{1}", Id.ToString().Replace("-",""), pair.Key);
                    string key = string.Format("@{0}", name.Replace(" ", ""));
                    com.Parameters.AddWithValue(key, pair.Value);
                }
#if(DEBUG)
                Debug.WriteLine(com.CommandText);
                foreach (SQLiteParameter p in com.Parameters)
                {
                    Debug.Write("ParameterName: " + p.ParameterName + "\r\n Value: " + p.Value + "\r\n");
                }
#endif
                com.ExecuteNonQuery();
            }
        }

        private ObservableCollection<Set> GetAllSets()
        {
            var result = new ObservableCollection<Set>();
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText =
                    "SElECT * FROM [sets] WHERE [game_real_id]=(SELECT real_id FROM games WHERE id = @game_id LIMIT 1);";

                com.Parameters.AddWithValue("@game_id", Id.ToString());
                using (SQLiteDataReader dr = com.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var guid = dr["id"] as string;
                        if (guid == null) continue;
                        var s = new Set
                                    {
                                        Id = Guid.Parse(guid),
                                        Name = (string) dr["name"],
                                        Game = this,
                                        GameVersion = new Version((string) dr["game_version"]),
                                        Version = new Version((string) dr["version"]),
                                        PackageName = (string) dr["package"]
                                    };
                        result.Add(s);
                    }
                }
            }

            return result;
        }

        private List<PropertyDef> GetCustomProperties()
        {
            var ret = new List<PropertyDef>();
            using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
            {
                com.CommandText = "SElECT DISTINCT name, type FROM [custom_properties] WHERE [game_id]=@game_id;";
                com.Parameters.AddWithValue("@game_id", Id.ToString());
                using (SQLiteDataReader dr = com.ExecuteReader())
                {
                    var dl = new Dictionary<string, PropertyType>();
                    while (dr.Read())
                    {
                        var name = dr["name"] as string;//name of property
                        var t = (int) ((long) dr["type"]);//type of property (String, Int, Char)
                        PropertyType pt;
                        switch (t)
                        {
                            case 0:
                                pt = PropertyType.String;
                                break;
                            case 1:
                                pt = PropertyType.Integer;
                                break;
                            default:
                                pt = PropertyType.Char;
                                break;
                        }
                        if (name != null)
                            if (!dl.ContainsKey(name))
                                dl.Add(name, pt);
                    }
                    ret.AddRange(dl.Select(d => new PropertyDef(d.Key, d.Value)));
                }
            }

            return ret;
        }

        internal void CopyDecks()
        {
            using (Package package = Package.Open(FullPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                CopyDecks(package);
            }
        }

        internal void CopyDecks(Package package)
        {
            string path = Path.Combine(GamesRepository.BasePath, "Decks");
            PackageRelationshipCollection decks = package.GetRelationshipsByType("http://schemas.octgn.org/set/deck");
            var buffer = new byte[0x1000];
            foreach (PackageRelationship deckRel in decks)
            {
                PackagePart deck = package.GetPart(deckRel.TargetUri);
                string deckuri = Path.GetFileName(deck.Uri.ToString());
                if (deckuri == null) continue;
                string deckFilename = Path.Combine(path, deckuri);
                using (Stream deckStream = deck.GetStream(FileMode.Open, FileAccess.Read))
                using (
                    FileStream targetStream = File.Open(deckFilename, FileMode.Create, FileAccess.Write, FileShare.Read)
                    )
                {
                    while (true)
                    {
                        int read = deckStream.Read(buffer, 0, buffer.Length);
                        if (read == 0) break;
                        targetStream.Write(buffer, 0, read);
                    }
                }
            }
        }

        // Precompile Regex.
        private static readonly Regex GUID_REGEX = new Regex("^([0-9a-fA-F]){8}(-([0-9a-fA-F]){4}){3}-([0-9a-fA-F]){12}$");

        // Empty return type for extractGuids method, so a
        // new IList<Guid> doesn't have to be created each time.
        private static readonly IList<Guid> GUID_EMPTY = (new List<Guid>()).AsReadOnly();

        //
        // Summary:
        //     Extracts all of the GUIDs found in a provided string. Illegal GUIDs
        //     are ignored.
        //
        // Parameters:
        //   text:
        //     The string possibly containing GUIDs.
        //
        // Returns:
        //     The (readonly) list of found GUIDs or an empty
        //     (readonly) list if none are found.
        private static IList<Guid> ExtractGuids(string text)
        {                
            if (string.IsNullOrEmpty(text)) return GUID_EMPTY; // short circuit
            if (text.Length < 32) return GUID_EMPTY;  // short circuit

            MatchCollection mc = GUID_REGEX.Matches(text);
            int c = mc.Count;
            if (c < 1) return GUID_EMPTY;  // short circuit
            List<Guid> list = new List<Guid>(c);
            foreach (Match m in mc)
            {
                Guid g;
                if (Guid.TryParse(m.Value, out g))
                {
                    list.Add(g);
                }
#if(DEBUG)
                else { Debug.Write("GUID passed regex but was unable to parse: " + m.Value); }
#endif
            }

            if (list.Count == 0) return GUID_EMPTY;  // short circuit

            // Since our singleton 'short circuit' list is readonly, we should
            // make our return value readonly as a consistency measure.
            return list.AsReadOnly();
        }

        private static string CleanupSql(StringBuilder sql)
        {
            // Cleanup whitespace in SQL code.
            sql = sql.Replace('\t', ' ');
            sql = sql.Replace("\r\n", " ");
            sql = sql.Replace('\r', ' ');

            while (true)
            {
                int len = sql.Length;
                sql = sql.Replace("  ", " ");

                // If we didn't replace anything then quit loop
                if (sql.Length == len) break;
            }

            String s = sql.ToString();
            s = s.Trim();

            return s;
        }

        public DataTable SelectCards(string[] conditions)
        {
            // Make null-safe
            if (conditions == null) conditions = new String[0];

            // This provides multiple 'and' conditions but only one 'or' condition which
            // should be OK for now, but will not accomodate multiple 'or' conditions
            // without a re-write.
            IList<String> consAnd = new List<String>();
            IList<String> consOr = new List<String>();
            foreach (string c in conditions)
            {
                if (c.Contains("set_id")) consOr.Add(c);
                else consAnd.Add(c);
            }
            
            StringBuilder sb;
            var cards = new DataTable();
            if (SimpleDataTableCache.GetInstance().IsCached(conditions))
            {
                cards = SimpleDataTableCache.GetInstance().GetCache(conditions);
            }
            else
            {
                var customProperties = new DataTable();
                IList<Guid> sets = GUID_EMPTY;
                if (conditions != null)
                {
                    sets = conditions
                        .Where(c => c.Contains("set_id"))
                        .SelectMany(c => ExtractGuids(c))
                        .ToList();                            
                }


                // We need 2 different queries here because it is possible for...
                //   [cards].[game_id] <> [games].[id]
                // ...or it is possible that there are cards not attached to a set
                // in which case we need...
                //   LEFT OUTER JOIN [sets]
                // ...instead of...
                //   INNER JOIN [sets]
                // ...which is possible in theory because the columns [sets].[game_real_id]
                // and [cards].[set_real_id] are both NULLABLE. This SQL implementation should
                // produce the same query results as before.
                sb = new StringBuilder();
                if (sets.Count > 0) 
                {
                    sb.Append(" SELECT *, [sets].[id] AS [set_id] ");
                    sb.Append(" FROM      [cards] ");
                    sb.Append(" INNER JOIN	[sets]	ON	[sets].[real_id]  = [cards].[set_real_id] ");
                    sb.Append(" INNER JOIN	[games]	ON	[games].[real_id] = [sets].[game_real_id] ");
                    sb.Append(" WHERE [games].[id] = @game_id ");
                    sb.Append(" AND ( ");
                    for(int i =0; i < sets.Count; i++)
                    {
                        if (i > 0) sb.Append(" OR ");
                        sb.Append(" [sets].[set_id] = @set_id" + i);
                    }
                    sb.Append(" ) ");
                }
                else
                {
                    sb.Append(" SELECT [cards].*, [sets].[id] AS [set_id] ");
                    sb.Append(" FROM 			 [cards] ");
                    sb.Append(" LEFT OUTER JOIN [sets]	ON	[sets].[real_id] = [cards].[set_real_id] ");
                    sb.Append(" WHERE [cards].[game_id] = @game_id ");
                }
                    
                String sql = CleanupSql(sb);

#if(DEBUG)
                Debug.Write(sql);
#endif

                using (SQLiteCommand com = GamesRepository.DatabaseConnection.CreateCommand())
                {
                    // Cards query
                    com.CommandText = sql;
                    com.Parameters.AddWithValue("@game_id", Id.ToString());
                    for (int i = 0; i < sets.Count; i++)
                    {
                        com.Parameters.AddWithValue("@set_id" + i, sets[i]);
                    }
                    cards.Load(com.ExecuteReader());
                    
                    // Custom Properties query
                    com.CommandText = "SELECT * FROM [custom_properties] WHERE [game_id] = @game_id";
                    com.Parameters.AddWithValue("@game_id", Id.ToString());
                    customProperties.Load(com.ExecuteReader());
                }

                // Update our cache
                SimpleDataTableCache.GetInstance().AddToCache(conditions, cards);
            }

            // Appears to remove the game ID guid from the column names
            string gameIdWithoutDashes = Id.ToString().Replace("-", "");
            for (int i = 0; i < cards.Columns.Count; i++)
            {
                cards.Columns[i].ColumnName = cards.Columns[i].ColumnName.Replace(gameIdWithoutDashes, "");
            }

            //Now apply the search query to it
            sb = new StringBuilder();
            for (int i = 0; i < consAnd.Count; i++)
            {
                if (i > 0) sb.Append(" AND ");
                sb.Append("(" + consAnd[i] + ")");
            }

            if (consOr.Count > 0)
            {
                // Do we already have some WHERE clause stuff, if so
                // we append the 'AND' on to the end of it.
                if (consAnd.Count > 0) sb.Append(" AND ");

                sb.Append("(");
                for (int i = 0; i < consOr.Count; i++)
                {
                    if (i > 0) sb.Append(" OR ");
                    sb.Append("(" + consOr[i] + ")");
                }
                sb.Append(")");
            }

            String where = CleanupSql(sb);

            // Did we actually create a 'WHERE' condition? If so,
            // apply it to our data table.
            if (!String.IsNullOrEmpty(where))
            {
                cards.CaseSensitive = false;
                DataTable dtnw = cards.Clone();
                dtnw.Rows.Clear();

                DataRow[] rows = cards.Select(where);

                foreach (DataRow r in rows) dtnw.ImportRow(r);

                // This isn't needed since the cards should be GC'd
                //cards.Rows.Clear();
                cards = dtnw;
            }

            return cards;
        }

        public IEnumerable<CardModel> SelectCardModels(params string[] conditions)
        {
            return from DataRow row in SelectCards(conditions).Rows
                   select CardModel.FromDataRow(this, row);
        }

        public IEnumerable<CardModel> SelectRandomCardModels(int count, params string[] conditions)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            if (count == 0) return Enumerable.Empty<CardModel>();

            DataTable table = SelectCards(conditions);
            IEnumerable<DataRow> candidates;
            if (table.Rows.Count <= count)
                candidates = table.Rows.Cast<DataRow>();
            else
            {
                var rnd = new Random();
                IEnumerable<int> indexes = from i in Enumerable.Range(0, table.Rows.Count)
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