using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Xml;
using VistaDB;
using VistaDB.DDA;

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

        public IEnumerable<string> DeckSections { get; set; }

        public IEnumerable<string> SharedDeckSections { get; set; }

        internal string basePath;
        internal GamesRepository repository;
        private ObservableCollection<Set> cachedSets;
        private IList<PropertyDef> cachedProperties;
        private IVistaDBDDA dda;
        internal IVistaDBDatabase db;
        private IVistaDBTable packTable, markerTable, cardTable;
        private Dictionary<Guid, CardModel> cardModelCache;
        private Dictionary<Guid, Set> setCache;

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
                if(cachedSets == null)
                    cachedSets = GetAllSets();
                return cachedSets;
            }
        }

        public IList<PropertyDef> CustomProperties
        {
            get
            {
                if(cachedProperties == null)
                    cachedProperties = GetCustomProperties();
                return cachedProperties;
            }
        }

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
        { get { return dda != null; } }

        public void OpenDatabase(bool readOnly)
        {
            System.Diagnostics.Debug.Assert(dda == null, "The database is already open");

            dda = VistaDBEngine.Connections.OpenDDA();
            db = dda.OpenDatabase(Path.Combine(basePath, "game.vdb3"),
              readOnly ? VistaDBDatabaseOpenMode.NonexclusiveReadOnly : VistaDBDatabaseOpenMode.ExclusiveReadWrite, null);
            packTable = db.OpenTable("Pack", false, readOnly);
            markerTable = db.OpenTable("Marker", false, readOnly);
            cardTable = db.OpenTable("Card", false, readOnly);
            cardModelCache = new Dictionary<Guid, CardModel>();
            setCache = new Dictionary<Guid, Set>();
        }

        public void CloseDatabase()
        {
            packTable.Dispose(); packTable = null;
            cardTable.Dispose(); cardTable = null;
            markerTable.Dispose(); markerTable = null;
            db.Dispose(); db = null;
            dda.Dispose(); dda = null;
            cardModelCache = null;
            setCache = null;
        }

        public Set GetSet(Guid id)
        {
            Set cachedSet;
            if(setCache.TryGetValue(id, out cachedSet))
                return cachedSet;
            using(var setTable = db.OpenTable("Set", false, true))
            {
                if(!setTable.Find("id:'" + id.ToString() + "'", "SetPK", false, false)) return null;
                var newSet = Set.FromDataRow(this, setTable.CurrentRow);
                setCache.Add(newSet.Id, newSet);
                return newSet;
            }
        }

        public CardModel GetCardByName(string name)
        {
            if(!cardTable.Find("name:'" + name.Replace("'", "''") + "'", "CardNameIX", false, false)) return null;
            CardModel cachedModel;
            if(cardModelCache.TryGetValue((Guid)cardTable.Get("id").Value, out cachedModel))
                return cachedModel;
            var newModel = CardModel.FromDataRow(this, cardTable.CurrentRow);
            cardModelCache.Add(newModel.Id, newModel);
            return newModel;
        }

        public CardModel GetCardById(Guid id)
        {
            CardModel cachedModel;
            if(cardModelCache.TryGetValue(id, out cachedModel))
                return cachedModel;
            if(!cardTable.Find("id:'" + id.ToString() + "'", "CardPK", false, false)) return null;
            var newModel = CardModel.FromDataRow(this, cardTable.CurrentRow);
            cardModelCache.Add(newModel.Id, newModel);
            return newModel;
        }

        public IEnumerable<MarkerModel> GetAllMarkers()
        {
            var result = new List<MarkerModel>();
            var previousGuid = Guid.Empty;
            markerTable.First();
            while(!markerTable.EndOfTable)
            {
                if(previousGuid != (Guid)markerTable.Get("id").Value)
                {
                    result.Add(MarkerModel.FromDataRow(this, markerTable.CurrentRow));
                    previousGuid = (Guid)markerTable.Get("id").Value;
                }
                markerTable.Next();
            }
            return result;
        }

        public Pack GetPackById(Guid id)
        {
            if(!packTable.Find("id:'" + id.ToString() + "'", "PackPK", false, false)) return null;
            var set = GetSet((Guid)packTable.Get("setId").Value);
            return new Pack(set, (string)packTable.Get("xml").Value);
        }

        public void InstallSet(string filename)
        {
            OpenDatabase(false);
            try
            {
                db.BeginTransaction();
                using(var package = Package.Open(filename, FileMode.Open, FileAccess.Read))
                {
                    var defRelationship = package.GetRelationshipsByType("http://schemas.octgn.org/set/definition").First();
                    var definition = package.GetPart(defRelationship.TargetUri);

                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.ValidationType = ValidationType.Schema;
                    settings.IgnoreWhitespace = true;
                    using(Stream s = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(GamesRepository), "CardSet.xsd"))
                    using(XmlReader reader = XmlReader.Create(s))
                        settings.Schemas.Add(null, reader);

                    Set set;

                    // Read the cards
                    using(var reader = XmlReader.Create(definition.GetStream(), settings))
                    {
                        reader.ReadToFollowing("set");  // <?xml ... ?>

                        set = new Set(filename, reader, repository);
                        if(set.Game != this)
                            throw new ApplicationException(string.Format("The set '{0}' is not built for the game '{1}'.", set.Name, Name));
                        if(set.GameVersion.Major != Version.Major || set.GameVersion.Minor != Version.Minor)
                            throw new ApplicationException(string.Format("The set '{0}' is incompatible with the installed game version.\nGame version: {1:2}\nSet made for version: {2:2}.", set.Name, Version, set.GameVersion));

                        InsertSet(set);

                        if(reader.IsStartElement("packaging"))
                        {
                            reader.ReadStartElement();  // <packaging>
                            while(reader.IsStartElement("pack"))
                            {
                                string xml = reader.ReadOuterXml();
                                var pack = new Pack(set, xml);
                                InsertPack(pack, xml, set.Id);
                            }
                            reader.ReadEndElement();  // </packaging>
                        }

                        if(reader.IsStartElement("markers"))
                        {
                            reader.ReadStartElement();  // <markers>
                            while(reader.IsStartElement("marker"))
                            {
                                reader.MoveToAttribute("name");
                                var markerName = reader.Value;
                                reader.MoveToAttribute("id");
                                var markerId = new Guid(reader.Value);
                                var markerImageUri = definition.GetRelationship("M" + markerId.ToString("N")).TargetUri;
                                var markerUri = markerImageUri.OriginalString;
                                if(!package.PartExists(markerImageUri))
                                    throw new ApplicationException(string.Format("Image for marker '{0}', with URI '{1}' was not found in the package.", markerName, markerUri));
                                reader.Read();  // <marker />
                                InsertMarker(markerId, markerName, markerUri, set.Id);
                            }
                            reader.ReadEndElement();    // </markers>
                        }

                        if(reader.IsStartElement("cards"))
                        {
                            reader.ReadStartElement();  // <cards>
                            while(reader.IsStartElement("card"))
                                InsertCard(new CardModel(reader, this, set, definition, package));
                            reader.ReadEndElement();    // </cards>
                        }

                        reader.ReadEndElement();
                    }

                    CopyDecks(package);
                    package.Close();

                    // Commit the changes
                    db.CommitTransaction();
                    if(cachedSets != null)
                        cachedSets.Add(set);
                }
            }
            catch
            {
                db.RollbackTransaction();
                throw;
            }
            finally
            { CloseDatabase(); }
        }

        public void DeleteSet(Set set)
        {
            OpenDatabase(false);
            try
            {
                using(var setTable = db.OpenTable("Set", false, false))
                {
                    if(setTable.Find("id:'" + set.Id + "'", "SetPK", false, false))
                    {
                        db.BeginTransaction();
                        try
                        {
                            setTable.Delete();
                            db.CommitTransaction();
                        }
                        catch
                        {
                            db.RollbackTransaction();
                            throw;
                        }
                        if(cachedSets != null)
                            cachedSets.Remove(set);
                    }
                }
            }
            finally
            { CloseDatabase(); }
        }

        private void InsertSet(Set set)
        {
            using(var setTable = db.OpenTable("Set", false, false))
            {
                if(setTable.Find("id:'" + set.Id + "'", "SetPK", false, false))
                    throw new ApplicationException(string.Format("The set '{0}' is already installed.", set.Id));
                setTable.Insert();
                setTable.PutGuid("id", set.Id);
                setTable.PutString("name", set.Name);
                setTable.PutString("gameVersion", set.GameVersion.ToString());
                setTable.PutString("version", set.Version.ToString());
                setTable.PutString("package", set.PackageName);
                setTable.Post();
            }
        }

        private void InsertPack(Pack pack, string xml, Guid setId)
        {
            packTable.Insert();
            packTable.PutGuid("id", pack.Id);
            packTable.PutString("name", pack.Name);
            packTable.PutGuid("setId", setId);
            packTable.PutString("xml", xml);
            packTable.Post();
        }

        private void InsertMarker(Guid id, string name, string iconUri, Guid setId)
        {
            markerTable.Insert();
            markerTable.PutGuid("id", id);
            markerTable.PutString("name", name);
            markerTable.PutString("icon", iconUri);
            markerTable.PutGuid("setId", setId);
            markerTable.Post();
        }

        private void InsertCard(CardModel card)
        {
            cardTable.Insert();
            cardTable.PutGuid("id", card.Id);
            cardTable.PutString("name", card.Name);
            cardTable.PutString("image", card.ImageUri);
            cardTable.PutGuid("setId", card.set.Id);
            foreach(KeyValuePair<string, object> pair in card.Properties)
            {
                if(pair.Value is string)
                    cardTable.PutString(pair.Key, (string)pair.Value);
                else if(pair.Value is int)
                    cardTable.PutInt32(pair.Key, (int)pair.Value);
                else		// char
                    cardTable.PutString(pair.Key, pair.Value.ToString());
            }
            cardTable.Post();
        }

        private ObservableCollection<Set> GetAllSets()
        {
            var result = new ObservableCollection<Set>();
            bool wasDbOpen = IsDatabaseOpen;
            if(!wasDbOpen)
                OpenDatabase(true);
            try
            {
                using(var setTable = db.OpenTable("Set", false, true))
                {
                    setTable.First();
                    while(!setTable.EndOfTable)
                    {
                        result.Add(Set.FromDataRow(this, setTable.CurrentRow));
                        setTable.Next();
                    }
                }
            }
            finally
            {
                if(!wasDbOpen)
                    CloseDatabase();
            }

            return result;
        }

        private List<PropertyDef> GetCustomProperties()
        {
            bool shouldClose = false;
            if(db == null)
            {
                OpenDatabase(true);
                shouldClose = true;
            }
            try
            {
                var result = new List<PropertyDef>();
                var schema = db.TableSchema("Card");
                var columns = from IVistaDBColumnAttributes col in schema
                              where col.Description != null && col.Description.StartsWith("Custom property")
                              select col;
                foreach(var col in columns)
                    result.Add(new PropertyDef(col.Name,
                        col.Type == VistaDBType.NVarChar ? PropertyType.String :
                        col.Type == VistaDBType.Int ? PropertyType.Integer :
                        PropertyType.Char));
                return result;
            }
            finally
            { if(shouldClose) CloseDatabase(); }
        }

        internal void CopyDecks(string filename)
        {
            using(Package package = Package.Open(filename, FileMode.Open, FileAccess.Read))
                CopyDecks(package);
        }

        internal void CopyDecks(Package package)
        {
            string path = Path.Combine(basePath, "Decks");
            var decks = package.GetRelationshipsByType("http://schemas.octgn.org/set/deck");
            byte[] buffer = new byte[0x1000];
            foreach(var deckRel in decks)
            {
                var deck = package.GetPart(deckRel.TargetUri);
                string deckFilename = Path.Combine(path, Path.GetFileName(deck.Uri.ToString()));
                using(var deckStream = deck.GetStream(FileMode.Open, FileAccess.Read))
                using(var targetStream = File.Open(deckFilename, FileMode.Create, FileAccess.Write))
                {
                    int read;
                    while(true)
                    {
                        read = deckStream.Read(buffer, 0, buffer.Length);
                        if(read == 0) break;
                        targetStream.Write(buffer, 0, read);
                    }
                }
            }
        }

        public System.Data.DataTable SelectCards(string[] conditions)
        {
            var sb = new StringBuilder();
            sb.Append("SELECT * FROM Card");
            if(conditions != null)
            {
                string connector = " WHERE ";
                foreach(string condition in conditions)
                {
                    sb.Append(connector);
                    sb.Append("(");
                    sb.Append(condition);
                    sb.Append(")");
                    connector = " AND ";
                }
            }
            using(var conn = new VistaDB.Provider.VistaDBConnection(db))
            {
                var cmd = new VistaDB.Provider.VistaDBCommand();
                cmd.Connection = conn;
                cmd.CommandText = sb.ToString();
                var result = new System.Data.DataTable();
                result.Load(cmd.ExecuteReader());
                return result;
            }
        }

        public IEnumerable<CardModel> SelectCardModels(params string[] conditions)
        {
            return from System.Data.DataRow row in SelectCards(conditions).Rows
                   select CardModel.FromDataRow(this, row);
        }

        public IEnumerable<CardModel> SelectRandomCardModels(int count, params string[] conditions)
        {
            if(count < 0) throw new ArgumentOutOfRangeException("count parameter must be greater or equal to 0.");
            if(count == 0) return Enumerable.Empty<CardModel>();

            var table = SelectCards(conditions);
            IEnumerable<System.Data.DataRow> candidates;
            if(table.Rows.Count <= count)
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