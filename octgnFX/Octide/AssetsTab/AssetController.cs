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
        public AssetController()  //base method
        {
            LoadAssetButton = new RelayCommand(LoadAsset);
            UnlinkAssetCommand = new RelayCommand(UnlinkAsset);
            AssetView = new ListCollectionView(ViewModelLocator.AssetsTabViewModel.Assets);
            DropHandler = new AssetButtonDropHandler() { Parent = this };
        }

        public AssetController(AssetType type) : this()  // for loading an empty assetcontrol
        {
            TargetAssetType = type;
            AssetView.Filter = obj =>
            {
                var asset = obj as Asset;
                return asset.Type == TargetAssetType;
            };
            SelectedAsset = ViewModelLocator.AssetsTabViewModel.Assets.FirstOrDefault(x => x.Type == type);
            RaisePropertyChanged("AssetView");
        }

        public AssetController(AssetType type, string source) : this()  // for loading an assetcontrol with a starting value
        {
            TargetAssetType = type;
            AssetView.Filter = obj =>
            {
                var asset = obj as Asset;
                return asset.Type == TargetAssetType;
            };
            if (source != null)
                SelectedAsset = Asset.Load(source);
            RaisePropertyChanged("AssetView");

        }
        public AssetController(Asset asset) : this()  // for loading an assetcontrol when the asset is already known
        {
            TargetAssetType = asset.Type;
            AssetView.Filter = obj =>
            {
                var a = obj as Asset;
                return a.Type == TargetAssetType;
            };
            SelectedAsset = asset;
            RaisePropertyChanged("AssetView");
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

        public void LoadAsset()
        {
            var asset = ViewModelLocator.AssetsTabViewModel.LoadAsset(TargetAssetType);
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
                    if (Asset.GetAssetType(file) != Parent.TargetAssetType)
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

                var asset = ViewModelLocator.AssetsTabViewModel.LoadAsset(Parent.TargetAssetType, file);
                if (asset == null) return;
                Parent.SelectedAsset = asset;
            }
        }
    }
}
