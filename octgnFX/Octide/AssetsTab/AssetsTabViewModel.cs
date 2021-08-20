// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.Library;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Octide.ViewModel
{
    public class AssetsTabViewModel : ViewModelBase
    {
        public AsyncObservableCollection<Asset> Assets { get; set; }
        private bool _filterLinked;
        private AssetType _filterType;
        public RelayCommand OpenFileLocationCommand {get; private set;}

        private DirectoryInfo _assetTempDirectory;
        public DirectoryInfo AssetTempDirectory
        {
            get
            {
                if (_assetTempDirectory == null)
                {
                    _assetTempDirectory = Directory.CreateDirectory(Config.Instance.Paths.GraveyardPath);
                }
                return _assetTempDirectory;
            }
        }

        public Asset DefaultBackgroundAsset;
        public Asset DefaultCardBackAsset;
        public Asset DefaultCardFrontAsset;
        public Asset DefaultPileAsset;

        private DirectoryInfo _workingDirectory;

        public DirectoryInfo WorkingDirectory
        {
            get
            {
                return _workingDirectory;
            }
            set
            {
                if (_workingDirectory != null && _workingDirectory.FullName.Equals(value.FullName)) return;
                _workingDirectory = value;
                if (ViewModelLocator.GameLoader.Game != null)
                    ViewModelLocator.GameLoader.Game.InstallPath = value.FullName;

                Watcher.Path = value.FullName;
                foreach (var asset in Assets)
                {
                    asset.UpdateLinkedAssetPaths();
                }
                RaisePropertyChanged(nameof(WorkingDirectory));
            }
        }

        public AssetsTabViewModel()
        {
            Assets = new AsyncObservableCollection<Asset>();
            AssetList = new List<Asset>();
            Assets.CollectionChanged += (a, b) =>
            {
                RefreshAssetsList();
            };

            //load in some sample assets

            var _tempDirectory = Directory.CreateDirectory(Path.Combine(Config.Instance.Paths.GraveyardPath, "OCTIDE"));

            var resourcePath = Path.Combine(_tempDirectory.FullName, "TempAssets");

            Directory.CreateDirectory(resourcePath);

            var back = Path.Combine(resourcePath, "back.png");
            Properties.Resources.back.Save(back);
            DefaultCardBackAsset = LoadExternalAsset(new FileInfo(back), new string[] { "Assets" });

            var front = Path.Combine(resourcePath, "front.png");
            Properties.Resources.front.Save(front);
            DefaultCardFrontAsset = LoadExternalAsset(new FileInfo(front), new string[] { "Assets" });

            var background = Path.Combine(resourcePath, "background.jpg");
            Properties.Resources.background.Save(background);
            DefaultBackgroundAsset = LoadExternalAsset(new FileInfo(background), new string[] { "Assets" });

            var hand = Path.Combine(resourcePath, "hand.png");
            Properties.Resources.hand.Save(hand);
            LoadExternalAsset(new FileInfo(hand), new string[] { "Assets" });

            var deck = Path.Combine(resourcePath, "deck.png");
            Properties.Resources.deck.Save(deck);
            DefaultPileAsset = LoadExternalAsset(new FileInfo(deck), new string[] { "Assets" });

            var score = Path.Combine(resourcePath, "score.png");
            Properties.Resources.score.Save(score);
            LoadExternalAsset(new FileInfo(score), new string[] { "Assets" });



            Watcher = new FileSystemWatcher();
            Watcher.IncludeSubdirectories = true;
            //      Watcher.Changed += FileChanged;
            //      Watcher.Created += FileCreated;
            //      Watcher.Renamed += FileRenamed;
            //      Watcher.Deleted += FileDeleted;
            if (WorkingDirectory != null)
            {
                Watcher.Path = WorkingDirectory.FullName;
                Watcher.EnableRaisingEvents = true;
            }
            OpenFileLocationCommand = new RelayCommand(OpenFileLocation);
        }

        private void OpenFileLocation()
        {
            if (Directory.Exists(Watcher?.Path))
            {
                Process.Start("explorer.exe", Watcher.Path);
            }
        }

        public Asset FindAsset(FileInfo file)
        {
            if (WorkingDirectory == null)
            {
                
                return null;
            }
            else
            {
                return Assets.FirstOrDefault(x => x.FullPath == file.FullName);
            }
        }

        public Asset LoadAsset(FileInfo file)
        {
            if (WorkingDirectory != null && file.FullName.StartsWith(WorkingDirectory.FullName))
            {
                return LoadInternalAsset(file);
            }
            else
            {
                return LoadExternalAsset(file, new string[] { "Assets" });
            }
        }

        public Asset LoadInternalAsset(FileInfo file)
        {
            // check to make sure the asset isn't already loaded
            var asset = Assets.FirstOrDefault(x => x.FullPath == file.FullName);
            if (asset == null)
            {
                asset = new Asset();
                asset.Name = Path.GetFileNameWithoutExtension(file.FullName);
                asset.Extension = file.Extension;
                var safeFilePath = Path.Combine(AssetTempDirectory.FullName, Guid.NewGuid().ToString() + file.Extension);
                asset.SafeFile = file.CopyTo(safeFilePath);
                asset.SafeFile.Attributes = FileAttributes.Temporary;
                if (file.Exists)
                {
                    asset.FileLocationPath = file.FullName;
                }
                var hierarchyList = new List<string>();
                var directory = file.Directory;

                while (directory.FullName != WorkingDirectory.FullName)
                {
                    hierarchyList.Insert(0, directory.Name);
                    directory = directory.Parent;
                    if (directory == null)
                    {
                        hierarchyList.Clear();
                        break;
                    }
                }
                asset.Hierarchy = hierarchyList.ToArray();
                Assets.Add(asset);
            }
            return asset;
        }

        public Asset LoadExternalAsset(FileInfo file, string[] hierarchy)
        {
            var asset = new Asset();
            if (file.Exists)
            {
                var safeFilePath = Path.Combine(AssetTempDirectory.FullName, Guid.NewGuid().ToString() + file.Extension);
                asset.SafeFile = file.CopyTo(safeFilePath);
                asset.SafeFile.Attributes = FileAttributes.Temporary;
            }
            asset.Extension = file.Extension;
            asset.Hierarchy = hierarchy;
            asset.Name = GetUniqueFileName(hierarchy, Path.GetFileNameWithoutExtension(file.FullName), file.Extension);
            Assets.Add(asset);
            return asset;
        }

        public string GetUniqueFileName(string[] hierarchy, string filename, string extension)
        {
            int n = 1;
            string ret = filename;
            while (Assets.Any(x =>
                x.Hierarchy.SequenceEqual(hierarchy)
                && x.Name == ret
                && x.Extension == extension))
            {
                ret = string.Format("{0} ({1})", filename, n++);
            }
            return ret;
        }

        public override void Cleanup()
        {
            base.Cleanup();
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
                RaisePropertyChanged("FilterType");
                RefreshAssetsList();
            }
        }

        public bool FilterLinked
        {
            get
            {
                return _filterLinked;
            }
            set
            {
                if (_filterLinked == value) return;
                _filterLinked = value;
                RaisePropertyChanged("FilterLinked");
                RefreshAssetsList();
            }
        }


        #region filewatcher
        public FileSystemWatcher Watcher { get; private set; }
        private void FileChanged(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
        private void FileCreated(object sender, FileSystemEventArgs args)
        {

            var file = new FileInfo(args.FullPath);
            ViewModelLocator.AssetsTabViewModel.LoadInternalAsset(file);
        }
        private void FileRenamed(object sender, RenamedEventArgs args)
        {
            var asset = Assets.FirstOrDefault(x => x.FileLocationPath == args.OldFullPath);
            if (asset != null)
            {
                asset.FileLocationPath = args.FullPath;
                asset.UpdateLinkedAssetPaths();
            }
        }
        private void FileDeleted(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
        #endregion

        public bool DeleteAsset(Asset asset)
        {
            Assets.Remove(asset);
            RefreshAssetsList();
            return true;
        }

        public void RefreshAssetsList()
        {
            AssetList = new List<Asset>(Assets.Where(x => FilterType == AssetType.All || x.Type == FilterType).Where(x => FilterLinked == false || x.IsLinked == false));
            RaisePropertyChanged("AssetList");
        }

        public List<Asset> AssetList { get; set; }

        public Asset _selectedAsset;
        public Asset SelectedAsset
        {
            get
            {
                return _selectedAsset;
            }
            set
            {
                if (_selectedAsset == value) return;
                _selectedAsset = value;
                RaisePropertyChanged("SelectedAsset");
            }
        }

    }
    public class AssetManagerUpdatedMessage
    {
    }
    public class WorkingDirectoryChangedMessage
    {
    }
}