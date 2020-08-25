// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.SetTab.ItemModel
{
    public class OptionModel : IdeBaseItem, IDroppable
    {
        public Option Option { get; set; }
        public RelayCommand AddPickCommand { get; private set; }
        public RelayCommand AddOptionsCommand { get; private set; }

        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public OptionModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            Option = new Option
            {
                Items = new List<object>()
            };
            Items = new IdeCollection<IdeBaseItem>(this, typeof(OptionsModel), typeof(PickModel));

            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionDef(b);
            };
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
        }

        public OptionModel(Option o, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            Option = o;
            Items = new IdeCollection<IdeBaseItem>(this, typeof(OptionsModel), typeof(PickModel));
            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionDef(b);
            };
            foreach (var item in o.Items)
            {
                if (item is OptionsList options)
                    Items.Add(new OptionsModel(options, Items));
                else if (item is Pick pick)
                    Items.Add(new PickModel(pick, Items));
            }
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
        }

        public OptionModel(OptionModel p, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            Option = new Option
            {
                Items = new List<object>(),
                Probability = p.Probability
            };
            Items = new IdeCollection<IdeBaseItem>(this, typeof(OptionsModel), typeof(PickModel));
            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionDef(b);
            };
            foreach (var packItem in p.Items)
            {
                if (packItem is PickModel pick)
                    Items.Add(new PickModel(pick, Items) );
                if (packItem is OptionsModel options)
                    Items.Add(new OptionsModel(options, Items) );
            }
            AddOptionsCommand = new RelayCommand(AddOptions);
            AddPickCommand = new RelayCommand(AddPick);
        }
        public void BuildOptionDef(NotifyCollectionChangedEventArgs args)
        {
            List<object> items = new List<object>();
            foreach (var packItem in Items)
            {
                if (packItem is PickModel pick)
                {
                    items.Add(pick._pick);
                }
                else if (packItem is OptionsModel options)
                {
                    items.Add(options._options);
                }
            }
            Option.Items = items;
        }

        public void AddPick()
        {
            Items.Add(new PickModel(Items) );
        }

        public void AddOptions()
        {
            Items.Add(new OptionsModel(Items));
        }
        public override object Clone()
        {
            return new OptionModel(this, Source);
        }
        public override object Create()
        {
            return new OptionModel(Source);
        }

        public bool CanAccept(object item)
        {
            if (item is PickModel || item is OptionsModel)
                return true;
            return false;
        }

        public double Probability
        {
            get
            {
                return Option.Probability;
            }
            set
            {
                if (Option.Probability == value) return;
                Option.Probability = value;
                RaisePropertyChanged("Probability");
            }
        }
    }
}
