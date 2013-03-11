namespace Octgn.DataNew
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Db4objects.Db4o;
    using Db4objects.Db4o.CS;
    using Db4objects.Db4o.Config;
    using Db4objects.Db4o.TA;

    using Octgn.DataNew.Entities;
    using Octgn.Library;

    using log4net;

    public class DbContext : IDisposable
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string DatabasePath = Path.Combine(SimpleConfig.DataDirectory, "Database");
        private static readonly string DatabaseFile = Path.Combine(DatabasePath, "master.db4o");

        internal static DbContext Context { get; set; }
        public static DbContext Get()
        {
            return Context ?? (Context = new DbContext());
        }

        internal IObjectServer Server { get; set; }
        internal IObjectContainer Client { get; set; }

        public IEnumerable<Game> Games
        {
            get
            {
                return this.Client.Query<Game>().AsEnumerable();
            }
        }

        public IEnumerable<Set> Sets
        {
            get
            {
                return this.Client.Query<Set>().AsEnumerable();
            }
        }

        public IEnumerable<Card> Cards
        {
            get
            {
                return this.Client.Query<Set>().SelectMany(x=>x.Cards.Cards);
            }
        }

        public void Save(Set set)
        {
            var game = Games.FirstOrDefault(x => x.Id == set.GameId);
            if(game == null) throw new Exception("Game doesn't exist!");
            if (this.Client.Query<Set>().Select(x=>x.Id).Any(x => x == set.Id))
            {
                var id = this.Client.Ext().GetID(Sets.FirstOrDefault(x => x.Id == set.Id));
                this.Client.Ext().Bind(set,id);
            }
            this.Client.Store(set);
            this.Client.Commit();
        }
        public void Save(Game game)
        {
            try
            {
                // Need to do this so that it doesn't just duplicate the entry
                // Stupid db40 uses longs for ids
                var curGame = Games.FirstOrDefault(x => x.Id == game.Id);
                if (curGame != null)
                {
                    var id = this.Client.Ext().GetID(curGame);
                    this.Client.Ext().Bind(game,id);
                }
                this.Client.Store(game);
                this.Client.Commit();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Remove(Game game)
        {
            var g = Games.FirstOrDefault(x => x.Id == game.Id);
            if (g == null) return;
            this.Client.Delete(g);
            this.Client.Commit();
        }

        public void Remove(Set set)
        {
            var s = Sets.FirstOrDefault(x => x.Id == set.Id);
            if (s == null) return;
            this.Client.Delete(s);
            this.Client.Commit();
        }

        public void Commit()
        {
            this.Client.Commit();
        }

        internal DbContext()
        {
            Log.Debug("Creating DB Context");
            //Db4oFactory.Configure().ObjectClass(typeof(Set)).ObjectField("id");
            //Db4oFactory.Configure().ObjectClass(typeof(Set)).Indexed(true);
            var conf = Db4oClientServer.NewServerConfiguration();
            conf.Common.Add(new TransparentPersistenceSupport());
            //conf.Common.ObjectClass(typeof(Set)).ObjectField("id").Indexed(true);

            var cconf = Db4oClientServer.NewClientConfiguration();
            cconf.Common.Add(new TransparentPersistenceSupport());
            //cconf.Common.ObjectClass(typeof(Set)).ObjectField("id").Indexed(true);
            // Try host server
            try
            {
                Log.Debug("Starting DB Server");

                this.Server = Db4objects.Db4o.CS.Db4oClientServer.OpenServer(conf, DatabaseFile, 57634);
                this.Server.GrantAccess("user", "password");
                this.Client = Db4objects.Db4o.CS.Db4oClientServer.OpenClient(cconf,"localhost", 57634, "user", "password");
                return;
            }
            catch (Exception)
            {
                Log.Debug("Couldn't start server");
                // either the file is locked, the port is taken, or both
            }
            // Try connect to server
            try
            {
                Log.Debug("Connecting to server");
                this.Client = Db4objects.Db4o.CS.Db4oClientServer.OpenClient(cconf,"localhost", 57634, "user", "password");
                return;
            }
            catch (Exception)
            {
                Log.Debug("Couldn't connect to server");
                // This means that port 57634 is taken, so we are basically fucked at this point.
            }
            throw new Exception("Port 57634 is taken. Can't create or connect to database.");
        }



        public void Dispose()
        {
            if (this.Server != null)
            {
                try
                {
                    this.Server.Close();
                    this.Server.Dispose();
                    this.Server = null;
                }
                catch
                {
                    // Noone cares
                }
            }
            if (this.Client != null)
            {
                try
                {
                    this.Client.Close();
                    this.Client.Dispose();
                    this.Client = null;
                }
                catch
                {
                    // Noone cares
                }
            }
        }
    }
}