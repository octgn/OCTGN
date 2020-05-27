// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Win32;
using Octgn.DataNew.Entities;
using Octide.ItemModel;

namespace Octide.ViewModel
{
    public class AssetsTabViewModel : ViewModelBase
    {
        public ObservableCollection<Asset> Assets { get; set; }
        private bool _filterUnused;
        private AssetType _filterType;
        public AssetsTabViewModel()
        {
            if (ViewModelLocator.GameLoader.Directory != null)
            {
                Assets = new ObservableCollection<Asset>();
                var di = new DirectoryInfo(ViewModelLocator.GameLoader.Directory);
                var files = di.GetFiles("*.*", SearchOption.AllDirectories);
                Assets = new ObservableCollection<Asset>(files.Select(x => LoadAsset(x)));
                AssetView = new ListCollectionView(Assets);
                Assets.CollectionChanged += (a, b) => AssetView.Refresh();

                Watcher = new FileSystemWatcher
                {
                    IncludeSubdirectories = true
                };
                Watcher.Path = ViewModelLocator.GameLoader.Directory;
                Watcher.EnableRaisingEvents = true;
                Watcher.Changed += FileChanged;
                Watcher.Created += FileCreated;
                Watcher.Renamed += FileRenamed;
                Watcher.Deleted += FileDeleted;
            }
            else
            {
                Watcher.EnableRaisingEvents = false;
            }
        }


        public AssetType FilterType
        {
            get
            {
                return _filterType;
            }
            set
            {
                if (_filterType == value) return;
                _filterType = value;
                UpdateFilter();
                RaisePropertyChanged("FilterType");
            }
        }

        public bool FilterUnused
        {
            get
            {
                return _filterUnused;
            }
            set
            {
                if (_filterUnused == value) return;
                _filterUnused = value;
                UpdateFilter();
                RaisePropertyChanged("FilterUnused");
            }
        }
        private void UpdateFilter()
        {
            AssetView.Filter = obj =>
            {
                var asset = obj as Asset;
                if (FilterType != AssetType.All && asset.Type != FilterType)
                    return false;
                if (FilterUnused && asset.IsLinked)
                    return false;
                return true;
            };
            RaisePropertyChanged("AssetView");
        }

        public Asset NewAsset(AssetType validAssetType, FileInfo file)
        {
            if (GetAssetType(file) != validAssetType) return null;
            var assetPath = Path.Combine(ViewModelLocator.GameLoader.Directory, "Assets");
            if (!Directory.Exists(assetPath))
                Directory.CreateDirectory(assetPath);

            var fileCopy = file.CopyTo(Utils.GetUniqueFilename(Path.Combine(assetPath, file.Name)));
            var asset = LoadAsset(fileCopy);
            return asset;
        }


        public Asset LoadAsset(FileInfo file)
        {
            lock (Assets)
            {
                var asset = Assets.FirstOrDefault(x => x.FullPath == file.FullName);
                if (asset == null)
                {
                    asset = new Asset(file);
                    Assets.Add(asset);
                }
                return asset;
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

        #region filewatcher
        public FileSystemWatcher Watcher { get; private set; }
        public ICollectionView AssetView { get; private set; }
        private void FileChanged(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
        private void FileCreated(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
        private void FileRenamed(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
        private void FileDeleted(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
        #endregion

    }
    public class AssetManagerUpdatedMessage
    {
    }
}