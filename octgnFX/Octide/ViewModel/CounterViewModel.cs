using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Command;

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Octide.ViewModel
{
    public class CounterViewModel : ViewModelBase
    {
        private CounterItemModel _selectedItem;
        public IList<Asset> Images => AssetManager.Instance.Assets.Where(x => x.Type == AssetType.Image).ToList();

        public RelayCommand CopyCommand { get; private set; }
        public RelayCommand DeleteCommand { get; private set; }

        public CounterViewModel()
        {
            CopyCommand = new RelayCommand(CopyItem);
            DeleteCommand = new RelayCommand(DeleteItem);
        }

        public void CopyItem()
        {

            if (SelectedItem == null) return;
            var ret = new CounterItemModel(SelectedItem);
            ViewModelLocator.PreviewTabViewModel.Counters.Add(ret);
        }

        public void DeleteItem()
        {
            if (SelectedItem == null) return;
            ViewModelLocator.PreviewTabViewModel.Counters.Remove(SelectedItem);
        }

        public CounterItemModel SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

    }

    public class CounterItemModel : ViewModelBase
    {
        public Counter _counter;

        public RelayCommand IncreaseCommand { get; private set; }
        public RelayCommand DecreaseCommand { get; private set; }

        public string IconImage => _counter.Icon;

        public void IncreaseValue()
        {
            Default += 1;
        }

        public void DecreaseValue()
        {
            Default -= 1;
        }

        public CounterItemModel()
        {
            IncreaseCommand = new RelayCommand(IncreaseValue);
            DecreaseCommand = new RelayCommand(DecreaseValue);
            _counter = new Counter();
            _counter.Icon = ViewModelLocator.GroupViewModel.Images.First().FullPath;
            RaisePropertyChanged("Icon");
            RaisePropertyChanged("IconImage");
        }

        public CounterItemModel(Counter c)
        {
            _counter = c;
            IncreaseCommand = new RelayCommand(IncreaseValue);
            DecreaseCommand = new RelayCommand(DecreaseValue);
        }

        public CounterItemModel(CounterItemModel c)
        {
            _counter = new Counter();
            _counter.Icon = c.Icon.FullPath;
            _counter.Name = c.Name;
            _counter.Reset = c.Reset;
            _counter.Start = c.Default;
            IncreaseCommand = new RelayCommand(IncreaseValue);
            DecreaseCommand = new RelayCommand(DecreaseValue);
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
                RaisePropertyChanged("Icon");
                RaisePropertyChanged("IconImage");
            }
        }

        public int Default
        {
            get
            {
                return _counter.Start;
            }
            set
            {
                if (_counter.Start == value) return;
                _counter.Start = value;
                RaisePropertyChanged("Default");
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
                if (_counter.Reset == value) return;
                _counter.Reset = value;
                RaisePropertyChanged("Reset");
            }
        }
    
    }
}