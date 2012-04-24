using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Octgn.Data.Properties;

namespace Octgn.Data
{
    public class GamesRepository
    {
        public static string BasePath = SimpleConfig.DataDirectory;

        private static readonly string DatabasePath = Path.Combine(BasePath, "Database");
        private static readonly string DatabaseFile = Path.Combine(DatabasePath, "master.db3");
        private static readonly string ConString = "URI=file:" + DatabaseFile;
        internal static SQLiteConnection DatabaseConnection;
        private ObservableCollection<Game> _allCachedGames;
        private ObservableCollection<Game> _cachedGames;
        private List<string> _missingFiles;

        static GamesRepository()
        {
            BasePath = SimpleConfig.DataDirectory;
            Directory.CreateDirectory(BasePath);
        }

        public GamesRepository()
        {
            bool buildSchema = false;
            if (!File.Exists(DatabaseFile))
            {
                Directory.CreateDirectory(DatabasePath);
                SQLiteConnection.CreateFile(DatabaseFile);
                buildSchema = true;
            }

            DatabaseConnection = new SQLiteConnection(ConString);
            DatabaseConnection.Open();
            using (SQLiteCommand com = DatabaseConnection.CreateCommand())
            {
                com.CommandText =
                    "PRAGMA automatic_index=FALSE; PRAGMA synchronous=OFF; PRAGMA auto_vacuum=INCREMENTAL; PRAGMA foreign_keys=ON; PRAGMA encoding='UTF-8';";
                com.ExecuteScalar();
            }

            if (buildSchema)
            {
                using (SQLiteCommand com = DatabaseConnection.CreateCommand())
                {
                    string md = Resource1.MakeDatabase;
                    com.CommandText = md;
                    com.ExecuteNonQuery();
                }
            }

            using (SQLiteCommand com = DatabaseConnection.CreateCommand())
            {
                UpdateDatabase.Update(DatabaseConnection);
            }
        }

        public ObservableCollection<Game> Games
        {
            get
            {
                if (_cachedGames == null)
                    GetGamesList();
                return _cachedGames;
            }
        }

        /// <summary>
        ///   All games. Includes games where the DEF file is missing. PROBUBLY SHOULDN'T USE THIS ONE DAWG.
        /// </summary>
        public ObservableCollection<Game> AllGames
        {
            get
            {
                if (_allCachedGames == null)
                    GetGamesList();
                return _allCachedGames;
            }
        }

        public List<string> MissingFiles
        {
            get
            {
                if (_missingFiles == null)
                    GetGamesList();
                return _missingFiles;
            }
        }

        public event EventHandler GameInstalled;

