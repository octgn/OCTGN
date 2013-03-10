namespace Octgn.Core.DataManagers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Octgn.DataNew;
    using Octgn.DataNew.Entities;

    using log4net;

    public class GameManager
    {
        #region Singleton
        private static GameManager Context { get; set; }
        private static object locker = new object();
        public static GameManager Get()
        {
            lock (locker) return Context ?? (Context = new GameManager());
        }
        internal GameManager()
        {

        }
        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public int GameCount {
            get
            {
                return DbContext.Get().Games.Count();
            }
        }

        public Game GetById(Guid id)
        {
            return DbContext.Get().Games.FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<Game> Games
        {
            get
            {
                return DbContext.Get().Games;
            }
        }
    }
}