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
    public class SizeTabViewModel : ViewModelBase
    {
        private Visibility _panelVisibility;
        private Game _game;
        private SizeListItemModel _selectedItem;
        public ObservableCollection<SizeListItemModel> Items { get; set; }
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();
        public SizeListItemModel _defaultSize;


        public RelayCommand AddCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }
        public RelayCommand DefaultCommand { get; private set; }

        public SizeTabViewModel()
        {
            _game = ViewModelLocator.GameLoader.Game;
            AddCommand = new RelayCommand(AddItem);
            RemoveCommand = new RelayCommand(RemoveItem, EnableButton);
            DefaultCommand = new RelayCommand(Default, EnableButton);
            PanelVisibility = Visibility.Collapsed;
            Items = new ObservableCollection<SizeListItemModel>(_game.CardSizes.Values.Select(x => new SizeListItemModel(x)));
            _defaultSize = Items.First(x => x._size.Name == "Default");
            Items.CollectionChanged += (a, b) =>
            {
                _game.CardSizes = Items.ToDictionary(y => y._id.ToString(), y => y._size);
            };
        }

        public SizeListItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                if (value == null) PanelVisibility = Visibility.Collapsed;
                else PanelVisibility = Visibility.Visible;
                RaisePropertyChanged("SelectedItem");
                RemoveCommand.RaiseCanExecuteChanged();
                DefaultCommand.RaiseCanExecuteChanged();
            }
        }

        public Visibility PanelVisibility
        {
            get { return _panelVisibility; }
            set
            {
                if (value == _panelVisibility) return;
                _panelVisibility = value;
                RaisePropertyChanged("PanelVisibility");
            }
        }

        public bool EnableButton()
        {
            if (_selectedItem != null && _selectedItem.Default) return false;
            return _selectedItem != null;
        }

        public void AddItem()
        {
            var ret = new SizeListItemModel() { Name = "CardSize" };
            Items.Add(ret);
            SelectedItem = ret;
        }

        public void RemoveItem()
        {
            Items.Remove(SelectedItem);
            Messenger.Default.Send(new CardPropertiesUpdateMessage());
        }
        
        public void Default()
        {
            DefaultSize = SelectedItem;
            RemoveCommand.RaiseCanExecuteChanged();
            DefaultCommand.RaiseCanExecuteChanged();
        }

        public SizeListItemModel DefaultSize
        {
            get
            {
                return _defaultSize;
            }
            set
            {
                var oldSize = _defaultSize;
                if (_defaultSize == value) return;
                _defaultSize = value;
                oldSize.Update();
                _defaultSize.Update();
                RaisePropertyChanged("DefaultSize");
            }
        }
    }

    public class SizeListItemModel : ViewModelBase
    {
        public CardSize _size;
        public Guid _id;
        
        public SizeListItemModel()
        {
            _size = new CardSize();
            _id = Guid.NewGuid();
            _size.Front = ViewModelLocator.SizeTabViewModel.Images.First().FullPath;
            _size.Back = ViewModelLocator.SizeTabViewModel.Images.First().FullPath;
            RaisePropertyChanged("Back");
            RaisePropertyChanged("Front");
            RaisePropertyChanged("BackImage");
            RaisePropertyChanged("FrontImage");
        }

        public SizeListItemModel(CardSize s)
        {
            _size = s;
            _id = Guid.NewGuid();
        }

        public Guid Id
        {
            get
            {
                return _id;
            }
        }

        public void Update()
        {
            RaisePropertyChanged("Default");
        }

        public bool Default
        {
            get
            {
                return this == ViewModelLocator.SizeTabViewModel._defaultSize;
            }
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
                _size.Name = value;
                //has to update the card data when the size name changes
                RaisePropertyChanged("Name");
                Messenger.Default.Send(new CardPropertiesUpdateMessage());
            }
        }

        public Asset Front
        {
            get
            {
                if (_size.Front == null)
                    return new Asset();
                return Asset.Load(_size.Front);
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                _size.Front = value.FullPath;
                RaisePropertyChanged("Front");
                RaisePropertyChanged("FrontImage");
            }
        }

        public string FrontImage => _size.Front;

        public Asset Back
        {
            get
            {
                if (_size.Back == null)
                    return new Asset();
                return Asset.Load(_size.Back);
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                _size.Back = value.FullPath;
                RaisePropertyChanged("Back");
                RaisePropertyChanged("BackImage");
            }
        }

        public string BackImage => _size.Back;

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
            }
        }
    }
}