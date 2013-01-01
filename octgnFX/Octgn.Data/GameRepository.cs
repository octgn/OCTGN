namespace Octgn.Data
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Reflection;

    using Raven.Client;
    using Raven.Client.Embedded;

    /// <summary>
    /// Provides access to all the game, set, cards etc.
    /// </summary>
    public class GameRepository : IGameRepository
    {
        #region Static
        /// <summary>
        /// Database
        /// </summary>
        private static EmbeddableDocumentStore database;

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
                return this.GetSession().Query<Game>();
            }
        } 

        public GameRepository()
        {
            if(FS == null)
                FS = new FileSystem();
            if (database != null)
                return;
            var dbConfigType =
                (from t in AppDomain.CurrentDomain.GetAssemblies().SelectMany(x=>x.GetTypes())
                 where t.GetInterfaces().Any(x=>x == typeof(IDatabaseConfig))
                 select t).FirstOrDefault();
            if(dbConfigType == null)
                throw new Exception("Database config class missing.");

            var dbConfig = (IDatabaseConfig)Activator.CreateInstance(dbConfigType);

            database = new EmbeddableDocumentStore() { DataDirectory = dbConfig.DataPath, UseEmbeddedHttpServer = false };
            if (!FS.Directory.Exists(dbConfig.DataPath)) FS.Directory.CreateDirectory(dbConfig.DataPath);
            database.Initialize();
        }

        private IDocumentSession GetSession()
        {
            return database.OpenSession();
        }


    }
}
