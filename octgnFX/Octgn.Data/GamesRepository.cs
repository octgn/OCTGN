using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public static readonly string BasePath;
        private readonly string masterDbPath;
        private ObservableCollection<Game> allCachedGames;
        private ObservableCollection<Game> cachedGames;
        private List<string> missingFiles;

        static GamesRepository()
        {
            BasePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Octgn");
            Directory.CreateDirectory(BasePath);
        }

        public GamesRepository()
        {
            masterDbPath = Path.Combine(BasePath, "Database", "master.db3");
            // Create the master base if needed
            if (!File.Exists(masterDbPath))
                CreateMasterDatabase();
        }

        public ObservableCollection<Game> Games
        {
            get
            {
                if (cachedGames == null)
                    GetGamesList();
                return cachedGames;
            }
        }

        /// <summary>
        /// All games. Includes games where the DEF file is missing.
        /// PROBUBLY SHOULDN'T USE THIS ONE DAWG.
        /// </summary>
        public ObservableCollection<Game> AllGames
        {
            get
            {
                if (allCachedGames == null)
                    GetGamesList();
                return allCachedGames;
            }
        }

        public List<string> MissingFiles
        {
            get
            {
                if (missingFiles == null)
                    GetGamesList();
                return missingFiles;
            }
        }

        public event EventHandler GameInstalled;

        public void InstallGame(Game game, IEnumerable<PropertyDef> properties)
        {
            SQLiteTransaction trans = null;
            try
            {
                using (var sc = new SQLiteConnection("URI=file:" + masterDbPath))
                {
                    sc.Open();
                    var sb = new StringBuilder();
                    trans = sc.BeginTransaction();
                    using (SQLiteCommand com = sc.CreateCommand())
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
                    string command = sb.ToString();
                    foreach (PropertyDef pair in properties)
                    {
                        using (SQLiteCommand com = sc.CreateCommand())
                        {
                            com.CommandText = command;
                            com.Parameters.AddWithValue("@card_id", "");
                            com.Parameters.AddWithValue("@vint", 0);
                            com.Parameters.AddWithValue("@vstr", " ");
                            com.Parameters.AddWithValue("@id", pair.Name + game.Id.ToString());
                            com.Parameters.AddWithValue("@game_id", game.Id.ToString());
                            com.Parameters.AddWithValue("@name", pair.Name);
                            if (pair.Type == PropertyType.String)
                                com.Parameters.AddWithValue("@type", 0);
                            else if (pair.Type == PropertyType.Integer)
                                com.Parameters.AddWithValue("@type", 1);
                            else // char
                                com.Parameters.AddWithValue("@type", 2);
                            com.ExecuteNonQuery();
                        }
                    }
                    trans.Commit();
                    sc.Close();
                }
            }
            catch (Exception)
            {
                if (trans != null)
                    trans.Rollback();
                if (Debugger.IsAttached) Debugger.Break();
                return;
            }
            Game existingGame = cachedGames.FirstOrDefault(g => g.Id == game.Id);
            if (existingGame != null) cachedGames.Remove(existingGame);
            cachedGames.Add(game);
            if (GameInstalled != null)
                GameInstalled.Invoke(game, new EventArgs());
        }

        private void CreateMasterDatabase()
        {
            SQLiteConnection.CreateFile(masterDbPath);
            using (var sc = new SQLiteConnection("URI=file:" + masterDbPath))
            {
                sc.Open();
                SQLiteCommand com = sc.CreateCommand();
                string md = Resource1.MakeDatabase;
                com.CommandText = md;
                try
                {
                    com.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    if (Debugger.IsAttached)
                        Debugger.Break();
                    else
                        throw;
                }
                sc.Close();
            }
        }

        private void GetGamesList()
        {
            allCachedGames = new ObservableCollection<Game>();
            using (var sc = new SQLiteConnection("URI=file:" + masterDbPath))
            {
                sc.Open();
                using (SQLiteCommand com = sc.CreateCommand())
                {
                    com.CommandText = "SELECT * FROM games;";
                    using (SQLiteDataReader read = com.ExecuteReader())
                    {
                        while (read.Read())
                        {
                            allCachedGames.Add(ReadGameFromTable(read));
                        }
                        read.Close();
                    }
                }
                sc.Close();
            }
            missingFiles = (from g in allCachedGames
                            let fullname = Path.Combine(g.basePath, "Defs", g.Filename)
                            where !File.Exists(fullname)
                            select fullname).ToList();

            cachedGames = missingFiles.Count > 0
                              ? new ObservableCollection<Game>(
                                    allCachedGames.Where(
                                        g => !missingFiles.Contains(Path.Combine(g.basePath, "Defs", g.Filename))))
                              : allCachedGames;
        }

        private Game ReadGameFromTable(SQLiteDataReader read)
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
                            basePath = BasePath,
                            repository = this
                        };


            return g;
        }

        private string SerializeList(IEnumerable<string> list)
        {
            var sb = new StringBuilder();
            foreach (string item in list)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(item.Replace(",", ",,"));
            }
            return sb.ToString();
        }

        private List<string> DeserializeList(string list)
        {
            string[] sections = Regex.Split(list, "(?<!,),(?!,)");
            return sections.Select(s => s.Replace(",,", ",")).ToList();
        }
    }
}