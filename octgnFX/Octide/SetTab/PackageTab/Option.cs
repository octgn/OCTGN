// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.SetTab.ItemModel
{
    public class OptionModel : IdeBaseItem
    {
        public Option Option { get; set; }
        public RelayCommand AddPickCommand { get; private set; }
        public RelayCommand AddOptionsCommand { get; private set; }

        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public OptionModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            Option = new Option
            {
                Definition = new PackDefinition()
            };
            Items = new IdeCollection<IdeBaseItem>(this);

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
            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionDef(b);
            };
            foreach (var item in o.Definition.Items)
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
                Definition = new PackDefinition(),
                Probability = p.Probability
            };
            Items = new IdeCollection<IdeBaseItem>(this);
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
            Option.Definition.Items = Items.Select(x => ((IBasePack)x)._packItem).ToList();
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
