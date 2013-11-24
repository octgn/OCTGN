namespace Octgn.Library
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Caching;
    using System.Threading;

    using log4net;

    public class Config : ISimpleConfig
    {
        #region Singleton

        internal static Config SingletonContext { get; set; }

        private static readonly object ConfigSingletonLocker = new object();

        public static Config Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (ConfigSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new Config();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly MemoryCache cache;

        private readonly ReaderWriterLockSlim cacheLocker;

        private readonly ReaderWriterLockSlim locker;

        private readonly string configPath;

        internal Config()
        {
            this.cacheLocker = new ReaderWriterLockSlim();
            this.locker = new ReaderWriterLockSlim();
            cache = new MemoryCache("ConfigCache");

            var p = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn", "Config");
            const string f = "settings.json";
            configPath = Path.Combine(p, f);

            if (!Directory.Exists(p))
            {
                Directory.CreateDirectory(p);
            }
        }

        ~Config()
        {
            if (cache != null)
            {
                cache.Dispose();
            }
        }

        public string DataDirectory
        {
            get
            {
                string ret;
                if (!this.GetFromCache("datadirectory", out ret))
                {
                    ret = ReadValue("datadirectory", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Octgn"));
                }
                return ret;
            }
            set
            {
                WriteValue("datadirectory", value);
            }
        }

        public string ConfigPath
        {
            get
            {
                return configPath;
            }
        }

        public T ReadValue<T>(string valName, T def)
        {
            T val;
            try
            {
                locker.EnterUpgradeableReadLock();
                if (!GetFromCache(valName, out val))
                {
                    try
                    {
                        locker.EnterWriteLock();
                        using (var cf = new ConfigFile())
                        {
                            if (!cf.Contains(valName))
                            {
                                cf.Add(valName, def);
                                cf.Save();
                                val = def;
                            }
                            else
                            {
                                if (!cf.Get(valName, out val))
                                {
                                    val = def;
                                }
                            }
                        }
                        AddToCache(valName, val);

                    }
                    finally
                    {
						locker.ExitWriteLock();
                    }
                }

            }
            finally
            {
				locker.ExitUpgradeableReadLock();
            } 
            return val;
        }

        public void WriteValue<T>(string valName, T value)
        {
            try
            {
				locker.EnterWriteLock();
                using (var cf = new ConfigFile())
                {
                    cf.AddOrSet(valName, value);
                    AddToCache(valName, value);
                    cf.Save();
                }

            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        #region Cache

        internal bool GetFromCache<T>(string name, out T val)
        {
            try
            {
				this.cacheLocker.EnterReadLock();
                if (cache.Contains(name))
                {
                    if (cache[name] is NullObject)
                        val = default(T);
					else
						val = (T)cache[name];
                    return true;
                }
                val = default(T);
                return false;
            }
            finally
            {
                this.cacheLocker.ExitReadLock();
            }
        }

        internal void AddToCache<T>(string name, T val)
        {
            try
            {
				this.cacheLocker.EnterWriteLock();
                Object addObj;
                if (typeof(T).IsValueType == false && val == null) 
					addObj = new NullObject();
                else
 					addObj = val;
				if(cache.Contains(name))
                    cache.Set(name, addObj, DateTime.Now.AddMinutes(1));
				else
					cache.Add(name, addObj, DateTime.Now.AddMinutes(1));
            }
            finally
            {
                this.cacheLocker.ExitWriteLock();
            }
        }

        internal void SetToCache<T>(string name, T val)
        {
            try
            {
				this.cacheLocker.EnterWriteLock();
				cache.Set(name,val,DateTime.Now.AddMinutes(1));
            }
            finally
            {
                this.cacheLocker.ExitWriteLock();
            }
        }

        #endregion Cache
    }

    internal class NullObject
    {
        
    }
}