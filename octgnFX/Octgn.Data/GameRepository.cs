namespace Octgn.Data
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;

    /// <summary>
    /// Provides access to all the game, set, cards etc.
    /// </summary>
    public class GameRepository : IGameRepository
    {
        #region Static
        /// <summary>
        /// Get an instance of IGameRepository
        /// </summary>
        /// <returns>IGameRepository</returns>
        public static IGameRepository GetRepo()
        {
            return new GameRepository();
        }
        #endregion

        internal IFileSystem FS { get; set; }

        public IQueryable<Game> Games
        {
            get
            {
                return null;
            }
        } 

        public GameRepository()
        {
            //if(FS == null)
            //    FS = new FileSystem();
            //if (database != null)
            //    return;
            //var dbConfigType =
            //    (from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x=>x.GetTypes())
            //     where t.GetInterfaces().Any(x=>x == typeof(IDatabaseConfig))
            //     select t).FirstOrDefault();
            //if(dbConfigType == null)
            //    throw new Exception("Database config class missing.");

            //var dbConfig = (IDatabaseConfig)Activator.CreateInstance(dbConfigType);

            //database = new EmbeddableDocumentStore()
            //               {
            //                   DataDirectory = dbConfig.DataPath, 
            //                   UseEmbeddedHttpServer = true
            //               };
            //NonAdminHttp.EnsureCanListenToWhenInNonAdminContext(database.Configuration.Port);
            //if (!FS.Directory.Exists(dbConfig.DataPath)) FS.Directory.CreateDirectory(dbConfig.DataPath);
            //database.Initialize();

            //DocumentStore s = new DocumentStore() { Url = "http://localhost:" + database.Configuration.Port.ToString() };
            //s.Initialize();

        }
    }
}
