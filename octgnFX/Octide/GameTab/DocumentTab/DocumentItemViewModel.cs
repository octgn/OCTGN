// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.ComponentModel;
using System.Linq;
namespace Octide.ItemModel
{
    public class DocumentItemModel : IdeBaseItem
    {
        public Document _document;

        public AssetController IconAsset { get; set; }
        public AssetController DocumentAsset { get; set; }
        public DocumentItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _document = new Document();
            DocumentAsset = new AssetController(AssetType.Document);
            _document.Source = DocumentAsset.FullPath;
            DocumentAsset.PropertyChanged += DocumentAssetUpdated;
            IconAsset = new AssetController(AssetType.Image);
            _document.Icon = IconAsset.FullPath;
            IconAsset.PropertyChanged += IconAssetUpdated;
            Name = "New Document";
        }

        public DocumentItemModel(Document d, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _document = d;
            DocumentAsset = new AssetController(AssetType.Document);
            DocumentAsset.Register(d.Source);
            DocumentAsset.PropertyChanged += DocumentAssetUpdated;
            IconAsset = new AssetController(AssetType.Image);
            IconAsset.Register(d.Icon);
            IconAsset.PropertyChanged += IconAssetUpdated;
        }

        public DocumentItemModel(DocumentItemModel d, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _document = new Document();
            DocumentAsset = new AssetController(AssetType.Document);
            DocumentAsset.Register(d._document.Source);
            _document.Source = DocumentAsset.FullPath;
            DocumentAsset.PropertyChanged += DocumentAssetUpdated;
            IconAsset = new AssetController(AssetType.Image);
            IconAsset.Register(d._document.Icon);
            _document.Icon = IconAsset.FullPath;
            IconAsset.PropertyChanged += IconAssetUpdated;
            Name = d.Name;
        }

        private void DocumentAssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _document.Source = DocumentAsset.FullPath;
                RaisePropertyChanged("DocumentAsset");
            }
        }
        private void IconAssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _document.Icon = IconAsset.FullPath;
                RaisePropertyChanged("IconAsset");
                RaisePropertyChanged("Icon");
            }
        }
        public override void Cleanup()
        {
            DocumentAsset.SelectedAsset = null;
            IconAsset.SelectedAsset = null;
            base.Cleanup();
        }

        public string Name
        {
            get
            {
                return _document.Name;
            }
            set
            {
                if (_document.Name == value) return;
                _document.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public new string Icon => IconAsset.SafePath;

        public override object Clone()
        {
            return new DocumentItemModel(this, Source);
        }

        public override object Create()
        {
            return new DocumentItemModel(Source);
        }
    }
}
