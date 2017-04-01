using System;
using System.Windows;
using System.Linq;
using System.ComponentModel;
using System.Windows.Media;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Octide.Messages;

namespace Octide.ViewModel
{
    public class SizeViewModel : ViewModelBase
    {
        private SizeItemModel _selectedItem;
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();

        
        public RelayCommand DeleteCommand { get; private set; }

        public SizeViewModel()
        {
            DeleteCommand = new RelayCommand(DeleteItem);
        }

        public void UpdateSizes()
        {
            ViewModelLocator.GameLoader.Game.CardSizes = ViewModelLocator.PreviewTabViewModel.CardSizes.ToDictionary(x => x.Name, y => y.SizeDef);
        }

        public void DeleteItem()
        {
            if (SelectedItem == null) return;
            ViewModelLocator.PreviewTabViewModel.CardSizes.Remove(SelectedItem);
        }

        public Visibility DefaultVisibility
        {
            get
            {
                return (SelectedItem.Default ? Visibility.Collapsed : Visibility.Visible);
            }
        }

        public SizeItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
                RaisePropertyChanged("DefaultVisibility");
            }
        }
    }

    public class SizeItemModel : ViewModelBase, ICloneable
    {
        public CardSize SizeDef;
        
        public SizeItemModel()
        {
            SizeDef = new CardSize();
            SizeDef.Front = ViewModelLocator.SizeViewModel.Images.First().FullPath;
            SizeDef.Back = ViewModelLocator.SizeViewModel.Images.First().FullPath;
            SizeDef.Name = Guid.NewGuid().ToString();
            RaisePropertyChanged("Back");
            RaisePropertyChanged("Front");
            RaisePropertyChanged("BackImage");
            RaisePropertyChanged("FrontImage");
        }

        public SizeItemModel(CardSize s)
        {
            SizeDef = s;
        }

        public SizeItemModel(SizeItemModel s)
        {
            SizeDef = new CardSize();
            SizeDef.Name = s.Name;
            SizeDef.Front = s.Front.FullPath;
            SizeDef.Height = s.Height;
            SizeDef.Width = s.Width;
            SizeDef.CornerRadius = s.CornerRadius;
            SizeDef.Back = s.Back.FullPath;
            SizeDef.BackHeight = s.Height;
            SizeDef.BackWidth = s.BackWidth;
            SizeDef.BackCornerRadius = s.BackCornerRadius;
        }

        public object Clone()
        {
            return new SizeItemModel(this);
        }

        public bool Default
        {
            get
            {
                return SizeDef.Name == "Default";
            }
        }
        
        public string Name
        {
            get
            {
                return SizeDef.Name;
            }
            set
            {
                if (SizeDef.Name == value) return;
                if (ViewModelLocator.PreviewTabViewModel.CardSizes.Count(x => x.Name == value) > 0 || value == "Default") return;
                SizeDef.Name = value;
                //has to update the card data when the size name changes
                RaisePropertyChanged("Name");
                ViewModelLocator.SizeViewModel.UpdateSizes();
                Messenger.Default.Send(new CardSizeChangedMesssage() { Size = this });
            }
        }

        public Asset Front
        {
            get
            {
                if (SizeDef.Front == null)
                    return new Asset();
                return Asset.Load(SizeDef.Front);
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                SizeDef.Front = value.FullPath;
                RaisePropertyChanged("Front");
                RaisePropertyChanged("FrontImage");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public string FrontImage => SizeDef.Front;

        public Asset Back
        {
            get
            {
                if (SizeDef.Back == null)
                    return new Asset();
                return Asset.Load(SizeDef.Back);
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                SizeDef.Back = value.FullPath;
                RaisePropertyChanged("Back");
                RaisePropertyChanged("BackImage");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public string BackImage => SizeDef.Back;

        public int Height
        {
            get
            {
                return SizeDef.Height;
            }
            set
            {
                if (value == SizeDef.Height) return;
                if (value < 5) return;
                SizeDef.Height = value;
                RaisePropertyChanged("Height");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int BackHeight
        {
            get
            {
                return SizeDef.BackHeight;
            }
            set
            {
                if (value == SizeDef.BackHeight) return;
                if (value < 5) return;
                SizeDef.BackHeight = value;
                RaisePropertyChanged("BackHeight");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int Width
        {
            get
            {
                return SizeDef.Width;
            }
            set
            {
                if (value == SizeDef.Width) return;
                if (value < 5) return;
                SizeDef.Width = value;
                RaisePropertyChanged("Width");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int BackWidth
        {
            get
            {
                return SizeDef.BackWidth;
            }
            set
            {
                if (value == SizeDef.BackWidth) return;
                if (value < 5) return;
                SizeDef.BackWidth = value;
                RaisePropertyChanged("BackWidth");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int CornerRadius
        {
            get
            {
                return SizeDef.CornerRadius;
            }
            set
            {
                if (value == SizeDef.CornerRadius) return;
                SizeDef.CornerRadius = value;
                RaisePropertyChanged("CornerRadius");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

        public int BackCornerRadius
        {
            get
            {
                return SizeDef.BackCornerRadius;
            }
            set
            {
                if (value == SizeDef.BackCornerRadius) return;
                SizeDef.BackCornerRadius = value;
                RaisePropertyChanged("BackCornerRadius");
                Messenger.Default.Send(new CardDetailsChangedMessage());
            }
        }

    }
}