// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.SetTab.ItemModel
{
    public partial class OptionsModel : IdeBaseItem
    {
        public OptionsList _options;
        public IdeCollection<IdeBaseItem> Items { get; private set; }
        public RelayCommand AddOptionCommand { get; private set; }

        public OptionsModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _options = new OptionsList();

            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionsDef(b);
            };
            Items.Add(new OptionModel(Items) );
            AddOptionCommand = new RelayCommand(AddOption);
        }

        public OptionsModel(OptionsList p, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _options = p;
            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionsDef(b);
            };
            foreach (Option item in p.Options)
            {
                Items.Add(new OptionModel(item, Items));
            }
            AddOptionCommand = new RelayCommand(AddOption);

        }

        public OptionsModel(OptionsModel p, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _options = new OptionsList();
            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                BuildOptionsDef(b);
            };
            foreach (OptionModel item in p.Items)
            {
                Items.Add(new OptionModel(item, Items) );
            }
            AddOptionCommand = new RelayCommand(AddOption);
        }

        public void BuildOptionsDef(NotifyCollectionChangedEventArgs args)
        {
            _options.Options = Items.Select(x => ((OptionModel)x).Option).ToList();
        }

        public void AddOption()
        {
            Items.Add(new OptionModel(Items));
        }

        public override object Clone()
        {
            return new OptionsModel(this, Source);
        }
        public override object Create()
        {
            return new OptionsModel(Source);
        }

    }
}
