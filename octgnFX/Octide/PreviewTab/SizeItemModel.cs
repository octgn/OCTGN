// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.Messages;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class SizeItemModel : IdeBaseItem
    {
        public CardSize _size;
        public AssetController FrontAsset { get; set; }
        public AssetController BackAsset { get; set; }

        public SizeItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            CanBeDefault = true;
            _size = new CardSize
            {
                Width = 15,
                Height = 15,
                BackWidth = 15,
                BackHeight = 15
            };
            Name = "New Size";
            BackAsset = new AssetController(AssetType.Image);
            _size.Back= BackAsset.FullPath;
            BackAsset.PropertyChanged += BackAssetUpdated;
            RaisePropertyChanged("Back");
            FrontAsset = new AssetController(AssetType.Image);
            _size.Front = FrontAsset.FullPath;
            FrontAsset.PropertyChanged += FrontAssetUpdated;
            RaisePropertyChanged("Front");
        }

        public SizeItemModel(CardSize s, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            CanBeDefault = true;
            _size = s;
            BackAsset = new AssetController(AssetType.Image);
            BackAsset.Register(s.Back);
            BackAsset.PropertyChanged += BackAssetUpdated;
            FrontAsset = new AssetController(AssetType.Image);
            FrontAsset.Register(s.Front);
            FrontAsset.PropertyChanged += FrontAssetUpdated;
        }

        public SizeItemModel(SizeItemModel s, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            CanBeDefault = true;
            _size = new CardSize
            {
                Height = s.Height,
                Width = s.Width,
                CornerRadius = s.CornerRadius,
                BackHeight = s.Height,
                BackWidth = s.BackWidth,
                BackCornerRadius = s.BackCornerRadius
            };
            BackAsset = new AssetController(AssetType.Image);
            BackAsset.Register(s._size.Back);
            _size.Back = BackAsset.FullPath;
            BackAsset.PropertyChanged += BackAssetUpdated;
            FrontAsset = new AssetController(AssetType.Image);
            FrontAsset.Register(s._size.Front);
            _size.Front = FrontAsset.FullPath;
            FrontAsset.PropertyChanged += FrontAssetUpdated;
            Name = s.Name;
        }
        private void BackAssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _size.Back = BackAsset.FullPath;
                Messenger.Default.Send(new CardDetailsChangedMessage());
                RaisePropertyChanged("BackAsset");
                RaisePropertyChanged("Icon");
            }
        }
        private void FrontAssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _size.Front = FrontAsset.FullPath;
                Messenger.Default.Send(new CardDetailsChangedMessage());
                RaisePropertyChanged("FrontAsset");
            }
        }
        public override void Cleanup()
        {
            FrontAsset.SelectedAsset = null;
            BackAsset.SelectedAsset = null;
            base.Cleanup();
        }

        public override object Clone()
        {
            return new SizeItemModel(this, Source);
        }

        public override object Create()
        {
            return new SizeItemModel(Source);
        }

        public IEnumerable<string> UniqueNames => Source.Select(x => ((SizeItemModel)x).Name);
        public new string Icon => BackAsset.SafePath;

        public string Name
        {
            get
            {
                return _size.Name;
            }
            set
            {
                if (_size.Name == value) return;
                _size.Name = Utils.GetUniqueName(value, UniqueNames);
                RaisePropertyChanged("Name");
                //has to update the card data when the size name changes
                Messenger.Default.Send(new CardSizeChangedMesssage() { Size = this, Action = PropertyChangedMessageAction.Modify });
            }
        }

        public int Height
        {
            get
            {
                return _size.Height;
            }
            set
            {
                if (value == _size.Height) return;
                _size.Height = value;
                RaisePropertyChanged("Height");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int BackHeight
        {
            get
            {
                return _size.BackHeight;
            }
            set
            {
                if (value == _size.BackHeight) return;
                _size.BackHeight = value;
                RaisePropertyChanged("BackHeight");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int Width
        {
            get
            {
                return _size.Width;
            }
            set
            {
                if (value == _size.Width) return;
                _size.Width = value;
                RaisePropertyChanged("Width");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int BackWidth
        {
            get
            {
                return _size.BackWidth;
            }
            set
            {
                if (value == _size.BackWidth) return;
                _size.BackWidth = value;
                RaisePropertyChanged("BackWidth");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int CornerRadius
        {
            get
            {
                return _size.CornerRadius;
            }
            set
            {
                if (value == _size.CornerRadius) return;
                _size.CornerRadius = value;
                RaisePropertyChanged("CornerRadius");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int BackCornerRadius
        {
            get
            {
                return _size.BackCornerRadius;
            }
            set
            {
                if (value == _size.BackCornerRadius) return;
                _size.BackCornerRadius = value;
                RaisePropertyChanged("BackCornerRadius");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

    }
}
