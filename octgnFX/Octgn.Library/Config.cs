/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.Caching;
using System.Threading;
using log4net;
using Octgn.Library.Providers;

namespace Octgn.Library
{
    public class Config : ISimpleConfig, IDisposable
    {
        public static Config Instance { get; set; }

        public static readonly object Sync = new object();

        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public IPaths Paths { get; }

        private readonly MemoryCache _cache;

        private readonly ReaderWriterLockSlim _cacheLocker;

        private readonly ReaderWriterLockSlim _locker;

        private bool _isConstructing = true;

        /// <summary>
        /// Can't call into Octgn.Core.Prefs
        /// Can't call into Octgn.Library.Paths
        /// </summary>
        public Config() {
            try {
                _isConstructing = true;

                var ass = typeof(Config).Assembly;

                var assFile = new FileInfo(ass.Location);

                WorkingDirectory = assFile.Directory.FullName;

                _cacheLocker = new ReaderWriterLockSlim();
                _locker = new ReaderWriterLockSlim();

                var dataDirectoryProvider = new DataDirectoryProvider();

                var dataDirectory = dataDirectoryProvider.Get();

                var correctedPath = ResolvePath(dataDirectory);

                Paths = new Paths(correctedPath, WorkingDirectory);

                DataDirectory = dataDirectory;

                // Expected Exception: System.InvalidOperationException
                // Additional information: The requested Performance Counter is not a custom counter, it has to be initialized as ReadOnly.
                _cache = new MemoryCache("ConfigCache");

                Log.Info("Config Path: " + ConfigPath);

                Log.Debug("Created Paths");
            } catch {
                _cacheLocker?.Dispose();
                _locker?.Dispose();
                _cache?.Dispose();

                throw;
            } finally {
                _isConstructing = false;
            }
        }

        public string DataDirectory {
            get {
                try {
                    _locker.EnterReadLock();

                    return _dataDirectory;
                } finally {
                    _locker.ExitReadLock();
                }
            }
            set {
                try {
                    _locker.EnterWriteLock();

                    var originalDataDirectory = _dataDirectory;

                    if (originalDataDirectory == value) return;

                    ValidatePath(value);

                    new DataDirectoryProvider().Set(value);

                    if (!_isConstructing) {
                        var originalConfigPath = CreateConfigPath(originalDataDirectory);
                        var newConfigPath = CreateConfigPath(value);

                        File.Copy(originalConfigPath, newConfigPath, true);
                    }

                    _dataDirectory = value;
                } finally {
                    _locker.ExitWriteLock();
                }

                if (!_isConstructing) {
                    DataDirectoryChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataDirectory)));
                }
            }
        }

        private string _dataDirectory;

        public event EventHandler<PropertyChangedEventArgs> DataDirectoryChanged;

        public string DataDirectoryFull => ResolvePath(DataDirectory);

        public string WorkingDirectory { get; }

        public string ImageDirectory {
            get { return ReadValue(nameof(ImageDirectory), "%OCTGN_DATA%\\ImageDatabase"); }
            set { WriteValue(nameof(ImageDirectory), value); }
        }

        public string ImageDirectoryFull => ResolvePath(ImageDirectory);

        public string ResolvePath(string path) {
            path = Environment.ExpandEnvironmentVariables(path);

            if (!Path.IsPathRooted(path)) {
                path = Path.Combine(WorkingDirectory, path);
            }

            return path;
        }

        public void ValidatePath(string path) {
            var dir = ResolvePath(path);

            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
        }

        public string ConfigPath => CreateConfigPath(DataDirectoryFull);

        private static string CreateConfigPath(string dataDirectory) {
            var configDirectory = Path.Combine(dataDirectory, "Config");

            if (!Directory.Exists(configDirectory)) {
                Log.Debug("Creating Config Directory");

                Directory.CreateDirectory(configDirectory);
            }

            const string configFileName = "settings.json";

            return Path.Combine(configDirectory, configFileName);
        }

        public T ReadValue<T>(string valName, T def) {
            T val = def;
            try {
                _locker.EnterUpgradeableReadLock();
                if (!GetFromCache(valName, out val)) {
                    try {
                        var configPath = ConfigPath;

                        _locker.EnterWriteLock();
                        using (var cf = new ConfigFile(configPath)) {
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
                        _locker.ExitWriteLock();
                    }
                }
            } catch (Exception e) {
                Log.Error("ReadValue", e);
            } finally {
                _locker.ExitUpgradeableReadLock();
            }
            return val;
        }

        public void WriteValue<T>(string valName, T value) {
            try {
                var configPath = ConfigPath;

                _locker.EnterWriteLock();
                using (var cf = new ConfigFile(configPath)) {
                    cf.AddOrSet(valName, value);
                    AddToCache(valName, value);
                    cf.Save();
                }
            } finally {
                _locker.ExitWriteLock();
            }
        }

        #region Cache

        internal bool GetFromCache<T>(string name, out T val) {
            try {
                _cacheLocker.EnterReadLock();
                if (_cache.Contains(name)) {
                    if (_cache[name] is NullObject)
                        val = default(T);
                    else
                        val = (T)_cache[name];
                    return true;
                }
                val = default(T);
                return false;
            } finally {
                _cacheLocker.ExitReadLock();
            }
        }

        internal void AddToCache<T>(string name, T val) {
            try {
                _cacheLocker.EnterWriteLock();
                Object addObj;
                if (typeof(T).IsValueType == false && val == null)
                    addObj = new NullObject();
                else
                    addObj = val;
                if (_cache.Contains(name))
                    _cache.Set(name, addObj, DateTime.Now.AddMinutes(1));
                else
                    _cache.Add(name, addObj, DateTime.Now.AddMinutes(1));
            } finally {
                _cacheLocker.ExitWriteLock();
            }
        }

        internal void SetToCache<T>(string name, T val) {
            try {
                _cacheLocker.EnterWriteLock();
                _cache.Set(name, val, DateTime.Now.AddMinutes(1));
            } finally {
                _cacheLocker.ExitWriteLock();
            }
        }

        #region IDisposable Support
        private bool _isDisposed; // To detect redundant calls

        protected void VerifyNotDisposed() {
            if (_isDisposed) throw new ObjectDisposedException(ToString());
        }

        protected virtual void Dispose(bool disposing) {
            if (!_isDisposed) {
                if (disposing) {
                    _cacheLocker?.Dispose();
                    _locker?.Dispose();
                    _cache?.Dispose();
                }

                _isDisposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion

        #endregion Cache
    }

    internal class NullObject
    {

    }
}