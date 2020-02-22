// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.ProxyGenerator.Definitions;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ProxyTab.TemplateItemModel;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Octide.ProxyTab.TemplateItemModel
{
    public class ArtOverlayBlockModel : IBaseBlock
    {

        public ArtOverlayBlockModel() //new
        {
            var _blockDefinition = new BlockDefinition()
            {
            };
            _wrapper = new LinkDefinition.LinkWrapper() {  CardArtCrop = _blockDefinition };
        }

        public ArtOverlayBlockModel(LinkDefinition.LinkWrapper lw) //load
        {
            _wrapper = lw;
        }

        public ArtOverlayBlockModel(ArtOverlayBlockModel artOverlay) //copy
        {
            _wrapper = new LinkDefinition.LinkWrapper()
            {
                CardArtCrop = new BlockDefinition()
                {
                }
            };

            Parent = artOverlay.Parent;
            ItemSource = artOverlay.ItemSource;
        }

        public override object Clone()
        {
            return new ArtOverlayBlockModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as ArtOverlayBlockModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new ArtOverlayBlockModel());
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
