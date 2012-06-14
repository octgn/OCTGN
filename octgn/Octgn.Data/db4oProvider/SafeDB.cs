/// Copyright (c) 2008-2011 Brad Williams
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
/// associated documentation files (the "Software"), to deal in the Software without restriction,
/// including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
/// and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
/// subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all copies or substantial
/// portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
/// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
/// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
/// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
/// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using Db4objects.Db4o;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Query;

namespace db4oProviders
{
    /// <summary>
    /// Serializes the lifetimes of IObjectContainer objects per connection string.
    /// </summary>
    public class SafeDB : IObjectContainer
    {
        protected Mutex mutex;
        protected string connectionString;
        protected static Dictionary<string, DateTime> constructionDict = new Dictionary<string, DateTime>();
        protected static Dictionary<string, IObjectContainer> dbDict = new Dictionary<string, IObjectContainer>();
        protected static int instanceCount;

        public static int DropAll()
        {
            int numberDropped = 0;

            if (instanceCount == 0)
            {
                while (dbDict.Keys.Count > 0)
                {
                    numberDropped++;

                    var enumerator = dbDict.Keys.GetEnumerator();

                    if (enumerator.MoveNext())
                    {
                        var key = enumerator.Current;
                        dbDict[key].Dispose();
                        dbDict.Remove(key);
                        constructionDict.Remove(key);
                    }
                }
            }

            return numberDropped;
        }

        public static int OpenConnections
        {
            get { return dbDict.Count; }
        }

        [SecuritySafeCritical]
        public SafeDB(string con)
        {
            connectionString = con;
            mutex = new Mutex(false, GetType() + connectionString.GetHashCode().ToString());
            mutex.WaitOne(5000);

            if (constructionDict.ContainsKey(connectionString))
            {
                if (constructionDict[connectionString].AddMinutes(30.0) < DateTime.Now)
                {
                    dbDict[connectionString].Dispose();
                    dbDict.Remove(connectionString);
                    constructionDict.Remove(connectionString);
                }
            }

            constructionDict[connectionString] = DateTime.Now;

            if (!dbDict.ContainsKey(connectionString))
            {
                var container = Db4oFactory.OpenFile(connectionString);
                dbDict[connectionString] = container;
            }

            instanceCount++;
        }

        #region IObjectContainer Members

        public void Dispose()
        {
            //dbDict[connectionString].Dispose();
            //dbDict.Remove(connectionString);

            mutex.ReleaseMutex();
            mutex = null;

            if (instanceCount > 0)
                instanceCount--;
        }

        #region IObjectContainer delegations

        public void Activate(object obj, int depth)
        {
            dbDict[connectionString].Activate(obj, depth);
        }

        public bool Close()
        {
            return dbDict[connectionString].Close();
        }

        public void Commit()
        {
            dbDict[connectionString].Commit();
        }

        public void Deactivate(object obj, int depth)
        {
            dbDict[connectionString].Deactivate(obj, depth);
        }

        public void Delete(object obj)
        {
            dbDict[connectionString].Delete(obj);
        }

        public IExtObjectContainer Ext()
        {
            return dbDict[connectionString].Ext();
        }

        public IObjectSet QueryByExample(object template)
        {
            return dbDict[connectionString].QueryByExample(template);
        }

        public IQuery Query()
        {
            return dbDict[connectionString].Query();
        }

        public IObjectSet Query(Type clazz)
        {
            return dbDict[connectionString].Query(clazz);
        }

        public IObjectSet Query(Predicate predicate)
        {
            return dbDict[connectionString].Query(predicate);
        }

        public IObjectSet Query(Predicate predicate, IQueryComparator comparator)
        {
            return dbDict[connectionString].Query(predicate, comparator);
        }

        public IObjectSet Query(Predicate predicate, IComparer comparer)
        {
            return dbDict[connectionString].Query(predicate, comparer);
        }

        public void Rollback()
        {
            dbDict[connectionString].Rollback();
        }

        public void Store(object obj)
        {
            dbDict[connectionString].Store(obj);
        }

        public IList<Extent> Query<Extent>(Predicate<Extent> match)
        {
            return dbDict[connectionString].Query(match);
        }

        public IList<Extent> Query<Extent>(Predicate<Extent> match, IComparer<Extent> comparer)
        {
            return dbDict[connectionString].Query(match, comparer);
        }

        public IList<Extent> Query<Extent>(Predicate<Extent> match, Comparison<Extent> comparison)
        {
            return dbDict[connectionString].Query(match, comparison);
        }

        public IList<ElementType> Query<ElementType>(Type extent)
        {
            return dbDict[connectionString].Query<ElementType>(extent);
        }

        public IList<Extent> Query<Extent>()
        {
            return dbDict[connectionString].Query<Extent>();
        }

        public IList<Extent> Query<Extent>(IComparer<Extent> comparer)
        {
            return dbDict[connectionString].Query(comparer);
        }

        #endregion

        #endregion
    }
}
