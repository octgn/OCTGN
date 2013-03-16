namespace Octgn.DataNew.FileDB
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    using Octgn.Library.ExtensionMethods;

    using log4net;

    public class CollectionQuery<T> : IEnumerable<T> where T : class
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal List<DirectoryInfo> Index { get; set; }
        internal List<T> Objects { get; set; }
        internal ICollectionDefinition Def { get; set; }
        public Type ElementType { get { return typeof(T); } }
        private readonly object indexLock = new object();

        public CollectionQuery(ICollectionDefinition def)
        {
            this.Def = def;
            lock (indexLock)
            {
                this.Index = Def.CreateSearchIndex().ToList();
            }
        }

        public CollectionQuery<T> By<P>(Expression<Func<T, P>> property, Op op, P value)
        {
            var queryPart = new Part<T>();
            queryPart.Property(property);

            if (Def.Parts.Any(x => x.PartString() == queryPart.PartString()) == false)
                throw new ArgumentException("There is no property " + queryPart.ToString() + " defined for the collection " + Def.Name, "property");

            var partIndex = 0;
            foreach (var part in new DirectoryInfo(Def.Path).Split())
            {
                if (part != queryPart.PartString())
                {
                    partIndex++;
                    continue;
                }
                lock (indexLock)
                {
                    foreach (var i in Index.ToArray())
                    {
                        var dirParts = i.Split();
                        var partString = dirParts[partIndex];
                        if (!partString.Is(queryPart.Type))
                        {
                            Index.Remove(i);
                            continue;
                        }
                        switch (op)
                        {
                            case Op.Eq:
                                if (partString == value.ToString()) continue;
                                break;
                            case Op.Neq:
                                if (partString != value.ToString()) continue;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("op");
                        }
                        Index.Remove(i);
                    }
                }
                break;
            }
            return this;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (Objects == null) GenerateObjects();
            return Objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal void GenerateObjects()
        {
            Objects = new List<T>();
            lock (indexLock)
            {
                foreach (var i in Index)
                {
                    T obj = null;
                    var path = "";
                    try
                    {
                        path = Path.Combine(i.FullName, Def.Parts.First(x => x.PartType == PartType.File).PartString());
                        if (Def.Config.Cache != null) obj = Def.Config.Cache.GetObjectFromPath<T>(path);
                        if (obj == null) obj = (T)Def.Serializer.Deserialize(path);
                        if (Def.Config.Cache != null) Def.Config.Cache.AddObjectToCache(path, obj);
                    }
                    catch (Exception e)
                    {
                        obj = null;
                        Log.Error("Error desterilizing " + path, e);
                    }
                    if (obj != null) Objects.Add(obj);
                }
            }
        }
    }

    public enum Op
    {
        Eq, Neq
    }
}