/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using log4net;
using Microsoft.Win32;

namespace Octgn.Library
{
    public class Config : ISimpleConfig
    {
        public static Config Instance { get; set; }

        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IPaths Paths { get; set; }

        private readonly MemoryCache cache;

        private readonly ReaderWriterLockSlim cacheLocker;

        private readonly ReaderWriterLockSlim locker;

        /// <summary>
        /// Can't call into Octgn.Core.Prefs
        /// Can't call into Octgn.Library.Paths
        /// </summary>
        public Config() {
            string dataDirectory = null;

            using (var subKey = Registry.CurrentUser.OpenSubKey(@"Software\OCTGN")) {
                if (subKey == null) {
                    throw new OctgnNotInstalledException("Octgn is missing from registry");
                }

                dataDirectory = (string)subKey.GetValue(@"DataDirectory");
            }

            if (dataDirectory == null) {
                throw new OctgnNotInstalledException("OCTGN DataDirectory is missing from registry");
            }

            Log.Info($"Found DataDirectory {dataDirectory}");

            if (!Directory.Exists(dataDirectory)) {
                Log.Info($"Creating DataDirectory {dataDirectory}");
                Directory.CreateDirectory(dataDirectory);
            }

            Log.Debug("Constructing");

            if (string.IsNullOrWhiteSpace(dataDirectory))
                throw new ArgumentNullException(nameof(dataDirectory));

            DataDirectory = dataDirectory;

            this.cacheLocker = new ReaderWriterLockSlim();
            this.locker = new ReaderWriterLockSlim();
            // Expected Exception: System.InvalidOperationException
            // Additional information: The requested Performance Counter is not a custom counter, it has to be initialized as ReadOnly.
            cache = new MemoryCache("ConfigCache");

            var p = Path.Combine(DataDirectory, "Config");
            const string f = "settings.json";
            ConfigPath = Path.Combine(p, f);

            if (!Directory.Exists(p)) {
                Log.Debug("Creating Config Directory");
                Directory.CreateDirectory(p);
            }
            Log.Debug("Creating Paths");
            Paths = new Paths(DataDirectory);
            Log.Debug("Created Paths");
        }

        ~Config() {
            if (cache != null) {
                cache.Dispose();
            }
        }

        public string DataDirectory { get; }

        public string ConfigPath { get; }

        public T ReadValue<T>(string valName, T def) {
            T val = def;
            try {
                locker.EnterUpgradeableReadLock();
                if (!GetFromCache(valName, out val)) {
                    try {
                        locker.EnterWriteLock();
                        using (var cf = new ConfigFile(ConfigPath)) {
                            if (!cf.Contains(valName)) {
                                cf.Add(valName, def);
                                cf.Save();
                                val = def;
                            } else {
                                if (!cf.Get(valName, out val)) {
                                    val = def;
                                }
                            }
                        }
                        AddToCache(valName, val);

                    } catch (Exception e) {
                        Log.Error("ReadValue", e);
                    } finally {
                        locker.ExitWriteLock();
                    }
                }

            } catch (Exception e) {
                Log.Error("ReadValue", e);
            } finally {
                locker.ExitUpgradeableReadLock();
            }
            return val;
        }

        public void WriteValue<T>(string valName, T value) {
            try {
                locker.EnterWriteLock();
                using (var cf = new ConfigFile(ConfigPath)) {
                    cf.AddOrSet(valName, value);
                    AddToCache(valName, value);
                    cf.Save();
                }

            } finally {
                locker.ExitWriteLock();
            }
        }

        #region Cache

        internal bool GetFromCache<T>(string name, out T val) {
            try {
                this.cacheLocker.EnterReadLock();
                if (cache.Contains(name)) {
                    if (cache[name] is NullObject)
                        val = default(T);
                    else
                        val = (T)cache[name];
                    return true;
                }
                val = default(T);
                return false;
            } finally {
                this.cacheLocker.ExitReadLock();
            }
        }

        internal void AddToCache<T>(string name, T val) {
            try {
                this.cacheLocker.EnterWriteLock();
                Object addObj;
                if (typeof(T).IsValueType == false && val == null)
                    addObj = new NullObject();
                else
                    addObj = val;
                if (cache.Contains(name))
                    cache.Set(name, addObj, DateTime.Now.AddMinutes(1));
                else
                    cache.Add(name, addObj, DateTime.Now.AddMinutes(1));
            } finally {
                this.cacheLocker.ExitWriteLock();
            }
        }

        internal void SetToCache<T>(string name, T val) {
            try {
                this.cacheLocker.EnterWriteLock();
                cache.Set(name, val, DateTime.Now.AddMinutes(1));
            } finally {
                this.cacheLocker.ExitWriteLock();
            }
        }

        #endregion Cache
    }
    public class OctgnNotInstalledException : Exception
    {
        public OctgnNotInstalledException() { }

        public OctgnNotInstalledException(string message) : base(message) { }
    }

    internal class NullObject
    {

    }
}