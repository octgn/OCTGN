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
    using Octgn.ProxyGenerator;

    using log4net;

    public class DbContext : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
                return SetQuery;
            }
        }

        public CollectionQuery<Set> SetQuery
        {
            get
            {
                return Db.Query<Set>();
            }
        }

        public IEnumerable<Set> SetsById(Guid setId)
        {
            return Db.Query<Set>().By(x => x.Id, Op.Eq, setId);
        }

        public Game GameById(Guid gameId)
        {
            return Db.Query<Game>().By(x => x.Id, Op.Eq, gameId).FirstOrDefault();
        }

        public IEnumerable<Card> Cards
        {
            get
            {
                return Sets.SelectMany(x => x.Cards);
            }
        }

        public IEnumerable<GameScript> Scripts
        {
            get
            {
                return Db.Query<GameScript>();
            }
        }

        public IEnumerable<ProxyDefinition> ProxyDefinitions
        {
            get
            {
                return Db.Query<ProxyDefinition>();
            }
        }

        internal FileDb Db { get; set; }

        internal DbContext()
        {
            var config = new FileDbConfiguration()
                .SetDirectory(Paths.Get().DataDirectory)
                .DefineCollection<Game>("GameDatabase")
                .SetPart(x => x.Property(y => y.Id))
                .SetPart(x => x.File("definition.xml"))
                .SetSerializer<GameSerializer>()
                .Conf()
                .DefineCollection<Set>("Sets")
                .OverrideRoot(x => x.Directory("GameDatabase"))
                .SetPart(x => x.Property(y => y.GameId))
                .SetPart(x => x.Directory("Sets"))
                .SetPart(x => x.Property(y => y.Id))
                .SetPart(x => x.File("set.xml"))
                .SetSerializer<SetSerializer>()
                .Conf()
                .DefineCollection<GameScript>("Scripts")
                .SetSteril()
                //.OverrideRoot(x => x.Directory("GameDatabase"))
                //.SetPart(x => x.Property(y => y.GameId))
                .Conf()
                .DefineCollection<ProxyDefinition>("Proxies")
                .SetSteril()
                //.OverrideRoot(x => x.Directory("GameDatabase"))
                //.SetPart(x => x.Property(y=>y.Key))
                .Conf()
                .SetCacheProvider<FullCacheProvider>();
            Db = new FileDb(config);
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

        public void Invalidate(Game game)
        {
            var g = Games.FirstOrDefault(x => x.Id == game.Id);
            if (g == null) return;
            foreach (var s in Sets.Where(x => x.GameId == g.Id).ToArray())
            {
                Db.Config.Cache.InvalidateObject(s);
            }
            foreach (var p in ProxyDefinitions.Where(x => (Guid)x.Key == g.Id).ToArray())
            {
                Db.Config.Cache.InvalidateObject(p);
            }
            foreach (var s in Scripts.Where(x => x.GameId == g.Id).ToArray())
            {
                Db.Config.Cache.InvalidateObject(s);
            }
            Db.Config.Cache.InvalidateObject(g);
        }

        public void Invalidate(Set set)
        {
            var s = Sets.FirstOrDefault(x => x.Id == set.Id);
            if (s == null) return;
            Db.Config.Cache.InvalidateObject(s);
        }

        public void Invalidate(ProxyDefinition proxy)
        {
            foreach(var p in ProxyDefinitions.Where(x => x.Key == proxy.Key))
            {
                Db.Config.Cache.InvalidateObject(p);
            }
        }
        
        public void Invalidate(GameScript script)
        {
            var s = Scripts.FirstOrDefault(x => x.Path == script.Path);
            if (s == null) return;
            Db.Config.Cache.InvalidateObject(s);
        }

        public void Dispose()
        {

        }
    }
}