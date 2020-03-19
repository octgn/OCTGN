// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.ItemModel;
using Octide.Messages;
using Octide.SetTab.CardItemModel;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.SetTab.PackageItemModel
{
    public class IncludeModel : IdeListBoxItemBase
    {
        new public PackageModel Parent { get; set; }
        public Include _include;
        public new ObservableCollection<IncludeModel> ItemSource { get; set; }
        public ObservableCollection<PropertyModel> Items { get; private set; }

        public RelayCommand AddPropertyCommand { get; private set; }
        public IncludeModel(PackageModel parent) //new
        {
            Parent = parent;
            _include = new Include()
            {
                Properties = new List<PickProperty>()
            };

            Items = new ObservableCollection<PropertyModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildIncludeDef(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
            SelectedSet = Sets.First();

        }

        public IncludeModel(Include i) //load
        {
            _include = i;

            Items = new ObservableCollection<PropertyModel>();
            foreach (var include in i.Properties)
            {
                Items.Add(new PropertyModel(include) { ItemSource = Items, isIncludeProperty = true });
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildIncludeDef(b);
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
        }

        public IncludeModel(IncludeModel i) //copy
        {
            _include = new Include()
            {
                Id = i._include.Id,
                SetId = i._include.SetId,
                Properties = new List<PickProperty>()
            };

            Items = new ObservableCollection<PropertyModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildIncludeDef(b);
            };
            foreach (var item in i.Items)
            {
                Items.Add(new PropertyModel(item) { ItemSource = Items, isIncludeProperty = true });
            }

            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
        }

        public void AddProperty()
        {
            Items.Add(new PropertyModel() { isIncludeProperty = true });
        }

        public override object Clone()
        {
            return new IncludeModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as IncludeModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new IncludeModel(Parent));
        }
        public void BuildIncludeDef(NotifyCollectionChangedEventArgs args)
        {
            if (args?.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PropertyModel x in args.NewItems)
                {
                    x.ItemSource = Items;
                }
            }
            _include.Properties = Items.Select(x => x.PropertyDef).ToList();
        }

        public IEnumerable<SetModel> Sets => ViewModelLocator.SetTabViewModel.Items.Cast<SetModel>().Where(x => x != Parent.Parent && x.CardItems.Count > 0);

        public IEnumerable<CardModel> Cards => SelectedSet?.CardItems.Cast<CardModel>();

        public SetModel SelectedSet
        {
            get
            {
                if (_include.SetId == null)
                    return null;
                return Sets.FirstOrDefault(x => x.Id == _include.SetId);
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
                return SelectedSet?.CardItems.FirstOrDefault(x => x.Id == _include.Id);
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