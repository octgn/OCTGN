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
        public static string BasePath = SimpleConfig.ReadValue("datadirectory", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn"));

        private static readonly string DatabasePath = Path.Combine(BasePath, "Database");
        private static readonly string DatabaseFile = Path.Combine(DatabasePath, "master.db3");
        private static readonly string ConString = "URI=file:" + DatabaseFile;
        internal static SQLiteConnection DatabaseConnection;
        private ObservableCollection<Game> _allCachedGames;
        private ObservableCollection<Game> _cachedGames;
        private List<string> _missingFiles;

        static GamesRepository()
        {
            BasePath = SimpleConfig.ReadValue("datadirectory", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn"));
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
        public event EventHandler<EventArgs<Game>> GameUninstalled;

        
        /// <summary>
        /// Raises the GameUninstalled event if any listeners are attached.
        /// </summary>
        /// <param name="uninstalled">The Game that was uninstalled</param>
        public void RaiseGameInstalled(Game installed)
        {
            if (GameInstalled != null)
            {
                //TODO: Change usage to recommended pattern
                GameInstalled(installed, EventArgs.Empty);
            }
        }
        
        /// <summary>
        /// Raises the GameUninstalled event if any listeners are attached.
        /// </summary>
        /// <param name="uninstalled">The Game that was uninstalled</param>
        public void RaiseGameUninstalled(Game uninstalled)
        {
            if (GameUninstalled != null)
            {
                GameUninstalled(this, new EventArgs<Game>(uninstalled));
            }
        }
        
        
        
        /// <summary>
        /// Removes a game and all it's related entries from the database.
        /// </summary>
        /// <param name="game">The game to uninstall</param>
        public void UninstallGame(Game game)
        {
            /*
             * games
             * +- // cards (via game.game_id)
             * +- custom_properties (via game.game_id)
             * +- // markers (via game.game_id)
             * +- sets (via game.real_id)
             *    +- cards (via set.real_id)
             *    +- packs (via set.real_id)
             *    +- markers (via set.real_id)
             */
            
            // If game doesn't exist, abort
            if (!_cachedGames.Any(g => g.Id == game.Id)) return;
            
            SQLiteTransaction trans = null;
            try
            {
                trans = DatabaseConnection.BeginTransaction();
                
                #region remove the cards from the database
                using (SQLiteCommand com = DatabaseConnection.CreateCommand())
                {
                    //Build Query
                    com.CommandText = 
@"DELETE FROM [cards]
WHERE [set_real_id]=(
    SELECT [real_id] FROM [sets] WHERE [game_real_id]=(
        SELECT [real_id] FROM [games] WHERE [id]=@id
    )
);";
                    com.Parameters.AddWithValue("@id", game.Id.ToString());
                    com.ExecuteNonQuery();
                }
                #endregion
                
                #region remove the packs from the database
                using (SQLiteCommand com = DatabaseConnection.CreateCommand())
                {
                    //Build Query
                    com.CommandText = 
@"DELETE FROM [packs]
WHERE [set_real_id]=(
    SELECT [real_id] FROM [sets] WHERE [game_real_id]=(
        SELECT [real_id] FROM [games] WHERE [id]=@id
    )
);";
                    com.Parameters.AddWithValue("@id", game.Id.ToString());
                    com.ExecuteNonQuery();
                }
                #endregion
                
                #region remove the markers from the database
                using (SQLiteCommand com = DatabaseConnection.CreateCommand())
                {
                    //Build Query
                    com.CommandText = 
@"DELETE FROM [markers]
WHERE [set_real_id]=(
    SELECT [real_id] FROM [sets] WHERE [game_real_id]=(
        SELECT [real_id] FROM [games] WHERE [id]=@id
    )
);";
                    com.Parameters.AddWithValue("@id", game.Id.ToString());
                    com.ExecuteNonQuery();
                }
                #endregion
                
                #region remove the sets from the database
                using (SQLiteCommand com = DatabaseConnection.CreateCommand())
                {
                    //Build Query
                    com.CommandText =
@"DELETE FROM [sets]
WHERE [game_real_id]=(
    SELECT [real_id] FROM [games] WHERE [id]=@id
);";
                    com.Parameters.AddWithValue("@id", game.Id.ToString());
                    com.ExecuteNonQuery();
                }
                #endregion
                
                #region remove the custom properties from the database
                using (SQLiteCommand com = DatabaseConnection.CreateCommand())
                {
                    //Build Query
                    com.CommandText = @"DELETE FROM [custom_properties] WHERE [game_id]=@id;";
                    com.Parameters.AddWithValue("@id", game.Id.ToString());
                    com.ExecuteNonQuery();
                }
                #endregion
                
                #region remove the game from the database
                using (SQLiteCommand com = DatabaseConnection.CreateCommand())
                {
                    //Build Query
                    com.CommandText = @"DELETE FROM [games] WHERE [id]=@id;";
                    com.Parameters.AddWithValue("@id", game.Id.ToString());
                    com.ExecuteNonQuery();
                }
                #endregion
                
                trans.Commit();
            }
            catch (Exception ex)
            {
                if (trans != null)
                    trans.Rollback();
                if (Debugger.IsAttached) Debugger.Break();
                return;
            }
            
            var existingGame = _cachedGames.FirstOrDefault(g => g.Id == game.Id);
            if (existingGame != null) _cachedGames.Remove(existingGame);
            
            RaiseGameUninstalled(game);
        }
        
        /// <summary>
        /// Adds a game and all directly related entries to the database.
        /// </summary>
        /// <param name="game">The game definition to install</param>
        /// <param name="properties">The card properties to install</param>
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
                    #region Update the game entry
                    using (SQLiteCommand com = DatabaseConnection.CreateCommand())
                    {
                        //Build Query
                        com.CommandText =
@"UPDATE [games] SET
    [filename]=@filename,
    [version]=@version,
    [card_width]=@card_width,
    [card_height]=@card_height,
    [card_back]=@card_back,
    [deck_sections]=@deck_sections,
    [shared_deck_sections]=@shared_deck_sections,
    [file_hash]=@file_hash
WHERE [id]=@id;";

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
                    }
                    #endregion
                }
                else
                {
                    #region Insert a new game entry
                    using (SQLiteCommand com = DatabaseConnection.CreateCommand())
                    {
                        //Build Query
                        com.CommandText =
@"INSERT OR REPLACE INTO [games](
    [id], [name], [filename], [version], [card_width], [card_height], [card_back], [deck_sections], [shared_deck_sections], [file_hash]
) VALUES (
    @id, @name, @filename, @version, @card_width, @card_height, @card_back, @deck_sections, @shared_deck_sections, @file_hash
);";

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
                    #endregion
                }
                
                #region Add custom properties for the card.
                string command =
@"INSERT OR REPLACE INTO [custom_properties](
    [id], [card_real_id], [game_id], [name], [type], [vint], [vstr]
) VALUES(
    @id, (
        SELECT real_id FROM cards WHERE id = @card_id LIMIT 1
    ), @game_id, @name, @type, @vint, @vstr
);";
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
                #endregion
                
                trans.Commit();
            }
            catch (Exception ex)
            {
                if (trans != null)
                    trans.Rollback();
                if (Debugger.IsAttached) Debugger.Break();
                return;
                /*
   bei System.Data.SQLite.SQLite3.Prepare(SQLiteConnection cnn, String strSql, SQLiteStatement previous, UInt32 timeoutMS, String& strRemain) in c:\dev\sqlite\dotnet\System.Data.SQLite\SQLite3.cs:Zeile 476.
   bei System.Data.SQLite.SQLiteCommand.BuildNextCommand() in c:\dev\sqlite\dotnet\System.Data.SQLite\SQLiteCommand.cs:Zeile 294.
   bei System.Data.SQLite.SQLiteCommand.GetStatement(Int32 index) in c:\dev\sqlite\dotnet\System.Data.SQLite\SQLiteCommand.cs:Zeile 306.
   bei System.Data.SQLite.SQLiteDataReader.NextResult() in c:\dev\sqlite\dotnet\System.Data.SQLite\SQLiteDataReader.cs:Zeile 1146.
   bei System.Data.SQLite.SQLiteDataReader..ctor(SQLiteCommand cmd, CommandBehavior behave) in c:\dev\sqlite\dotnet\System.Data.SQLite\SQLiteDataReader.cs:Zeile 103.
   bei System.Data.SQLite.SQLiteCommand.ExecuteReader(CommandBehavior behavior) in c:\dev\sqlite\dotnet\System.Data.SQLite\SQLiteCommand.cs:Zeile 592.
   bei System.Data.SQLite.SQLiteCommand.ExecuteNonQuery() in c:\dev\sqlite\dotnet\System.Data.SQLite\SQLiteCommand.cs:Zeile 622.
   bei Octgn.Data.GamesRepository.InstallGame(Game game, IEnumerable`1 properties) in c:\Users\dschachtler\Documents\SharpDevelop Projects\OCTGN\octgnFX\Octgn.Data\GamesRepository.cs:Zeile 330."                 */
            }
            existingGame = _cachedGames.FirstOrDefault(g => g.Id == game.Id);
            if (existingGame != null) _cachedGames.Remove(existingGame);
            _cachedGames.Add(game);
            
            RaiseGameInstalled(game);
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