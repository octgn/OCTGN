// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Messaging;
using Octgn.ProxyGenerator.Definitions;
using Octide.Messages;

namespace Octide.ProxyTab.ItemModel
{
    public class ArtOverlayBlockModel : IBaseBlock
    {

        public ArtOverlayBlockModel(IdeCollection<IdeBaseItem> source) : base(source) //new
        {
            var _blockDefinition = new BlockDefinition()
            {
            };
            _wrapper = new LinkDefinition.LinkWrapper() { CardArtCrop = _blockDefinition };
            PropertyChanged += ((a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage()));
        }

        public ArtOverlayBlockModel(LinkDefinition.LinkWrapper lw, IdeCollection<IdeBaseItem> source) : base(source) //load
        {
            _wrapper = lw;
            PropertyChanged += ((a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage()));
        }

        public ArtOverlayBlockModel(ArtOverlayBlockModel artOverlay, IdeCollection<IdeBaseItem> source) : base(source) //copy
        {
            _wrapper = new LinkDefinition.LinkWrapper()
            {
                CardArtCrop = new BlockDefinition()
            };
            PropertyChanged += ((a, b) => Messenger.Default.Send(new ProxyTemplateChangedMessage()));
        }

        public override object Clone()
        {
            return new ArtOverlayBlockModel(this, Source);
        }
        public override object Create()
        {
            return new ArtOverlayBlockModel(Source);
        }

        public int X
        {
            get
            {
                return _wrapper.CardArtCrop.location.x;
            }
            set
            {
                if (_wrapper.CardArtCrop.location.x == value) return;
                _wrapper.CardArtCrop.location.x = value;
                RaisePropertyChanged("X");
            }
        }
        public int Y
        {
            get
            {
                return _wrapper.CardArtCrop.location.y;
            }
            set
            {
                if (_wrapper.CardArtCrop.location.y == value) return;
                _wrapper.CardArtCrop.location.y = value;
                RaisePropertyChanged("X");
            }
        }
        public int Width
        {
            get
            {
                return _wrapper.CardArtCrop.wordwrap.width;
            }
            set
            {
                if (_wrapper.CardArtCrop.wordwrap.width == value) return;
                _wrapper.CardArtCrop.wordwrap.width = value;
                RaisePropertyChanged("Width");
            }
        }
        public int Height
        {
            get
            {
                return _wrapper.CardArtCrop.wordwrap.height;
            }
            set
            {
                if (_wrapper.CardArtCrop.wordwrap.height == value) return;
                _wrapper.CardArtCrop.wordwrap.height = value;
                RaisePropertyChanged("Height");
            }
        }
    }
}
