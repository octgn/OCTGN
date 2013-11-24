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

    using Octgn.Library.Exceptions;
    using Octgn.Library.ExtensionMethods;

    using log4net;

    public class CollectionQuery<T> : IEnumerable<T> where T : class
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal List<T> Objects { get; set; }
        internal Dictionary<Guid,List<DirectoryInfo>> Index { get; set; }
        internal ICollectionDefinition[] Defs { get; set; }
        public Type ElementType { get { return typeof(T); } }
        private readonly object indexLock = new object();
        private readonly object objectLock = new object();

        public CollectionQuery(IEnumerable<ICollectionDefinition> def)
        {
            this.Defs = def.ToArray();
            lock (indexLock)
            {
                this.Index = new Dictionary<Guid,List<DirectoryInfo>>();
                foreach(var d in this.Defs.Where(x=>x.IsSteril == false))
                    this.Index.Add(d.Key,d.CreateSearchIndex());
            }
        }

        /// <summary>
        /// Do not use. Currently blows up the internal enumerator. Will fix at some point...
        /// </summary>
        /// <
        /// <typeparam name="P"></typeparam>
        /// <param name="property"></param>
        /// <param name="op"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public CollectionQuery<T> By<P>(Expression<Func<T, P>> property, Op op, P value)
        {
            //TODO Fix this so it doesn't blow up the enumerator.
            var queryPart = new Part<T>();
            queryPart.Property(property);

            foreach (var def in this.Defs)
            {
                if (def.Parts.Any(x => x.PartString() == queryPart.PartString()) == false)
                    throw new ArgumentException(
                        "There is no property " + queryPart.ToString() + " defined for the collection " + def.Name,
                        "property");
            }

            foreach (var def in this.Defs)
            {
                var partIndex = 0;
                foreach (var part in new DirectoryInfo(def.Path).Split())
                {
                    if (part != queryPart.PartString())
                    {
                        partIndex++;
                        continue;
                    }
                    lock (indexLock)
                    {
                        var dindex = Index[def.Key];
                        var remlist = new List<DirectoryInfo>();
                        foreach (var i in dindex)
                        {
                            var dirParts = i.Split();
                            var partString = dirParts[partIndex];
                            if (!partString.Is(queryPart.Type))
                            {
                                remlist.Add(i);
                                continue;
                            }
                            switch (op)
                            {
                                case Op.Eq:
                                    if (String.Equals(partString,value.ToString(),StringComparison.InvariantCultureIgnoreCase)) continue;
                                    break;
                                case Op.Neq:
                                    if (!String.Equals(partString, value.ToString(), StringComparison.InvariantCultureIgnoreCase)) continue;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException("op");
                            }
                            remlist.Add(i);
                        }
                        foreach (var r in remlist) dindex.Remove(r);
                        Index[def.Key] = dindex;
                    }
                    break;
                }
            }
            return this;
        }

        public IEnumerator<T> GetEnumerator()
        {
            // Both enumerator functions MUST be manually implemented, otherwise we get a deadlock
            GenerateObjects();
            lock(objectLock)
                return Objects.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            // Both enumerator functions MUST be manually implemented, otherwise we get a deadlock
            GenerateObjects();
            lock(objectLock)
                return Objects.GetEnumerator();
        }

        internal void GenerateObjects()
        {
            lock (objectLock)
            {
                if (Objects != null) return;
                Objects = new List<T>();
                lock (indexLock)
                {
                    foreach (var ilist in Index)
                    {
                        var il = ilist;
                        var defs = Defs.Where(x => x.Key == il.Key).ToArray();
                        var def = defs.First();
                        foreach (var i in il.Value)
                        {
                            T obj = null;
                            var path = "";
#if(!DEBUG)
                            try
                            {
#endif
                                path = Path.Combine(
                                    i.FullName, def.Parts.First(x => x.PartType == PartType.File).PartString());
                                if (def.Config.Cache != null) obj = def.Config.Cache.GetObjectFromPath<T>(path);
                                if (obj == null)
                                {
                                    obj = (T)def.Serializer.Deserialize(path);
                                    if (def.Config.Cache != null) def.Config.Cache.AddObjectToCache(path, obj);
                                }
#if(!DEBUG)
                            }
                            catch (UserMessageException e)
                            {
                                throw;
                            }
                            catch (Exception e)
                            {
                                obj = null;
                                Log.Error("Error desterilizing " + path + " for collection " + typeof(T).Name, e);
                            }
#endif
                            if (obj != null) Objects.Add(obj);
                        }
                    }
                }
            }
        }
    }

    public enum Op
    {
        Eq, Neq
    }
}