// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.Linq;
namespace Octide.ItemModel
{
     public class DocumentItemModel : IdeBaseItem
    {
        public Document _document;

        public DocumentItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _document = new Document();
            DocumentAsset = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Document);
            IconAsset = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image);
            Name = "New Document";
        }

        public DocumentItemModel(Document d, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _document = d;
        }

        public DocumentItemModel(DocumentItemModel d, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _document = new Document()
            {
                Source = d.DocumentAsset.FullPath,
                Icon = d.IconAsset.FullPath
            };
            Name = d.Name;
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

        public Asset DocumentAsset
        {
            get
            {
                return Asset.Load(_document.Source);
            }
            set
            {
                _document.Source = value?.FullPath;
                RaisePropertyChanged("DocumentAsset");
            }
        }

        public new string Icon => IconAsset?.FullPath;

        public Asset IconAsset
        {
            get
            {
                return Asset.Load(_document.Icon);
            }
            set
            {
                _document.Icon = value?.FullPath;
                RaisePropertyChanged("IconAsset");
                RaisePropertyChanged("Icon");
            }
        }
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
