namespace Octgn.Data
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Db4objects.Db4o;

    public class DbContext : IDisposable
    {
        private static readonly string DatabasePath = Path.Combine(SimpleConfig.DataDirectory, "Database");
        private static readonly string DatabaseFile = Path.Combine(DatabasePath, "master.db4o");

        internal static DbContext Context { get; set; }
        public static DbContext Get()
        {
            return Context ?? (Context = new DbContext());
        }

        internal IObjectServer Server { get; set; }
        internal IObjectContainer Client { get; set; }

        public IEnumerable<Entities.Game> Games
        {
            get
            {
                return Client.Query<Entities.Game>().AsEnumerable();
            }
        }

        internal DbContext()
        {
            // Try host server
            try
            {
                Server = Db4objects.Db4o.CS.Db4oClientServer.OpenServer(DatabaseFile, 57634);
                Server.GrantAccess("user", "password");
                return;
            }
            catch (Exception)
            {
                // either the file is locked, the port is taken, or both
            }
            // Try connect to server
            try
            {
                Client = Db4objects.Db4o.CS.Db4oClientServer.OpenClient("localhost", 57634, "user", "password");
                return;
            }
            catch (Exception)
            {
                // This means that port 57634 is taken, so we are basically fucked at this point.
            }
            throw new Exception("Port 57634 is taken. Can't create or connect to database.");
        }



        public void Dispose()
        {
            if (Server != null)
            {
                try
                {
                    Server.Close();
                    Server.Dispose();
                    Server = null;
                }
                catch
                {
                    // Noone cares
                }
            }
            if (Client != null)
            {
                try
                {
                    Client.Close();
                    Client.Dispose();
                    Client = null;
                }
                catch
                {
                    // Noone cares
                }
            }
        }
    }
}