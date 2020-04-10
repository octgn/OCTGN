// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class CounterItemModel : IdeBaseItem
    {
        public Counter _counter;

        public RelayCommand IncreaseCommand { get; private set; }
        public RelayCommand DecreaseCommand { get; private set; }

        public void IncreaseValue()
        {
            DefaultValue += 1;
        }

        public void DecreaseValue()
        {
            DefaultValue -= 1;
        }

        public CounterItemModel(IdeCollection<IdeBaseItem> source) : base(source)
        {
            IncreaseCommand = new RelayCommand(IncreaseValue);
            DecreaseCommand = new RelayCommand(DecreaseValue);
            _counter = new Counter
            {
                Icon = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath
            };
            Name = "New Counter";
            RaisePropertyChanged("Asset");
        }

        public CounterItemModel(Counter c, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _counter = c;
            IncreaseCommand = new RelayCommand(IncreaseValue);
            DecreaseCommand = new RelayCommand(DecreaseValue);
        }

        public CounterItemModel(CounterItemModel c, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _counter = new Counter
            {
                Icon = c.Asset.FullPath,
                Reset = c.Reset,
                Start = c.DefaultValue
            };
            Name = c.Name;
            IncreaseCommand = new RelayCommand(IncreaseValue);
            DecreaseCommand = new RelayCommand(DecreaseValue);
        }

        public override object Clone()
        {
            return new CounterItemModel(this, Source);
        }

        public override object Create()
        {
            return new CounterItemModel(Source);
        }

        public IEnumerable<string> UniqueNames => Source.Select(x => ((CounterItemModel)x).Name);

        public string Name
        {
            get
            {
                return _counter.Name;
            }
            set
            {
                if (value == _counter.Name) return;
                _counter.Name = Utils.GetUniqueName(value, UniqueNames);
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

        public int DefaultValue
        {
            get
            {
                return _counter.Start;
            }
            set
            {
                if (_counter.Start == value) return;
                _counter.Start = value;
                RaisePropertyChanged("DefaultValue");
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
