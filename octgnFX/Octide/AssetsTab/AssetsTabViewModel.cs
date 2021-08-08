// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Octgn.Library;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Octide.ViewModel
{
    public class AssetsTabViewModel : ViewModelBase
    {
        public AsyncObservableCollection<Asset> Assets { get; set; }
        private bool _filterLinked;
        private AssetType _filterType;


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
        public AssetsTabViewModel()
        {
            Assets = new AsyncObservableCollection<Asset>();
            AssetList = new List<Asset>();
            Assets.CollectionChanged += (a, b) =>
            {
                RefreshAssetsList();
            };

            Watcher = new FileSystemWatcher();
            Watcher.IncludeSubdirectories = true;
            //      Watcher.Changed += FileChanged;
            //      Watcher.Created += FileCreated;
            //      Watcher.Renamed += FileRenamed;
            //      Watcher.Deleted += FileDeleted;
            if (ViewModelLocator.GameLoader.WorkingDirectory != null)
            {
                Watcher.Path = ViewModelLocator.GameLoader.WorkingDirectory.FullName;
                Watcher.EnableRaisingEvents = true;
            }
        }

        private string WorkingDirectory => ViewModelLocator.GameLoader.WorkingDirectory?.FullName;


        public Asset LoadAsset(FileInfo file)
        {
            if (WorkingDirectory != null && file.FullName.StartsWith(WorkingDirectory))
            {
                // loads a file from within the working directory
                var asset = Assets.FirstOrDefault(x => x.FullPath == file.FullName);
                if (asset == null)
                {
                    asset = new Asset(file);
                    if (file.Exists)
                    {
                        asset.TargetFile = file;
                    }
                    var hierarchyList = new List<string>();
                    var directory = file.Directory;

                    while (directory != null && directory.FullName != WorkingDirectory)
                    {
                        if (directory == null) break;
                        hierarchyList.Insert(0, directory.Name);
                        directory = directory.Parent;
                    }
                    asset.Hierarchy = hierarchyList.ToArray();
                    asset.Name = Path.GetFileNameWithoutExtension(file.FullName);

                    Assets.Add(asset);
                }
                return asset;
            }
            else
            {
                // loads a file from outside the working directory
                var asset = new Asset(file);
                asset.Hierarchy = new string[] { "Asset" };

                asset.Name = GetUniqueFileName(asset.Hierarchy, Path.GetFileNameWithoutExtension(file.FullName), file.Extension);
                Assets.Add(asset);
                return asset;
            }
        }

        public Asset NewAsset(string[] hierarchy, string name, string extension)
        {
            var asset = new Asset()
            {
                Hierarchy = hierarchy,
                Name = GetUniqueFileName(hierarchy, name, extension),
                Extension = extension
            };

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
        public void UpdateWorkingDirectory(DirectoryInfo newPath)
        {
            Watcher.Path = newPath.FullName;
            foreach (var asset in Assets)
            {
                asset.UpdateLinkedAssetPaths();
            }
        }
        private void FileChanged(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
        private void FileCreated(object sender, FileSystemEventArgs args)
        {

            var file = new FileInfo(args.FullPath);
            ViewModelLocator.AssetsTabViewModel.LoadAsset(file);
        }
        private void FileRenamed(object sender, RenamedEventArgs args)
        {
            var asset = Assets.FirstOrDefault(x => x.TargetFilePath == args.OldFullPath);
            if (asset != null)
            {
                asset.TargetFile = new FileInfo(args.FullPath);
                asset.UpdateLinkedAssetPaths();
            }
        }
        private void FileDeleted(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
        #endregion

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