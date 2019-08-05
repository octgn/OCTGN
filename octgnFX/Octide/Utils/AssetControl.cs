using System.Windows.Controls;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Data;
using System.IO;
using System.Linq;
using System.ComponentModel;
using Octide.ViewModel;
using System;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using Elysium;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;

namespace Octide
{
    [TemplatePart(Name = "PART_Button", Type = typeof(Button))]
    public partial class AssetControl : Control
    {
        static AssetControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AssetControl), new FrameworkPropertyMetadata(typeof(AssetControl)));
        }

        public static readonly DependencyProperty SelectedAssetProperty =
            DependencyProperty.Register("SelectedAsset", typeof(Asset), typeof(AssetControl),
                new FrameworkPropertyMetadata(
                    OnSelectedAssetChanged,
                        CoerceSelectedAsset
                    )
                );

        private static void OnSelectedAssetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        private static object CoerceSelectedAsset(DependencyObject element, object value)
        {
            return (Asset)value;
        }

        public Asset SelectedAsset
        {
            get { return (Asset)GetValue(SelectedAssetProperty); }
            set { SetValue(SelectedAssetProperty, value); }
        }

        public AssetType TargetAssetType
        {
            get { return (AssetType)GetValue(TargetAssetTypeProperty); }
            set { SetValue(TargetAssetTypeProperty, value); }
        }
        
        public static readonly DependencyProperty TargetAssetTypeProperty =
            DependencyProperty.Register("TargetAssetType", typeof(AssetType), typeof(AssetControl), new UIPropertyMetadata(AssetType.Other));
        
        public ICollectionView AssetView { get; set; }
        
        public AssetControl()
        {
            AssetView = new ListCollectionView(AssetManager.Instance.Assets);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            AssetView.Filter = obj =>
            {
                var asset = obj as Asset;
                return asset.Type == TargetAssetType;
            };

            var _button = GetTemplateChild("PART_Button") as Button;
            _button.Click += LoadAsset;
            _button.DragOver += OnDragOver;
            _button.Drop += OnDrop;
        }
        
        public bool FilterAssets(object item)
        {
            var asset = item as Asset;
            return asset.Type == TargetAssetType;
        }

        public void LoadAsset(object sender, RoutedEventArgs e)
        {
            var asset = AssetManager.Instance.LoadAsset(TargetAssetType);
            if (asset == null) return;
            SelectedAsset = asset;
        }

        protected void OnDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
            try
            {
                var data = e.Data as IDataObject;
                var path = (string[])data.GetData(DataFormats.FileDrop);
                var file = new FileInfo(path.First());
                if (Asset.GetAssetType(file) != TargetAssetType)
                    e.Effects = DragDropEffects.None;
            }
            catch
            {
                e.Effects = DragDropEffects.None;
            }
        }

        protected void OnDrop(object sender, DragEventArgs e)
        {
            var data = e.Data as IDataObject;
            var path = (string[])data.GetData(DataFormats.FileDrop);
            var file = new FileInfo(path[0]);

            var asset = AssetManager.Instance.LoadAsset(TargetAssetType, file);
            if (asset == null) return;
            SelectedAsset = asset;
        }
    }
}