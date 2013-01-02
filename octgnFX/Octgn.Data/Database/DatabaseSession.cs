namespace Octgn.Data.Database
{
    using System;
    using System.Linq;
    using System.Reflection;

    using FluentNHibernate.Cfg;
    using FluentNHibernate.Cfg.Db;

    using NHibernate;

    public static class DatabaseSession
    {
        internal static ISessionFactory SessionFactory
        {
            get
            {
                if (sessionFactory == null) CreateSessionFactory();
                return sessionFactory;
            }
        }

        private static ISessionFactory sessionFactory;

        public static ISession GetSession()
        {
            return SessionFactory.OpenSession();
        }

        internal static void CreateSessionFactory()
        {
            sessionFactory =
                Fluently.Configure()
                        .Database(SQLiteConfiguration.Standard.UsingFile(GetDatabasePath()))
                        .Mappings(x => x.FluentMappings.AddFromAssemblyOf<IDatabaseConfig>())
                        .BuildSessionFactory();
        }

        internal static string GetDatabasePath()
        {
            var ass = AppDomain.CurrentDomain.GetAssemblies();
            var assTypes = ass.SelectMany(x => x.GetTypes()).OrderBy(x=>x.Name).ToArray();
            var dbConfigType =
                (from t in assTypes
                 where t.GetInterfaces().Any(x => x == typeof(IDatabaseConfig))
                 select t).FirstOrDefault();
            if (dbConfigType == null)
                throw new Exception("Database config class missing.");

            return ((IDatabaseConfig)Activator.CreateInstance(dbConfigType)).DataPath;
        }
    }
}
