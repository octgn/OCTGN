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
        public static string BasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                                     "Octgn");

        private static readonly string DatabaseFile = Path.Combine(BasePath, "Database", "master.db3");
        private static readonly string ConString = "URI=file:" + DatabaseFile;
        internal static SQLiteConnection DatabaseConnection;
        private ObservableCollection<Game> _allCachedGames;
        private ObservableCollection<Game> _cachedGames;
        private List<string> _missingFiles;

        static GamesRepository()
        {
            BasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Octgn");
            Directory.CreateDirectory(BasePath);
        }

        public GamesRepository()
        {
            var buildSchema = false;
            if (!File.Exists(DatabaseFile))
            {
                SQLiteConnection.CreateFile(DatabaseFile);
                buildSchema = true;
            }

            DatabaseConnection = new SQLiteConnection(ConString);
            DatabaseConnection.Open();
            using (var com = DatabaseConnection.CreateCommand())
            {
                com.CommandText =
                    "PRAGMA automatic_index=FALSE; PRAGMA synchronous=OFF; PRAGMA auto_vacuum=INCREMENTAL; PRAGMA foreign_keys=ON; PRAGMA encoding='UTF-8';";
                com.ExecuteScalar();
            }

            if (buildSchema)
            {
                using (var com = DatabaseConnection.CreateCommand())
                {
                    var md = Resource1.MakeDatabase;
                    com.CommandText = md;
                    com.ExecuteNonQuery();
                }
            }

            using (var com = DatabaseConnection.CreateCommand())
            {
                var md = Resource1.UpdateDatabase;
                com.CommandText = md;
                com.ExecuteNonQuery();
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
            SQLiteTransaction trans = null;
            try
            {
                var sb = new StringBuilder();
                trans = DatabaseConnection.BeginTransaction();
                using (var com = DatabaseConnection.CreateCommand())
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

                    game.CopyDecks(game.Filename);
                }
                //Add custom properties for the card.
                sb = new StringBuilder();
                sb.Append("INSERT OR REPLACE INTO [custom_properties](");
                sb.Append("[id],[card_real_id],[game_id],[name], [type],[vint],[vstr]");
                sb.Append(") VALUES(");
                sb.Append(
                    "@id,(SELECT real_id FROM cards WHERE id = @card_id LIMIT 1),@game_id,@name,@type,@vint,@vstr");
                sb.Append(");\n");
                var command = sb.ToString();
                foreach (var pair in properties)
                {
                    using (var com = DatabaseConnection.CreateCommand())
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
                trans.Commit();
            }
            catch (Exception)
            {
                if (trans != null)
                    trans.Rollback();
                if (Debugger.IsAttached) Debugger.Break();
                return;
            }
            var existingGame = _cachedGames.FirstOrDefault(g => g.Id == game.Id);
            if (existingGame != null) _cachedGames.Remove(existingGame);
            _cachedGames.Add(game);
            if (GameInstalled != null)
                GameInstalled.Invoke(game, new EventArgs());
        }

        private void GetGamesList()
        {
            _allCachedGames = new ObservableCollection<Game>();
            using (var com = DatabaseConnection.CreateCommand())
            {
                com.CommandText = "SELECT * FROM games;";
                using (var read = com.ExecuteReader())
                {
                    while (read.Read())
                    {
                        _allCachedGames.Add(ReadGameFromTable(read));
                    }
                    read.Close();
                }
            }
            _missingFiles = (from g in _allCachedGames
                             let fullname = Path.Combine(BasePath, "Defs", g.Filename)
                             where !File.Exists(fullname)
                             select fullname).ToList();

            _cachedGames = _missingFiles.Count > 0
                               ? new ObservableCollection<Game>(
                                     _allCachedGames.Where(
                                         g => !_missingFiles.Contains(Path.Combine(BasePath, "Defs", g.Filename))))
                               : _allCachedGames;
        }

        private Game ReadGameFromTable(IDataRecord read)
        {
            var temp = read["shared_deck_sections"];
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
                            Repository = this
                        };


            return g;
        }

        private static string SerializeList(IEnumerable<string> list)
        {
            var sb = new StringBuilder();
            foreach (var item in list)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(item.Replace(",", ",,"));
            }
            return sb.ToString();
        }

        private static List<string> DeserializeList(string list)
        {
            var sections = Regex.Split(list, "(?<!,),(?!,)");
            return sections.Select(s => s.Replace(",,", ",")).ToList();
        }
    }
}