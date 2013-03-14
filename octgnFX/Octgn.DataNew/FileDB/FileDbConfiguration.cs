namespace Octgn.DataNew.FileDB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class FileDbConfiguration
    {
        internal string Directory { get; set; }
        internal List<ICollectionDefinition> Configurations { get; set; }

        public FileDbConfiguration()
        {
            Configurations = new List<ICollectionDefinition>();
        }

        public FileDbConfiguration SetDirectory(string directory)
        {
            Directory = directory;
            return this;
        }

        public CollectionDefinition<T> DefineCollection<T>(string name)
        {
            var coll = new CollectionDefinition<T>(this,name);
            Configurations.Add(coll);
            return coll;
        }

        public CollectionQuery<T> Query<T>() where T: class

        {
            var config = Configurations.FirstOrDefault(x => x.Type == typeof(T));
            if(config == null)
                throw new ArgumentException("can't find definition for type " + typeof(T).Name,"T");

            return new CollectionQuery<T>(config);
        }
    }
}