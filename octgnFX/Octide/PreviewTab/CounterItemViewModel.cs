using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System;
using System.Linq;

namespace Octide.ItemModel
{
    public class CounterItemViewModel : IdeListBoxItemBase
    {
        public Counter _counter;

        public object _parent;

        public RelayCommand IncreaseCommand { get; private set; }
        public RelayCommand DecreaseCommand { get; private set; }

        public void IncreaseValue()
        {
            Default += 1;
        }

        public void DecreaseValue()
        {
            Default -= 1;
        }

        public CounterItemViewModel()
        {
            IncreaseCommand = new RelayCommand(IncreaseValue);
            DecreaseCommand = new RelayCommand(DecreaseValue);
            _counter = new Counter
            {
                Icon = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
                Name = "New Counter"
            };
            RaisePropertyChanged("Asset");
        }

        public CounterItemViewModel(Counter c)
        {
            _counter = c;
            IncreaseCommand = new RelayCommand(IncreaseValue);
            DecreaseCommand = new RelayCommand(DecreaseValue);
        }

        public CounterItemViewModel(CounterItemViewModel c)
        {
            _counter = new Counter
            {
                Icon = c.Asset.FullPath,
                Name = c.Name,
                Reset = c.Reset,
                Start = c.Default
            };
            IncreaseCommand = new RelayCommand(IncreaseValue);
            DecreaseCommand = new RelayCommand(DecreaseValue);
            ItemSource = c.ItemSource;
            Parent = c.Parent;
        }

        public override object Clone()
        {
            return new CounterItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as CounterItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new CounterItemViewModel() { Parent = Parent, ItemSource = ItemSource });
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
                if (string.IsNullOrEmpty(value)) return;
                _counter.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public Asset Asset
        {
            get
            {
                return Asset.Load(_counter.Icon);
            }
            set
            {
                _counter.Icon = value?.FullPath;
                RaisePropertyChanged("Asset");
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
