// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.SetTab.ItemModel
{
    public class IncludeModel : IdeBaseItem, IDroppable
    {
        public Include _include;
        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public RelayCommand AddPropertyCommand { get; private set; }
        public IncludeModel(IdeCollection<IdeBaseItem> src) : base(src) //new
        {
            _include = new Include()
            {
                Properties = new List<PickProperty>()
            };

            Items = new IdeCollection<IdeBaseItem>(this, typeof(PackagePropertyModel));
            Items.CollectionChanged += (a, b) =>
            {
                BuildIncludeDef(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
            SelectedSet = (SetModel)Sets.First();

        }

        public IncludeModel(Include i, IdeCollection<IdeBaseItem> src) : base(src) //load
        {
            _include = i;

            Items = new IdeCollection<IdeBaseItem>(this, typeof(PackagePropertyModel));
            foreach (var include in i.Properties)
            {
                Items.Add(new PackagePropertyModel(include, Items) { IsIncludeProperty = true });

            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildIncludeDef(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
        }

        public IncludeModel(IncludeModel i, IdeCollection<IdeBaseItem> src) : base(src) //copy
        {
            _include = new Include()
            {
                Id = i._include.Id,
                SetId = i._include.SetId,
                Properties = new List<PickProperty>()
            };

            Items = new IdeCollection<IdeBaseItem>(this, typeof(PackagePropertyModel));
            Items.CollectionChanged += (a, b) =>
            {
                BuildIncludeDef(b);
            };
            foreach (PackagePropertyModel item in i.Items)
            {
                Items.Add(new PackagePropertyModel(item, Items) { IsIncludeProperty = true });
            }

            AddPropertyCommand = new RelayCommand(AddProperty);
        }

        public void AddProperty()
        {
            Items.Add(new PackagePropertyModel(Items) { IsIncludeProperty = true });
        }

        public override object Clone()
        {
            return new IncludeModel(this, Source);
        }
        public override object Create()
        {
            return new IncludeModel(this, Source);
        }

        public bool CanAccept(object item)
        {
            if (item is PackagePropertyModel)
                return true;
            return false;
        }

        public void BuildIncludeDef(NotifyCollectionChangedEventArgs args)
        {
            _include.Properties = Items.Select(x => ((PackagePropertyModel)x).Def).ToList();
        }

        public IEnumerable<IdeBaseItem> Sets => ViewModelLocator.SetTabViewModel.Items.Where(x => ((PackageModel)Source.Parent).Source.Parent != x && ((SetModel)x).CardItems.Count > 0); //TODO : test this rewrite

        public IEnumerable<CardModel> Cards => SelectedSet?.CardItems.Cast<CardModel>();

        public SetModel SelectedSet
        {
            get
            {
                if (_include.SetId == null)
                    return null;
                return (SetModel)Sets.FirstOrDefault(x => ((SetModel)x).Id == _include.SetId);
            }
            set
            {
                if (_include.SetId == value.Id) return;
                _include.SetId = value.Id;
                RaisePropertyChanged("SelectedSet");
                SelectedCard = Cards.First();
                RaisePropertyChanged("Cards");
            }
        }
        public CardModel SelectedCard
        {
            get
            {
                if (_include.Id == null)
                    return null;
                return (CardModel)SelectedSet?.CardItems.FirstOrDefault(x => ((CardModel)x).Id == _include.Id);
            }
            set
            {

                if (_include.Id == value.Id) return;
                _include.Id = value.Id;
                RaisePropertyChanged("SelectedCard");
            }
        }
    }
}