        public void InstallGame(Game game, IEnumerable<PropertyDef> properties)
        {
            Game existingGame = _cachedGames.FirstOrDefault(g => g.Id == game.Id);
            SQLiteTransaction trans = null;
            try
            {
                var sb = new StringBuilder();
                trans = DatabaseConnection.BeginTransaction();

                if (existingGame != null && existingGame.Id == game.Id)
                {
                    UpdateGameDefinition(game, properties);
                }
                else
                {
                    using (SQLiteCommand com = DatabaseConnection.CreateCommand())
                    {
                        //Build Query
                        sb.Append("INSERT OR REPLACE INTO [games](");
                        sb.Append(
                            "[id],[name],[filename],[version], [card_width],[card_height],[card_back],[deck_sections],[shared_deck_sections],[file_hash]");
                        sb.Append(") VALUES(");
                        sb.Append(
                            "@id,@name,@filename,@version,@card_width,@card_height,@card_back,@deck_sections,@shared_deck_sections,@file_hash");
                        sb.Append(");\n");
                        com.CommandText = sb.ToString();

                        com.Parameters.AddWithValue("@id", game.Id.ToString());
                        com.Parameters.AddWithValue("@name", game.Name);
                        com.Parameters.AddWithValue("@filename", game.Filename);
                        com.Parameters.AddWithValue("@version", game.Version.ToString());
                        com.Parameters.AddWithValue("@card_width", game.CardWidth);
                        com.Parameters.AddWithValue("@card_height", game.CardHeight);
                        com.Parameters.AddWithValue("@card_back", game.CardBack);
                        com.Parameters.AddWithValue("@deck_sections", SerializeList(game.DeckSections));
                        if (game.SharedDeckSections != null)
                            com.Parameters.AddWithValue("@shared_deck_sections", SerializeList(game.SharedDeckSections));
                        else
                            com.Parameters.AddWithValue("@shared_deck_sections", DBNull.Value);
                        com.Parameters.AddWithValue("@file_hash", game.FileHash);

                        com.ExecuteNonQuery();
                        if (!Directory.Exists(Path.Combine(BasePath, "Decks")))
                            Directory.CreateDirectory(Path.Combine(BasePath, "Decks"));

                        game.CopyDecks();
                    }
                }
                //Add custom properties for the card.
                sb = new StringBuilder();
                sb.Append("INSERT OR REPLACE INTO [custom_properties](");
                sb.Append("[id],[card_real_id],[game_id],[name], [type],[vint],[vstr]");
                sb.Append(") VALUES(");
                sb.Append(
                    "@id,(SELECT real_id FROM cards WHERE id = @card_id LIMIT 1),@game_id,@name,@type,@vint,@vstr");
                sb.Append(");\n");
                string command = sb.ToString();
                foreach (PropertyDef pair in properties)
                {
                    if (!DatabaseHandler.ColumnExists("cards", pair.Name, DatabaseConnection))
                    {
                        DatabaseHandler.AddColumn("cards", pair.Name, pair.Type, DatabaseConnection);
                    }

                    using (SQLiteCommand com = DatabaseConnection.CreateCommand())
                    {
                        com.CommandText = command;
                        com.Parameters.AddWithValue("@card_id", "");
                        com.Parameters.AddWithValue("@vint", 0);
                        com.Parameters.AddWithValue("@vstr", " ");
                        com.Parameters.AddWithValue("@id", pair.Name + game.Id);
                        com.Parameters.AddWithValue("@game_id", game.Id.ToString());
                        com.Parameters.AddWithValue("@name", pair.Name);
                        switch (pair.Type)
                        {
                            case PropertyType.String:
                                com.Parameters.AddWithValue("@type", 0);
                                break;
                            case PropertyType.Integer:
                                com.Parameters.AddWithValue("@type", 1);
                                break;
                            default:
                                com.Parameters.AddWithValue("@type", 2);
                                break;
                        }
                        com.ExecuteNonQuery();
                    }
                }
                DatabaseHandler.RebuildCardTable(DatabaseConnection);
            trans.Commit();
            }
            catch (Exception)
            {
                if (trans != null)
                    trans.Rollback();
                if (Debugger.IsAttached) Debugger.Break();
                return;
            }
            existingGame = _cachedGames.FirstOrDefault(g => g.Id == game.Id);
            if (existingGame != null) _cachedGames.Remove(existingGame);
            _cachedGames.Add(game);
            if (GameInstalled != null)
                GameInstalled.Invoke(game, new EventArgs());
        }

