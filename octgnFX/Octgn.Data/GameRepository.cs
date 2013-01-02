namespace Octgn.Data
{
    using System;
    using System.IO.Abstractions;
    using System.Linq;

    using Octgn.Data.Database;

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

        public IQueryable<Database.Entities.Game> Games
        {
            get
            {
                    return DatabaseSession.GetSession().CreateCriteria<Database.Entities.Game>().Future<Database.Entities.Game>().AsQueryable();
            }
        } 

        public GameRepository()
        {
            if (FS == null)
                FS = new FileSystem();
        }
    }
}
