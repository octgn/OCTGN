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
        public FileSystemWatcher Watcher { get; private set; }
        private ObservableCollection<Asset> _assets;
        private bool _filterUnused;
        private AssetType _filterType;
        public AssetsTabViewModel()
        {
            if (ViewModelLocator.GameLoader.Directory != null)
            {
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
        public ICollectionView AssetView { get; private set; }


        public ObservableCollection<Asset> Assets
        {
            get
            {
                if (_assets == null)
                {
                    _assets = new ObservableCollection<Asset>();
                    // AssetManager.Instance.CollectAssets();
                }
                return _assets;
            }
            set
            {
                _assets = value;
                RaisePropertyChanged("Assets");
                RaisePropertyChanged("AssetView");
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
            var assetPath = Path.Combine(ViewModelLocator.GameLoader.Directory, "Assets");
            if (!Directory.Exists(assetPath))
                Directory.CreateDirectory(assetPath);

            var fileCopy = file.CopyTo(Utils.GetUniqueFilename(Path.Combine(assetPath, file.Name)));
            var asset = Asset.Load(fileCopy);
            Assets.Add(asset);
            return asset;
        }

        public void CollectAssets()
        {
            var di = new DirectoryInfo(ViewModelLocator.GameLoader.Directory);
            var files = di.GetFiles("*.*", SearchOption.AllDirectories);
            var ret = files.Select(Asset.Load);
            Assets = new ObservableCollection<Asset>(ret);
            AssetView = new ListCollectionView(Assets);
            Assets.CollectionChanged += (a, b) => AssetView.Refresh();
        }

    }
    public class AssetManagerUpdatedMessage
    {
    }
}