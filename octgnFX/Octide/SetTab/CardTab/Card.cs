// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.Core.DataExtensionMethods;
using Octgn.DataNew.Entities;
using Octgn.Library;
using Octide.Messages;
using Octide.ProxyTab.TemplateItemModel;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Octide.SetTab.CardItemModel
{
    public class CardModel : IdeListBoxItemBase
    {
        public Card _card { get; private set; }

        public new ObservableCollection<CardModel> ItemSource { get; set; }
        public new SetModel Parent { get; set; }
        public ObservableCollection<AlternateModel> Items { get; private set; }

     //   public SetCardAltItemViewModel BaseCardAlt => Items.FirstOrDefault(x => x.IsDefault) as SetCardAltItemViewModel;
        public AlternateModel BaseCardAlt { get; set; }

        private AlternateModel _selectedItem;

        public RelayCommand AddAltCommand { get; private set; }

        public CardModel() //for adding new items
        {
            var guid = Guid.NewGuid();  //every card has a new GUID
            _card = new Card(
                guid,
                ViewModelLocator.SetTabViewModel.SelectedItem.Id,
                "Card",
                guid.ToString(),
                "",
                ViewModelLocator.PreviewTabViewModel.DefaultSize._size,
                new Dictionary<string, CardPropertySet>());

            Items = new ObservableCollection<AlternateModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            BaseCardAlt = new AlternateModel()
            {
                IsDefault = true,
                Parent = this,
            };
            BaseCardAlt.Name = "";
            //TODO: Check that the base card initializes in the xml and itemsource properly
            AddAltCommand = new RelayCommand(AddAlt);
        }

        public CardModel(Card c) //for loading an existing collection
        {
            _card = c;
            Items = new ObservableCollection<AlternateModel>();
            foreach (var alt in c.PropertySets)
            {
                var AltItem = new AlternateModel(alt.Value) { ItemSource = Items, Parent = this };
                if (alt.Key == "") // deals with the default card alt
                {
                    BaseCardAlt = AltItem;
                    BaseCardAlt.IsDefault = true;
                }
                else
                {
                    Items.Add(AltItem);
                }
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            SelectedItem = BaseCardAlt;

            AddAltCommand = new RelayCommand(AddAlt);
        }

        public CardModel(CardModel c) //for copying the item
        {
            var guid = Guid.NewGuid();
            _card = new Card(c._card)
            {
                Id = Guid.NewGuid(),
                ImageUri = guid.ToString()
            };
            BaseCardAlt = new AlternateModel(c.BaseCardAlt) { Parent = this };
            Items = new ObservableCollection<AlternateModel>();
            Items.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            foreach (AlternateModel alt in c.Items)
            {
                Items.Add(new AlternateModel(alt));
            }

            AddAltCommand = new RelayCommand(AddAlt);
        }

        public void AddAlt()
        {
            var ret = new AlternateModel()
            {
                Parent = this,
                Name = "Alt"
            };
            Items.Add(ret);
            SelectedItem = ret;
        }

        public void UpdateCardName()
        {
            _card.Name = BaseCardAlt.Name;
            RaisePropertyChanged("Name");
        }

        public void BuildCardDef(NotifyCollectionChangedEventArgs args)
        {
            if (args?.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (AlternateModel x in args.NewItems)
                {
                    x.ItemSource = Items;
                    x.Parent = this;
                }
            }
            _card.PropertySets = Items.Select(x => x._altDef).ToDictionary(x => x.Type, x => x);
            _card.PropertySets.Add(BaseCardAlt.Name, BaseCardAlt._altDef);
        }

        public override void Remove()
        {
            if (CanRemove == false) return;
            ItemSource.Remove(this);
        }

        public override object Clone()
        {
            return new CardModel(this);
        }
        
        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = Parent.CardItems.IndexOf(this);
            ItemSource.Insert(index, Clone() as CardModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = Parent.CardItems.IndexOf(this);
            ItemSource.Insert(index, new CardModel());
        }

        public AlternateModel SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if (_selectedItem == value) return;
                _selectedItem = value ?? BaseCardAlt; // return to the base card if you de-select the active alt
                RaisePropertyChanged("SelectedItem");
            }
        }

        public object Name
        {
            get
            {
                return BaseCardAlt.AltName;
            }
            set
            {
                if (BaseCardAlt.AltName == (string)value) return;
                BaseCardAlt.AltName = value.ToString() ;
                RaisePropertyChanged("Name");
            }
        }
        public Guid Id
        {
            get
            {
                return _card.Id;
            }
        }
    }
}