        public void UpdateGameDefinition(Game game, IEnumerable<PropertyDef> properties)
        {
            StringBuilder sb = new StringBuilder();
            using (SQLiteCommand com = DatabaseConnection.CreateCommand())
            {
                //Build Query
                sb.Append("UPDATE [games] SET ");
                sb.Append("[filename]=@filename, [version]=@version, ");
                sb.Append("[card_width]=@card_width, [card_height]=@card_height, [card_back]=@card_back, ");
                sb.Append("[deck_sections]=@deck_sections, [shared_deck_sections]=@shared_deck_sections, [file_hash]=@file_hash");
                sb.Append(" WHERE [id]=@id;");
                com.CommandText = sb.ToString();

                com.Parameters.AddWithValue("@id", game.Id.ToString());
                com.Parameters.AddWithValue("@filename", game.Filename);
                com.Parameters.AddWithValue("@version", game.Version.ToString());
                com.Parameters.AddWithValue("@card_width", game.CardWidth);
                com.Parameters.AddWithValue("@card_height", game.CardHeight);
                com.Parameters.AddWithValue("@card_back", game.CardBack);
                com.Parameters.AddWithValue("@deck_sections", SerializeList(game.DeckSections));
                if (game.SharedDeckSections != null)
                    com.Parameters.AddWithValue("@shared_deck_sections", SerializeList(game.SharedDeckSections));
                else
                    com.Parameters.AddWithValue("@shared_deck_sections", DBNull.Value);
                com.Parameters.AddWithValue("@file_hash", game.FileHash);

                com.ExecuteNonQuery();

                com.CommandText = "DELETE FROM [custom_properties] WHERE [game_id]=@game_id";
                com.Parameters.AddWithValue("@game_id", game.Id.ToString());
                com.ExecuteNonQuery();
            }
        }

        public void UpdateGameHash(Game game, string hash)
        {
            try
            {
                using (var com = DatabaseConnection.CreateCommand())
                {
                    com.CommandText = "UPDATE games SET file_hash=@file_hash WHERE id=@id";
                    com.Parameters.AddWithValue("@file_hash", hash);
                    com.Parameters.AddWithValue("@id", game.Id.ToString());
                    com.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                if(Debugger.IsAttached)Debugger.Break();
            }

        }

        private void GetGamesList()
        {
            _allCachedGames = new ObservableCollection<Game>();
            using (SQLiteCommand com = DatabaseConnection.CreateCommand())
            {
                com.CommandText = "SELECT * FROM games;";
                using (SQLiteDataReader read = com.ExecuteReader())
                {
                    while (read.Read())
                    {
                        _allCachedGames.Add(ReadGameFromTable(read));
                    }
                    read.Close();
                }
            }
            _missingFiles = (from g in _allCachedGames
                             let fullname =g.FullPath
                             where !File.Exists(fullname)
                             select fullname).ToList();

            _cachedGames = _missingFiles.Count > 0
                               ? new ObservableCollection<Game>(
                                     _allCachedGames.Where(
                                         g => !_missingFiles.Contains(g.FullPath)))
                               : _allCachedGames;
        }

        private Game ReadGameFromTable(IDataRecord read)
        {
            object temp = read["shared_deck_sections"];
            string sharedDeckSections;
            if (temp == DBNull.Value)
                sharedDeckSections = null;
            else
                sharedDeckSections = (string) read["shared_deck_sections"];
            
            var g = new Game
                        {
                            Id = Guid.Parse((string) read["id"]),
                            Name = (string) read["name"],
                            Version = new Version((string) read["version"]),
                            Filename = (string) read["filename"],
                            CardWidth = (int) ((long) read["card_width"]),
                            CardHeight = (int) ((long) read["card_height"]),
                            CardBack = (string) read["card_back"],
                            DeckSections = DeserializeList((string) read["deck_sections"]),
                            SharedDeckSections =
                                sharedDeckSections == null ? null : DeserializeList(sharedDeckSections),
                            Repository = this,
                            FileHash = read["file_hash"] == DBNull.Value ? "" : (string) read["file_hash"]
                        };


            return g;
        }

        private static string SerializeList(IEnumerable<string> list)
        {
            var sb = new StringBuilder();
            foreach (string item in list)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(item.Replace(",", ",,"));
            }
            return sb.ToString();
        }

        private static List<string> DeserializeList(string list)
        {
            string[] sections = Regex.Split(list, "(?<!,),(?!,)");
            return sections.Select(s => s.Replace(",,", ",")).ToList();
        }
    }
}