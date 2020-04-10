// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octide
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    using GalaSoft.MvvmLight.Messaging;

    using Microsoft.Win32;
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
            Watcher = new FileSystemWatcher
            {
                IncludeSubdirectories = true
            };
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

        private ObservableCollection<Asset> _assets;

        public ObservableCollection<Asset> Assets
        {
            get
            {
                if (_assets == null)
                {
                    AssetManager.Instance.CollectAssets();
                }
                return _assets;
            }
            set
            {
                _assets = value;
            }
        }

        public Asset LoadAsset(AssetType validAssetType)
        {

            var fo = new OpenFileDialog
            {
                Filter = Asset.GetAssetFilters(validAssetType)
            };
            if ((bool)fo.ShowDialog() == false)
            {
                return null;
            }
            return LoadAsset(validAssetType, new FileInfo(fo.FileName));
        }
       
        public Asset LoadAsset(AssetType validAssetType, FileInfo file)
        {
            if (Asset.GetAssetType(file) != validAssetType) return null;
            var assetPath = Path.Combine(ViewModelLocator.GameLoader.GamePath, "Assets");
            if (!Directory.Exists(assetPath))
                Directory.CreateDirectory(assetPath);

            var fileCopy = file.CopyTo(Utils.GetUniqueFilename(Path.Combine(assetPath, file.Name)));
            var asset = Asset.Load(fileCopy);
            Assets.Add(asset);
            return asset;
        }

        public void CollectAssets()
        {
            if (!ViewModelLocator.GameLoader.ValidGame) return;
            var di = new DirectoryInfo(ViewModelLocator.GameLoader.GamePath);
            var files = di.GetFiles("*.*", SearchOption.AllDirectories);
            var ret = files.Select(Asset.Load);
            Assets = new ObservableCollection<Asset>(ret);
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
    }

    public class AssetManagerUpdatedMessage
    {
    }

    public partial class Asset : IEqualityComparer<Asset>, IEquatable<Asset>
    {
        public FileInfo File { get; private set; }
        public AssetType Type { get; private set; }

        public string FullPath => File?.FullName;
        public string Folder => File?.Directory.FullName;
        public string FileName => File?.Name.Substring(0, File.Name.Length - File.Extension.Length);
        public string Extension => File?.Extension.Substring(1);
        public string RelativePath => File == null ? null : Utils.MakeRelativePath(ViewModelLocator.GameLoader.GamePath, File.FullName);

        public Asset()
        {
        }

        private static readonly Dictionary<string, Asset> AssetCache = new Dictionary<string, Asset>(StringComparer.InvariantCultureIgnoreCase);

        public static AssetType GetAssetType(FileInfo file)
        {
            switch (file.Extension.Substring(1).ToLower())
            {
                case "jpg":
                case "jpeg":
                case "bmp":
                case "png":
                case "gif":
                case "tiff":
                    return AssetType.Image;
                case "py":
                    return AssetType.PythonScript;
                case "xml":
                    return AssetType.Xml;
                case "ttf":
                    return AssetType.Font;
                case "o8d":
                    return AssetType.Deck;
                case "mp3":
                case "oog":
                case "wav":
                    return AssetType.Sound;
                case "txt":
                case "html":
                case "pdf":
                    return AssetType.Document;
                default:
                    return AssetType.Other;
            }
        }

        public static string GetAssetFilters(AssetType assetType)
        {
            switch (assetType)
            {
                case AssetType.Image:
                    return "Image Files (*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG";
                case AssetType.PythonScript:
                    return "Python files (*.PY)|*.PY";
                case AssetType.Xml:
                    return "Xml files (*.XML)|*.XML";
                case AssetType.Font:
                    return "Font files (*.TTF)|*.TTF";
                case AssetType.Deck:
                    return "OCTGN Deck files (*.O8D)|*.O8D";
                case AssetType.Document:
                    return "Document files (*.HTML;*.PDF;*.TXT)|*.HTML;*.PDF;*.TXT";
                case AssetType.Sound:
                    return "Sound files (*.MP3;*.WAV;*.OGG)|*.MP3;*.WAV;*.OGG";
                default:
                    return "Any files|*.*";
            }
        }

        public static Asset Load(FileInfo file)
        {
            lock (AssetCache)
            {
                if (AssetCache.ContainsKey(file.FullName))
                {
                    return AssetCache[file.FullName];
                }

                var a = new Asset
                {
                    File = file
                };

                a.Type = GetAssetType(file);
                
                AssetCache.Add(file.FullName, a);
                return a;
            }
        }

        public static Asset Load(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var file = new FileInfo(Path.GetFullPath(path));
            if (file == null || path == ViewModelLocator.GameLoader.GamePath)
                return null;
            return Load(file);
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
        PythonScript,
        Image,
        Xml,
        Deck,
        Font,
        Sound,
        Document,
        Other
    }
}