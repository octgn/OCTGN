namespace Octgn.DataNew
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Octgn.DataNew.Entities;
    using Octgn.Library;

    using log4net;

    public class DbContext : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string DatabasePath = Path.Combine(SimpleConfig.DataDirectory, "Database");
        private static readonly string DatabaseFile = Path.Combine(DatabasePath, "master.db");

        internal static DbContext Context { get; set; }
        public static DbContext Get()
        {
            return Context ?? (Context = new DbContext());
        }

        public IEnumerable<Game> Games
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<Set> Sets
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<Card> Cards
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void Save(Set set)
        {
            var game = Games.FirstOrDefault(x => x.Id == set.GameId);
            if (game == null) throw new Exception("Game doesn't exist!");
            throw new NotImplementedException();
        }
        public void Save(Game game)
        {
            throw new NotImplementedException();
        }

        public void Remove(Game game)
        {
            var g = Games.FirstOrDefault(x => x.Id == game.Id);
            if (g == null) return;
            throw new NotImplementedException();
        }

        public void Remove(Set set)
        {
            var s = Sets.FirstOrDefault(x => x.Id == set.Id);
            if (s == null) return;
            throw new NotImplementedException();
        }

        internal DbContext()
        {
        }



        public void Dispose()
        {

        }
    }
}