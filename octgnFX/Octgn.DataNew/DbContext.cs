namespace Octgn.DataNew
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Octgn.DataNew.Entities;
    using Octgn.DataNew.FileDB;
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
                return Db.Query<Game>();
            }
        }

        public IEnumerable<Set> Sets
        {
            get
            {
                return Db.Query<Set>();
            }
        }

        public IEnumerable<Card> Cards
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        internal FileDbConfiguration Db { get; set; }

        internal DbContext()
        {
            Db = new FileDbConfiguration()
                .SetDirectory(Paths.DataDirectory)
                .DefineCollection<Game>("Games")
                .SetPart(x => x.Property(y => y.Id))
                .SetPart(x => x.File("definition.xml"))
                .SetSerializer<GameSerializer>()
                .Conf()
                .DefineCollection<Set>("Sets")
                .OverrideRoot(x => x.Directory("Games"))
                .SetPart(x => x.Property(y => y.GameId))
                .SetPart(x => x.Directory("Sets"))
                .SetPart(x => x.Property(y => y.Id))
                .SetPart(x => x.File("set.xml"))
                .SetSerializer<SetSerializer>()
                .Conf();
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



        public void Dispose()
        {

        }
    }
}