
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.Linq;
namespace Octide.ItemModel
{
     public class DocumentItemViewModel : IdeListBoxItemBase
    {
        public Document _document;

        public DocumentItemViewModel() // new item
        {
            _document = new Document
            {
                Name = "Document",
                Source = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Document)?.FullPath,
                Icon = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath
            };
        }

        public DocumentItemViewModel(Document d) // load item
        {
            _document = d;
        }

        public DocumentItemViewModel(DocumentItemViewModel d) // copy item
        {
            _document = new Document()
            {
                Name = d.Name,
                Source = d.DocumentAsset.FullPath,
                Icon = d.IconAsset.FullPath
            };
            ItemSource = d.ItemSource;
            Parent = d.Parent;
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
            return new DocumentItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as DocumentItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new DocumentItemViewModel() { Parent = Parent, ItemSource = ItemSource });
        }
    }
}
