// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.Messages;
using Octide.ViewModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class SizeItemViewModel : IdeListBoxItemBase
    {
        public CardSize _size;

        public SizeItemViewModel() // new item
        {
            _size = new CardSize
            {
                Front = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
                Back = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
                Width = 15,
                Height = 15,
                BackWidth = 15,
                BackHeight = 15
            };
            RaisePropertyChanged("Back");
            RaisePropertyChanged("Front");
        }

        public SizeItemViewModel(CardSize s) // load item
        {
            _size = s;
        }

        public SizeItemViewModel(SizeItemViewModel s) // copy item
        {
            _size = new CardSize
            {
                Name = Utils.GetUniqueName(s.Name, s.ItemSource.Select(x => (x as SizeItemViewModel).Name)),
                Front = s.Front.FullPath,
                Height = s.Height,
                Width = s.Width,
                CornerRadius = s.CornerRadius,
                Back = s.Back.FullPath,
                BackHeight = s.Height,
                BackWidth = s.BackWidth,
                BackCornerRadius = s.BackCornerRadius
            };
            ItemSource = s.ItemSource;
            Parent = s.Parent;
        }

        public override object Clone()
        {
            return new SizeItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as SizeItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new SizeItemViewModel() { Parent = Parent, ItemSource = ItemSource, Name = "Size"});
        }

        public override void Remove()
        {
            if (CanRemove == false) return;
            ItemSource.Remove(this);
            Messenger.Default.Send(new CardSizeChangedMesssage());
        }

        public string Name
        {
            get
            {
                return _size.Name;
            }
            set
            {
                if (_size.Name == value) return;
                if (string.IsNullOrEmpty(value)) return;
                _size.Name = Utils.GetUniqueName(value, ItemSource.Select(x => (x as SizeItemViewModel).Name));
                RaisePropertyChanged("Name");
                //has to update the card data when the size name changes
                //Messenger.Default.Send(new CardSizeChangedMesssage() { Size = this });
            }
        }

        public Asset Front
        {
            get
            {
                return Asset.Load(_size.Front);
            }
            set
            {
                _size.Front = value?.FullPath;
                RaisePropertyChanged("Front");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public Asset Back
        {
            get
            {
                return Asset.Load(_size.Back);
            }
            set
            {
                _size.Back = value?.FullPath;
                RaisePropertyChanged("Back");
                Messenger.Default.Send(new CardDetailsChangedMessage());
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
                if (value < 5) return;
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
                if (value < 5) return;
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
                if (value < 5) return;
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
                if (value < 5) return;
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
                if (value < 0) return;
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
                if (value < 0) return;
                _size.BackCornerRadius = value;
                RaisePropertyChanged("BackCornerRadius");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

    }
}
