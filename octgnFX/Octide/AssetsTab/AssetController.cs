// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octide.ViewModel;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Win32;

namespace Octide
{
    public class AssetController : ViewModelBase
    {
        public RelayCommand LoadAssetButton { get; private set; }
        public RelayCommand UnlinkAssetCommand { get; private set; }

        public AssetButtonDropHandler DropHandler { get; set; }
        public AssetType TargetAssetType { get; set; }
        public Asset _asset;
        public ICollectionView AssetView { get; set; }

        public bool CanRemove { get; set; }

        public string FullPath => SelectedAsset?.FullPath;

        public AssetController(AssetType type) // Creates a new assetcontroller with a default asset from the given type.
        {
            InitializeAssetController(type);
            SelectedAsset = ViewModelLocator.AssetsTabViewModel.Assets.FirstOrDefault(x => x.Type == type);
            RaisePropertyChanged("AssetView");
        }

        public AssetController(AssetType type, string source)  // loads the assetcontroller with the pre-set asset (from initializing deserialized item models)
        {
            InitializeAssetController(type);
            if (source != null && source != ViewModelLocator.GameLoader.Directory)
            {
                var path = new FileInfo(source);
                SelectedAsset = ViewModelLocator.AssetsTabViewModel.LoadAsset(path);
            }
            RaisePropertyChanged("AssetView");

        }
        public AssetController(Asset asset)  // for loading an assetcontrol with a known asset
        {
            InitializeAssetController(asset.Type);
            SelectedAsset = asset;
            RaisePropertyChanged("AssetView");
        }

        public void InitializeAssetController(AssetType type)  //base method
        {
            LoadAssetButton = new RelayCommand(LoadAssetDialog);
            UnlinkAssetCommand = new RelayCommand(UnlinkAsset);
            DropHandler = new AssetButtonDropHandler() { Parent = this };

            TargetAssetType = type;
            AssetView = new ListCollectionView(ViewModelLocator.AssetsTabViewModel.Assets);
            AssetView.Filter = obj =>
            {
                var asset = obj as Asset;
                return asset.Type == TargetAssetType;
            };
        }

        public void UnlinkAsset()
        {
            if (SelectedAsset != null)
                SelectedAsset = null;
        }

        public Asset SelectedAsset
        {
            get
            {
                return _asset;
            }
            set
            {
                if (_asset == value) return;
                if (_asset != null)
                {
                    _asset.UnlinkAsset(this);
                }
                if (value != null)
                {
                    value.LinkAsset(this);
                }
                _asset = value;
                RaisePropertyChanged("SelectedAsset");
                RaisePropertyChanged("RemoveButtonVisibility");
            }
        }

        public void LoadAssetDialog()
        {
            var fo = new OpenFileDialog
            {
                Filter = AssetsTabViewModel.GetAssetFilters(TargetAssetType)
            };
            if ((bool)fo.ShowDialog() == false)
            {
                return;
            }
            var file = new FileInfo(fo.FileName);
            var asset = ViewModelLocator.AssetsTabViewModel.NewAsset(TargetAssetType, file);
            if (asset == null) return;
            SelectedAsset = asset;
        }

        public Visibility RemoveButtonVisibility
        {
            get
            {
                return (CanRemove && SelectedAsset != null) ? Visibility.Visible : Visibility.Hidden;
            }
        }

    }
    public class AssetButtonDropHandler : IDropTarget
    {
        public AssetController Parent { get; set; }
        public void DragOver(IDropInfo dropInfo)
        {
            try
            {
                var data = dropInfo.Data as IDataObject;
                var path = (string[])data.GetData(DataFormats.FileDrop);
                var file = new FileInfo(path.First());
                if (AssetsTabViewModel.GetAssetType(file) != Parent.TargetAssetType)
                    dropInfo.Effects = DragDropEffects.None;
                else
                {
                    dropInfo.Effects = DragDropEffects.Copy;
                }
            }
            catch
            {
                dropInfo.Effects = DragDropEffects.None;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var data = dropInfo.Data as IDataObject;
            var path = (string[])data.GetData(DataFormats.FileDrop);
            var file = new FileInfo(path[0]);

            var asset = ViewModelLocator.AssetsTabViewModel.NewAsset(Parent.TargetAssetType, file);
            if (asset == null) return;
            Parent.SelectedAsset = asset;
        }
    }
}
