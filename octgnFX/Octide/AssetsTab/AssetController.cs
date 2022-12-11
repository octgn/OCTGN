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
using Octgn.Library;

namespace Octide
{
    public class AssetController : ViewModelBase
    {
        public RelayCommand LoadAssetButton { get; private set; }
        public RelayCommand ClearAssetCommand { get; private set; }

        public AssetButtonDropHandler DropHandler { get; set; }
        public AssetType TargetAssetType { get; set; }
        public Asset _asset;
        public ICollectionView AssetView { get; set; }

        public bool CanRemove { get; set; }

        public string SafePath => SelectedAsset?.SafeFile.FullName;

        public string FullPath
        {
            get
            {
                return SelectedAsset?.FullPath;
            }
        }

        public AssetController(AssetType type)
        {
            LoadAssetButton = new RelayCommand(LoadAssetDialog);
            ClearAssetCommand = new RelayCommand(ClearAsset);
            DropHandler = new AssetButtonDropHandler() { Controller = this };

            TargetAssetType = type;

            AssetView = new ListCollectionView(ViewModelLocator.AssetsTabViewModel.Assets);
            AssetView.Filter = obj =>
            {
                var asset = obj as Asset;
                return asset.Type == TargetAssetType;
            };
            RaisePropertyChanged("AssetView");

            SelectedAsset = ViewModelLocator.AssetsTabViewModel.Assets.FirstOrDefault(x => x.Type == type);
            RaisePropertyChanged("AssetView");
        }

        public void Register(string path)
        {
            if (path != null)
            {
                var file = new FileInfo(path);
                SelectedAsset = ViewModelLocator.AssetsTabViewModel.LoadAsset(file);
            }
            RaisePropertyChanged("AssetView");
        }

        public void ClearAsset()
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
                RaisePropertyChanged("Path");
                RaisePropertyChanged("RemoveButtonVisibility");
                RaisePropertyChanged("AssetView");
            }
        }

        public void LoadAssetDialog()
        {
            var fo = new OpenFileDialog
            {
                Filter = Asset.GetAssetFilters(TargetAssetType)
            };
            if ((bool)fo.ShowDialog() == false)
            {
                return;
            }
            var file = new FileInfo(fo.FileName);
            var asset = ViewModelLocator.AssetsTabViewModel.LoadAsset(file);
            if (asset == null) return;
            SelectedAsset = asset;
        }

        public Visibility RemoveButtonVisibility
        {
            get
            {
                return (CanRemove && SelectedAsset != null) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

    }
    public class AssetButtonDropHandler : IDropTarget
    {
        public AssetController Controller { get; set; }

        public void DragEnter(IDropInfo dropInfo) {
            
        }

        public void DragLeave(IDropInfo dropInfo) {
            
        }

        public void DragOver(IDropInfo dropInfo)
        {
            try
            {
                var data = dropInfo.Data as IDataObject;
                var path = (string[])data.GetData(DataFormats.FileDrop);
                var file = new FileInfo(path.First());
                if (Asset.GetAssetType(file.Extension) != Controller.TargetAssetType)
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

            var asset = ViewModelLocator.AssetsTabViewModel.LoadAsset(file);
            if (asset == null) return;
            Controller.SelectedAsset = asset;
        }
    }
}
