namespace Octgn.DataNew
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Db4objects.Db4o;

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

        internal DbContext()
        {
            Log.Debug("Creating DB Context");
            // Try host server
            try
            {
                Log.Debug("Starting DB Server");
                this.Server = Db4objects.Db4o.CS.Db4oClientServer.OpenServer(DatabaseFile, 57634);
                this.Server.GrantAccess("user", "password");
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
                this.Client = Db4objects.Db4o.CS.Db4oClientServer.OpenClient("localhost", 57634, "user", "password");
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