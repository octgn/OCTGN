namespace Octgn.DataNew.FileDB
{
    using System;
    using System.Linq;

    public class FileDb
    {
        internal FileDbConfiguration Config { get; set; }
        public FileDb(FileDbConfiguration config)
        {
            Config = config;
        }

        public CollectionQuery<T> Query<T>() where T : class
        {
            var config = Config.Configurations.FirstOrDefault(x => x.Type == typeof(T));
            if (config == null)
                throw new ArgumentException("can't find definition for type " + typeof(T).Name, "T");

            return new CollectionQuery<T>(config);
        }
    }
}