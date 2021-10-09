// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.Library;
using Octide.Messages;
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
        public RelayCommand RefreshAllAssetsCommand {get; private set;}

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

                if (value != null)
                {
                    Watcher.Path = value.FullName;
                    Watcher.EnableRaisingEvents = true;
                }
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

            var _tempDirectory = Directory.CreateDirectory(Path.Combine(GameLoader.TempDirectory.FullName, "TempAssets")).FullName;

            var back = Path.Combine(_tempDirectory, "back.png");
            Properties.Resources.back.Save(back);
            DefaultCardBackAsset = LoadExternalAsset(new FileInfo(back), new string[] { "Assets" });

            var front = Path.Combine(_tempDirectory, "front.png");
            Properties.Resources.front.Save(front);
            DefaultCardFrontAsset = LoadExternalAsset(new FileInfo(front), new string[] { "Assets" });

            var background = Path.Combine(_tempDirectory, "background.jpg");
            Properties.Resources.background.Save(background);
            DefaultBackgroundAsset = LoadExternalAsset(new FileInfo(background), new string[] { "Assets" });

            var hand = Path.Combine(_tempDirectory, "hand.png");
            Properties.Resources.hand.Save(hand);
            LoadExternalAsset(new FileInfo(hand), new string[] { "Assets" });

            var deck = Path.Combine(_tempDirectory, "deck.png");
            Properties.Resources.deck.Save(deck);
            DefaultPileAsset = LoadExternalAsset(new FileInfo(deck), new string[] { "Assets" });

            var score = Path.Combine(_tempDirectory, "score.png");
            Properties.Resources.score.Save(score);
            LoadExternalAsset(new FileInfo(score), new string[] { "Assets" });



            Watcher = new FileSystemWatcher();
            Watcher.IncludeSubdirectories = true;
            //changed event is way too weird to handle properly
        //    Watcher.Changed += FileChanged;
            Watcher.Created += FileCreated;
            Watcher.Renamed += FileRenamed;
            Watcher.Deleted += FileDeleted;
            OpenFileLocationCommand = new RelayCommand(OpenFileLocation);
            RefreshAllAssetsCommand = new RelayCommand(RefreshAssets);

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
                asset.CreateSafeAsset(file);
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
                asset.CreateSafeAsset(file);
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

        public void RefreshAssets()
        {
            foreach (var asset in Assets)
            {
                asset.RefreshAsset();
            }
        }

        #region filewatcher
        public FileSystemWatcher Watcher { get; private set; }
        private void FileChanged(object sender, FileSystemEventArgs args)
        {
            Debug.WriteLine("FileChanged: " + args.FullPath);
            var file = new FileInfo(args.FullPath);
            if (file.Attributes.HasFlag(FileAttributes.Hidden) || file.Extension.ToLower().Equals(".tmp"))
            {

            }
            else if (file.Attributes.HasFlag(FileAttributes.Archive))
            {
                var asset = LoadInternalAsset(file);
                if (asset.SafeFile.Exists && file.Exists
                    && asset.SafeFile.LastWriteTime != file.LastWriteTime
                    && asset.SafeFile.Length != file.Length)
                {
                    asset.CreateSafeAsset(file);
                }
                Messenger.Default.Send(new AssetManagerUpdatedMessage());
            }
        }
        private void FileCreated(object sender, FileSystemEventArgs args)
        {
            Debug.WriteLine("FileCreated: " + args.FullPath);
            var file = new FileInfo(args.FullPath);
            if (file.Attributes.HasFlag(FileAttributes.Hidden) || file.Extension.ToLower().Equals(".tmp"))
            {

            }
            else if (file.Attributes.HasFlag(FileAttributes.Archive))
            {
                var asset = LoadInternalAsset(file);
                asset.FileLocationPath = args.FullPath;
            }
            else if (file.Attributes.HasFlag(FileAttributes.Directory))
            {
                /// new folder is created
            }
        }
        private void FileRenamed(object sender, RenamedEventArgs args)
        {
            Debug.WriteLine("FileRenamed: " + args.OldFullPath + " -> " + args.FullPath);
            var file = new FileInfo(args.FullPath);
            if (file.Attributes.HasFlag(FileAttributes.Hidden) || file.Extension.ToLower().Equals(".tmp"))
            {

            }
            else if (file.Attributes.HasFlag(FileAttributes.Directory))
            {
                var oldHierarchy = args.OldName.Split(Path.DirectorySeparatorChar);
                var newHierarchy = args.Name.Split(Path.DirectorySeparatorChar);
                // renaming a folder
                foreach (var asset in Assets)
                {
                    if (asset.Hierarchy.Length < oldHierarchy.Length) continue;
                    for (var i = 0; i < oldHierarchy.Length; i++)
                    {
                        if (asset.Hierarchy[i] != oldHierarchy[i])
                        {
                            break;
                        }
                        if (i == oldHierarchy.Length - 1)
                        {
                            asset.Hierarchy[i] = newHierarchy[i];
                            asset.FileLocationPath = asset.FullPath;
                            asset.RaisePropertyChanged("RelativePath");
                        }
                    }
                }
            }
            else if (file.Attributes.HasFlag(FileAttributes.Archive))
            {
                var asset = Assets.FirstOrDefault(x => x.FileLocationPath == args.OldFullPath);
                if (asset != null)
                {
                    asset.Name = Path.GetFileNameWithoutExtension(args.FullPath);
                    asset.FileLocationPath = args.FullPath;
                    asset.UpdateLinkedAssetPaths();
                }
            }
        }

        private void FileDeleted(object sender, FileSystemEventArgs args)
        {
            Debug.WriteLine("FileDeleted: " + args.FullPath);
            var file = new FileInfo(args.FullPath);
            if (file.Attributes.HasFlag(FileAttributes.Hidden) || file.Extension.ToLower().Equals(".tmp"))
            {

            }
            else if (file.Attributes.HasFlag(FileAttributes.Directory))
            {

            }
            else if (file.Attributes.HasFlag(FileAttributes.Archive))
            {
                var asset = LoadInternalAsset(file);
                asset.FileLocationPath = null;
                if (asset.LinkedAssetsCount == 0)
                {
                    Assets.Remove(asset);
                }
                else
                {
                    // todo: maybe a dialog asking to unlink the assets?
                }
            }
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
}