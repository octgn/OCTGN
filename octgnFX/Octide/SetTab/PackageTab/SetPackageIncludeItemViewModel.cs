// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.Messages;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class PackIncludeItemModel : IdeListBoxItemBase
    {
        new public SetPackageItemViewModel Parent { get; set; }
        public Include _include;
        public ObservableCollection<PackPropertyItemModel> Items { get; private set; }

        public RelayCommand AddPropertyCommand { get; private set; }
        public PackIncludeItemModel()
        {
            _include = new Include()
            {
                Properties = new List<PickProperty>()
            };

            Items = new ObservableCollection<PackPropertyItemModel>();
            Items.CollectionChanged += (a, b) =>
            {
                _include.Properties = Items.Select(x => x.PropertyDef).ToList();
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
        }

        public PackIncludeItemModel(Include i)
        {
            _include = i;

            Items = new ObservableCollection<PackPropertyItemModel>(i.Properties.Select(x => new PackPropertyItemModel(x) { ParentCollection = Items }));
            Items.CollectionChanged += (a, b) =>
            {
                _include.Properties = Items.Select(x => x.PropertyDef).ToList();
            };
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
        }

        public PackIncludeItemModel(PackIncludeItemModel i)
        {
            _include = new Include()
            {
                Id = i.SelectedCard.Id,
                SetId = i.SelectedSet.Id
            };

            Items = new ObservableCollection<PackPropertyItemModel>(i.Items.Select(x => new PackPropertyItemModel(x) { ParentCollection = Items }));
            Items.CollectionChanged += (a, b) =>
            {
                _include.Properties = Items.Select(x => x.PropertyDef).ToList();
            };
            _include.Properties = Items.Select(x => x.PropertyDef).ToList();

            ItemSource = i.ItemSource;
            Parent = i.Parent;
            AddPropertyCommand = new RelayCommand(AddProperty);
            RemoveCommand = new RelayCommand(Remove);
        }

        public ObservableCollection<IdeListBoxItemBase> Sets => ViewModelLocator.SetTabViewModel.Items;
        public ObservableCollection<IdeListBoxItemBase> Cards => SelectedSet?.CardItems;

        public SetItemViewModel _selectedSet;

        public SetItemViewModel SelectedSet
        {
            get
            {
                if (_include.SetId == null)
                    return null;
                return (SetItemViewModel)Sets.FirstOrDefault(x => (x as SetItemViewModel).Id == _include.SetId);
            }
            set
            {
                if (SelectedSet == value) return;
                _include.SetId = value.Id;
                RaisePropertyChanged("SelectedSet");
                RaisePropertyChanged("Cards");
                SelectedCard = (SetCardItemViewModel)Cards.FirstOrDefault();
            }
        }
        public SetCardItemViewModel SelectedCard
        {
            get
            {
                if (_include.Id == null)
                    return null;
                return (SetCardItemViewModel)SelectedSet?.CardItems.FirstOrDefault(x => (x as SetCardItemViewModel).Id == _include.Id);
            }
            set
            {

                if (SelectedCard == value) return;
                _include.Id = value.Id;
                RaisePropertyChanged("SelectedCard");
            }
        }

        public void AddProperty()
        {
            Items.Add(new PackPropertyItemModel() { ParentCollection = Items });
        }

        public override object Clone()
        {
            return new PackIncludeItemModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as PackIncludeItemModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new PackIncludeItemModel() { ItemSource = ItemSource, Parent = Parent });
        }
    }
}