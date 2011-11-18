using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VistaDB.DDA;
using VistaDB;
using System.IO.Packaging;
using System.Collections.ObjectModel;

namespace Octgn.Data
{
  public class GamesRepository
  {
    public static readonly string BasePath;
    private List<string> missingFiles;
    private string masterDbPath;
    private ObservableCollection<Game> cachedGames, allCachedGames;
      public event EventHandler GameInstalled;

      static GamesRepository()
    {
      BasePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Octgn");
      Directory.CreateDirectory(BasePath);
    }

    public GamesRepository()
    {      
      masterDbPath = Path.Combine(BasePath, "master.vdb3");      

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

    // This collection is a little bit unsafe because it includes 
    // games, whose .o8g file is missing.
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

    public void InstallGame(Game game, IEnumerable<PropertyDef> properties)
    {
      game.basePath = Path.Combine(BasePath, game.Id.ToString());
      game.repository = this;
      using (var dda = VistaDBEngine.Connections.OpenDDA())
      using (var masterDb = dda.OpenDatabase(masterDbPath, VistaDBDatabaseOpenMode.NonexclusiveReadWrite, null))
      using (var gamesTable = masterDb.OpenTable("Game", false, false))
      {
        masterDb.BeginTransaction();
        bool previousCompatibleVersion = false;
        try
        {
          if (!gamesTable.Find("id:'" + game.Id.ToString() + "'", "GamePK", false, false))
            gamesTable.Insert();
          else
            previousCompatibleVersion = CheckCompatibility(ReadGameFromTable(gamesTable), properties);
          gamesTable.PutGuid("id", game.Id);
          gamesTable.PutString("name", game.Name);
          gamesTable.PutString("version", game.Version.ToString());
          gamesTable.PutString("filename", game.Filename);
          gamesTable.PutInt32("cardWidth", game.CardWidth);
          gamesTable.PutInt32("cardHeight", game.CardHeight);
          gamesTable.PutString("cardBack", game.CardBack);
          gamesTable.PutString("deckSections", SerializeList(game.DeckSections));
          if (game.SharedDeckSections != null)
            gamesTable.PutString("sharedDeckSections", SerializeList(game.SharedDeckSections));
          gamesTable.Post();

          string gamePath = Path.Combine(BasePath, game.Id.ToString());
          Directory.CreateDirectory(Path.Combine(gamePath, "Decks"));
          Directory.CreateDirectory(Path.Combine(gamePath, "Sets"));
          game.CopyDecks(game.Filename);          
          if (!previousCompatibleVersion)
            CreateGameDatabase(dda, gamePath, properties);

          masterDb.CommitTransaction();
        }
        catch
        {
          masterDb.RollbackTransaction();
          throw;
        }
      }
      var existingGame = cachedGames.FirstOrDefault(g => g.Id == game.Id);
      if (existingGame != null) cachedGames.Remove(existingGame);
      cachedGames.Add(game);
        if(GameInstalled != null)
            GameInstalled.Invoke(game,new EventArgs());
    }

    #region DB creation

    private void CreateMasterDatabase()
    {
      using (var dda = VistaDBEngine.Connections.OpenDDA())
      using (var masterDb = dda.CreateDatabase(masterDbPath, true, null, 0, 0, false))
      {
        var gamesSchema = masterDb.NewTable("Game");
        gamesSchema.AddColumn("id", VistaDBType.UniqueIdentifier);
        gamesSchema.DefineColumnAttributes("id", false, false, false, false, null, null);
        gamesSchema.AddColumn("name", VistaDBType.NVarChar, 200);
        gamesSchema.DefineColumnAttributes("name", false, false, false, false, null, null);
        gamesSchema.AddColumn("filename", VistaDBType.NVarChar, 300);
        gamesSchema.DefineColumnAttributes("name", false, false, false, false, null, null);
        gamesSchema.AddColumn("version", VistaDBType.VarChar, 30);
        gamesSchema.DefineColumnAttributes("version", false, false, false, false, null, null);
        gamesSchema.AddColumn("cardWidth", VistaDBType.Int);
        gamesSchema.DefineColumnAttributes("cardWidth", false, false, false, false, null, null);
        gamesSchema.AddColumn("cardHeight", VistaDBType.Int);
        gamesSchema.DefineColumnAttributes("cardHeight", false, false, false, false, null, null);
        gamesSchema.AddColumn("cardBack", VistaDBType.NVarChar, 200);
        gamesSchema.DefineColumnAttributes("cardBack", false, false, false, false, null, null);
        gamesSchema.AddColumn("deckSections", VistaDBType.NVarChar, 1000);
        gamesSchema.DefineColumnAttributes("deckSections", false, false, false, false, null, null);
        gamesSchema.AddColumn("sharedDeckSections", VistaDBType.NVarChar, 1000);
        gamesSchema.DefineColumnAttributes("sharedDeckSections", true, false, false, false, null, null);
        using (var gamesTable = masterDb.CreateTable(gamesSchema, false, false))
          gamesTable.CreateIndex("GamePK", "id", true, true);
      }
    }

    private void CreateGameDatabase(IVistaDBDDA dda, string gamePath, IEnumerable<PropertyDef> properties)
    {
      string dbFile = Path.Combine(gamePath, "game.vdb3");
      if (File.Exists(dbFile)) File.Delete(dbFile);
      using (var gameDb = dda.CreateDatabase(dbFile, true, null, 0, 0, false))
      {
        // Table 'Set'
        var setsSchema = gameDb.NewTable("Set");
        setsSchema.AddColumn("id", VistaDBType.UniqueIdentifier);
        setsSchema.DefineColumnAttributes("id", false, false, false, false, null, null);
        setsSchema.AddColumn("name", VistaDBType.NVarChar, 200);
        setsSchema.DefineColumnAttributes("name", false, false, false, false, null, null);
        setsSchema.AddColumn("gameVersion", VistaDBType.NVarChar, 30);
        setsSchema.DefineColumnAttributes("gameVersion", false, false, false, false, null, null);
        setsSchema.AddColumn("version", VistaDBType.NVarChar, 30);
        setsSchema.DefineColumnAttributes("version", false, false, false, false, null, null);
        setsSchema.AddColumn("package", VistaDBType.NVarChar, 300);
        setsSchema.DefineColumnAttributes("package", false, false, false, false, null, null);
        using (var setTable = gameDb.CreateTable(setsSchema, false, false))
        {
          setTable.CreateIndex("SetPK", "id", true, true);
        }

        // Table 'Card'
        var cardsSchema = gameDb.NewTable("Card");
        cardsSchema.AddColumn("id", VistaDBType.UniqueIdentifier);
        cardsSchema.DefineColumnAttributes("id", false, false, false, false, null, null);
        cardsSchema.AddColumn("name", VistaDBType.NVarChar, 200);
        cardsSchema.DefineColumnAttributes("name", false, false, false, false, null, null);
        cardsSchema.AddColumn("image", VistaDBType.NVarChar, 200);
        cardsSchema.DefineColumnAttributes("image", false, false, false, false, null, null);
        cardsSchema.AddColumn("setId", VistaDBType.UniqueIdentifier);
        cardsSchema.DefineColumnAttributes("setId", false, false, false, false, null, null);
        foreach (PropertyDef prop in properties)
        {
          switch (prop.Type)
          {
            case PropertyType.Char:
              cardsSchema.AddColumn(prop.Name, VistaDBType.NChar, 1); break;
            case PropertyType.Integer:
              cardsSchema.AddColumn(prop.Name, VistaDBType.Int); break;
            case PropertyType.String:
              cardsSchema.AddColumn(prop.Name, VistaDBType.NVarChar, 4000); break;
            default:
              throw new ArgumentOutOfRangeException("Unknown data type: " + prop.Type.ToString());
          }
          cardsSchema.DefineColumnAttributes(prop.Name, true, false, false, false, null, "Custom property");
        }
        using (var gameTable = gameDb.CreateTable(cardsSchema, false, false))
        {
          gameTable.CreateIndex("CardPK", "id", true, true);
          gameTable.CreateIndex("CardNameIX", "name", false, false);
          gameTable.CreateForeignKey("CardSetFK", "setId", "Set", VistaDBReferentialIntegrity.Cascade, VistaDBReferentialIntegrity.Cascade, null);
        }

        // Table 'Marker'
        var markerSchema = gameDb.NewTable("Marker");
        markerSchema.AddColumn("id", VistaDBType.UniqueIdentifier);
        markerSchema.DefineColumnAttributes("id", false, false, false, false, null, null);
        markerSchema.AddColumn("name", VistaDBType.NVarChar, 200);
        markerSchema.DefineColumnAttributes("name", false, false, false, false, null, null);
        markerSchema.AddColumn("icon", VistaDBType.NVarChar, 200);
        markerSchema.DefineColumnAttributes("icon", false, false, false, false, null, null);
        markerSchema.AddColumn("setId", VistaDBType.UniqueIdentifier);
        markerSchema.DefineColumnAttributes("setId", false, false, false, false, null, null);
        using (var markerTable = gameDb.CreateTable(markerSchema, false, false))
        {
          markerTable.CreateIndex("MarkerPK", "id; setId", true, true);
          markerTable.CreateForeignKey("MarkerSetFK", "setId", "Set", VistaDBReferentialIntegrity.Cascade, VistaDBReferentialIntegrity.Cascade, null);
        }

        // Table 'Pack'
        CreatePackTable(gameDb); 
      }
    }

    private static void CreatePackTable(IVistaDBDatabase gameDb)
    {
      var packSchema = gameDb.NewTable("Pack");
      packSchema.AddColumn("id", VistaDBType.UniqueIdentifier);
      packSchema.DefineColumnAttributes("id", false, false, false, false, null, null);
      packSchema.AddColumn("name", VistaDBType.NVarChar, 200);
      packSchema.DefineColumnAttributes("name", false, false, false, false, null, null);
      packSchema.AddColumn("setId", VistaDBType.UniqueIdentifier);
      packSchema.DefineColumnAttributes("setId", false, false, false, false, null, null);
      packSchema.AddColumn("xml", VistaDBType.NText);
      using (var packTable = gameDb.CreateTable(packSchema, false, false))
      {
        packTable.CreateIndex("PackPK", "id", true, true);
        packTable.CreateIndex("SetIX", "setId", false, false);
        packTable.CreateForeignKey("PackSetFK", "setId", "Set", VistaDBReferentialIntegrity.Cascade, VistaDBReferentialIntegrity.Cascade, null);
      }
    }

    #endregion

    private void GetGamesList()
    {
      using (var dda = VistaDBEngine.Connections.OpenDDA())
      using (var masterDb = dda.OpenDatabase(masterDbPath, VistaDBDatabaseOpenMode.NonexclusiveReadOnly, null))
      using (var gamesTable = masterDb.OpenTable("Game", false, true))
      {
        allCachedGames = new ObservableCollection<Game>();
        gamesTable.First();
        while (!gamesTable.EndOfTable)
        {
          allCachedGames.Add(ReadGameFromTable(gamesTable));
          gamesTable.Next();
        }

        missingFiles = (from g in allCachedGames
                        let fullname = Path.Combine(g.basePath, g.Filename)
                        where !File.Exists(fullname)
                        select fullname).ToList();                                     
        
        if (missingFiles.Count > 0)
          cachedGames = new ObservableCollection<Game>(allCachedGames.Where(g => !missingFiles.Contains(Path.Combine(g.basePath, g.Filename))));
        else
          cachedGames = allCachedGames;
      }
    }

    private Game ReadGameFromTable(IVistaDBTable gamesTable)
    {
      var sharedDeckSections = gamesTable.Get("sharedDeckSections");

      return new Game()
      {
        Id = (Guid)gamesTable.Get("id").Value,
        Name = (string)gamesTable.Get("name").Value,
        Version = new Version((string)gamesTable.Get("version").Value),
        Filename = (string)gamesTable.Get("filename").Value,
        CardWidth = (int)gamesTable.Get("cardWidth").Value,
        CardHeight = (int)gamesTable.Get("cardHeight").Value,
        CardBack = (string)gamesTable.Get("cardBack").Value,
        DeckSections = DeserializeList((string)gamesTable.Get("deckSections").Value),
        SharedDeckSections = sharedDeckSections.IsNull ? null : DeserializeList((string)sharedDeckSections.Value),
        basePath = Path.Combine(BasePath, gamesTable.Get("id").Value.ToString()),
        repository = this
      };
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
      string[] sections = System.Text.RegularExpressions.Regex.Split(list, "(?<!,),(?!,)");
      return sections.Select(s => s.Replace(",,", ",")).ToList();
    }

    private bool CheckCompatibility(Game game, IEnumerable<PropertyDef> properties)
    {
      return properties.SequenceEqual(game.CustomProperties);
    }

    public static void UpgradeFrom(Version version)
    {
      // Version 0.7.3 is created anew, no migration.
      if (version < new Version(0, 7, 3)) return;

      if (version < new Version(0, 7, 7))
        // Extend the Pack.xml column size
        ForEachGame((gameDb) =>
        {
          var packSchema = gameDb.TableSchema("Pack");
          packSchema.AlterColumnType("xml", VistaDBType.NText);
          gameDb.AlterTable("Pack", packSchema);
        });

      if (version < new Version(0, 9, 3))
      {
        string masterDbPath = Path.Combine(BasePath, "master.vdb3");
        using (var dda = VistaDBEngine.Connections.OpenDDA())
        using (var masterDb = dda.OpenDatabase(masterDbPath, VistaDBDatabaseOpenMode.ExclusiveReadWrite, null))
        {
          var gameSchema = masterDb.TableSchema("Game");
          gameSchema.AddColumn("sharedDeckSections", VistaDBType.NVarChar, 1000);
          gameSchema.DefineColumnAttributes("sharedDeckSections", true, false, false, false, null, null);
          masterDb.AlterTable("Game", gameSchema);
        }
      }        
    }

    public static void ForEachGame(Action<IVistaDBDatabase> action)
    {
      string masterDbPath = Path.Combine(BasePath, "master.vdb3");
      using (var dda = VistaDBEngine.Connections.OpenDDA())
      using (var masterDb = dda.OpenDatabase(masterDbPath, VistaDBDatabaseOpenMode.NonexclusiveReadOnly, null))
      using (var gamesTable = masterDb.OpenTable("Game", false, true))
      {
        gamesTable.First();
        while (!gamesTable.EndOfTable)
        {
          string gamePath = Path.Combine(BasePath, gamesTable.Get("id").Value.ToString());
          using (var gameDb = dda.OpenDatabase(Path.Combine(gamePath, "game.vdb3"), VistaDBDatabaseOpenMode.ExclusiveReadWrite, null))
            action(gameDb);
          gamesTable.Next();
        }
      }
    }
  }
}
