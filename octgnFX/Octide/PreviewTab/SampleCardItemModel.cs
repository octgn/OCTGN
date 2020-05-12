// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.ItemModel;
using Octide.Messages;
using Octide.ViewModel;

namespace Octide.ItemModel
{
    public class SampleCardItemModel : ViewModelBase
    {
        private double _x;
        private double _y;
        private SizeItemModel _size;
        private bool _isBack = true;

        public bool IsBack
        {
            get { return _isBack; }
            set
            {
                if (_isBack == value) return;
                _isBack = value;
                RaisePropertyChanged("IsBack");
                RefreshValues();
            }
        }

        public double X
        {
            get { return _x; }
            set
            {
                if (_x == value) return;
                _x = value;
                RaisePropertyChanged("X");
            }
        }

        public double Y
        {
            get { return _y; }
            set
            {
                if (_y == value) return;
                _y = value;
                RaisePropertyChanged("Y");
            }
        }

        public SizeItemModel Size
        {
            get
            {
                if (_size == null) return (SizeItemModel)ViewModelLocator.PreviewTabViewModel.CardSizes.DefaultItem;
                return _size;
            }
            set
            {
                if (_size == value) return;
                _size = value;
                RaisePropertyChanged("Size");
            }
        }

        public int CardCornerRadius
        {
            get
            {
                return IsBack ? Size.BackCornerRadius : Size.CornerRadius;
            }
        }
        public int CardWidth
        {
            get
            {
               return IsBack ? Size.BackWidth : Size.Width;
            }
            set
            {
                if (IsBack) Size.BackWidth = value;
                else Size.Width = value;
                RaisePropertyChanged("CardWidth");
            }
        }

        public int CardHeight
        {
            get
            {
               return IsBack ? Size.BackHeight : Size.Height;
            }
            set
            {
                if (IsBack) Size.BackHeight = value;
                else Size.Height = value;
                RaisePropertyChanged("CardHeight");
            }
        }

        public string CardImage
        {
            get
            {
                return IsBack ? Size.BackAsset.SelectedAsset?.FullPath : Size.FrontAsset.SelectedAsset?.FullPath;
            }
        }

        public SampleCardItemModel()
        {
            Messenger.Default.Register<CardDetailsChangedMessage>(this, x => this.RefreshValues());
            Messenger.Default.Register<PropertyChangedMessage<Game>>(this, x => this.RefreshValues());
        }

        public void RefreshValues()
        {
            RaisePropertyChanged("");
            RaisePropertyChanged("Size");
            RaisePropertyChanged("CardWidth");
            RaisePropertyChanged("CardHeight");
            RaisePropertyChanged("CardImage");
            RaisePropertyChanged("CardCornerRadius");
        }
    }
}