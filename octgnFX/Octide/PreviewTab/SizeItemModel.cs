// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.Messages;
using System.Collections.Generic;
using System.Linq;

namespace Octide.ItemModel
{
    public class SizeItemModel : IdeBaseItem
    {
        public CardSize _size;

        public SizeItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            CanBeDefault = true;
            _size = new CardSize
            {
                Front = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
                Back = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
                Width = 15,
                Height = 15,
                BackWidth = 15,
                BackHeight = 15
            };
            Name = "New Size";
            RaisePropertyChanged("Back");
            RaisePropertyChanged("Front");
        }

        public SizeItemModel(CardSize s, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            CanBeDefault = true;
            _size = s;
        }

        public SizeItemModel(SizeItemModel s, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            CanBeDefault = true;
            _size = new CardSize
            {
                Front = s.Front.FullPath,
                Height = s.Height,
                Width = s.Width,
                CornerRadius = s.CornerRadius,
                Back = s.Back.FullPath,
                BackHeight = s.Height,
                BackWidth = s.BackWidth,
                BackCornerRadius = s.BackCornerRadius
            };
            Name = s.Name;
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
