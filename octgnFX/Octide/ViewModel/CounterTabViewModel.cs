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
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace Octide.ViewModel
{
    public class CounterTabViewModel : ViewModelBase
    {
        private Visibility _panelVisibility;
        private Game _game;
        private CounterListItemModel _selectedItem;
        public ObservableCollection<CounterListItemModel> Items { get; set; }
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand RemoveCommand { get; private set; }
        public RelayCommand UpCommand { get; private set; }
        public RelayCommand DownCommand { get; private set; }
        
        public CounterTabViewModel()
        {
            _game = ViewModelLocator.GameLoader.Game;
            AddCommand = new RelayCommand(AddItem);
            RemoveCommand = new RelayCommand(RemoveItem, EnableButton);
            UpCommand = new RelayCommand(MoveItemUp, EnableButton);
            DownCommand = new RelayCommand(MoveItemDown, EnableButton);
            PanelVisibility = Visibility.Collapsed;
            Items = new ObservableCollection<CounterListItemModel>(_game.Player.Counters.Select(x => new CounterListItemModel(x)));
            Items.CollectionChanged += (a, b)=>{
                _game.Player.Counters = Items.Select(x => x._counter).ToList();
            };
        }

        public CounterListItemModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                if (value == null) PanelVisibility = Visibility.Collapsed;
                else PanelVisibility = Visibility.Visible;
                RaisePropertyChanged("SelectedItem");
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
            return _selectedItem != null;
        }

        public void AddItem()
        {
            var ret = new CounterListItemModel() { Name = "Counter", Icon = null };
            Items.Add(ret);
            SelectedItem = ret;
        }

        public void RemoveItem()
        {
            Items.Remove(SelectedItem);
        }

        public void MoveItemUp()
        {
            MoveItem(-1);
        }

        public void MoveItemDown()
        {
            MoveItem(1);
        }

        public void MoveItem(int move)
        {
            var index = Items.IndexOf(_selectedItem);
            int newIndex = index + move;
            if (newIndex < 0 || newIndex >= Items.Count) return;
            Items.Move(index, index + move);
        }
        
    }

    public class CounterListItemModel : ViewModelBase
    {
        public Counter _counter;
        private string _iconImage;

        public CounterListItemModel()
        {
            _counter = new Counter();
        }

        public CounterListItemModel(Counter c)
        {
            _counter = c;
        }

        public string Name
        {
            get
            {
                return _counter.Name;
            }
            set
            {
                if (value == _counter.Name) return;
                _counter.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public Asset Icon
        {
            get
            {
                if (_counter.Icon == null)
                    return new Asset();
                return Asset.Load(_counter.Icon);
            }
            set
            {
                if (value == null) return;
                if (!File.Exists(value.FullPath)) return;
                _counter.Icon = value.FullPath;
                _iconImage = Asset.Load(_counter.Icon).FullPath;
                RaisePropertyChanged("Icon");
                RaisePropertyChanged("IconImage");
            }
        }

        public string IconImage => _iconImage;

        public int Start
        {
            get
            {
                return _counter.Start;
            }
            set
            {
                if (value == _counter.Start) return;
                _counter.Start = value;
                RaisePropertyChanged("Start");
            }
        }
                
        public bool Reset
        {
            get
            {
                return _counter.Reset;
            }
            set
            {
                if (value == _counter.Reset) return;
                _counter.Reset = value;
                RaisePropertyChanged("Reset");
            }
        }
    }
}