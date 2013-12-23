namespace Octide
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using GalaSoft.MvvmLight.Messaging;

    using Octgn.DataNew.Entities;

    using Octide.ViewModel;

    public class AssetManager
    {
        #region Singleton

        internal static AssetManager SingletonContext { get; set; }

        private static readonly object AssetManagerSingletonLocker = new object();

        public static AssetManager Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (AssetManagerSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new AssetManager();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        internal FileSystemWatcher Watcher;

        internal AssetManager()
        {
            Watcher = new FileSystemWatcher();
            Watcher.IncludeSubdirectories = true;
            Messenger.Default.Register<PropertyChangedMessage<Game>>(this,
                x =>
                {
                    if (ViewModelLocator.GameLoader.ValidGame)
                    {
                        Watcher.Path = ViewModelLocator.GameLoader.GamePath;
                        Watcher.EnableRaisingEvents = true;
                    }
                    else
                    {
                        Watcher.EnableRaisingEvents = false;
                    }
                });
            Watcher.Changed += WatcherOnChanged;
        }

        ~AssetManager()
        {
            Watcher.Changed -= WatcherOnChanged;
        }

        public List<Asset> Assets
        {
            get
            {
                if (!ViewModelLocator.GameLoader.ValidGame) return new List<Asset>();
                var di = new DirectoryInfo(ViewModelLocator.GameLoader.GamePath);
                var files = di.GetFiles("*.*", SearchOption.AllDirectories);
                var ret = files.Select(Asset.Load);
                return ret.ToList();
            }
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
    }

    public class AssetManagerUpdatedMessage
    {
    }

    public sealed class Asset : IEqualityComparer<Asset>, IEquatable<Asset>
    {
        public string FullPath { get; private set; }
        public string RelativePath { get; private set; }
        public string Folder { get; private set; }
        public string FileName { get; private set; }
        public string Extension { get; private set; }
        public AssetType Type { get; private set; }

        public Asset()
        {
        }

        private static readonly Dictionary<string, Asset> AssetCache = new Dictionary<string, Asset>(StringComparer.InvariantCultureIgnoreCase);

        public static Asset Load(FileInfo file)
        {
            lock (AssetCache)
            {
                if (AssetCache.ContainsKey(file.FullName))
                {
                    return AssetCache[file.FullName];
                }

                var a = new Asset();
                a.FullPath = file.FullName;
                a.Folder = file.Directory.FullName;
                a.FileName = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
                a.Extension = file.Extension.Substring(1);
                a.RelativePath = Utils.MakeRelativePath(ViewModelLocator.GameLoader.GamePath, file.FullName);

                switch (a.Extension.ToLower())
                {
                    case "jpg":
                    case "jpeg":
                    case "bmp":
                    case "png":
                    case "gif":
                    case "tiff":
                        a.Type = AssetType.Image;
                        break;
                    case "py":
                        a.Type = AssetType.PythonScript;
                        break;
                    case "xml":
                        a.Type = AssetType.Xml;
                        break;
                    case "mp3":
                    case "oog":
                    case "wav":
                        a.Type = AssetType.Sound;
                        break;
                    default:
                        a.Type = AssetType.Other;
                        break;
                }
                AssetCache.Add(file.FullName, a);
                return a;
            }
        }

        public static Asset Load(string file)
        {
            return Load(new FileInfo(Path.GetFullPath(file)));
        }

        public bool Equals(Asset other)
        {
            if (other == null) return false;
            var ret = FullPath.Equals(other.FullPath,StringComparison.InvariantCultureIgnoreCase);
            return ret;
        }

        public override string ToString()
        {
            return RelativePath;
        }

        public bool Equals(Asset x, Asset y)
        {
            if (x == null && y != null) return false;
            if (x != null && y == null) return false;
            return String.Equals(x.FullPath, y.FullPath, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(Asset obj)
        {
            if (obj == null) return 0;
            return obj.FullPath.GetHashCode();
        }
    }

    public enum AssetType
    {
        Image, PythonScript, Xml, Sound, Other
    }
}