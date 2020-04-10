// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ItemModel;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Octide.SetTab.ItemModel
{
    public class CardModel : IdeBaseItem
    {
        public Card _card { get; private set; }
        public IdeCollection<IdeBaseItem> Items { get; private set; }

     //   public SetCardAltItemViewModel BaseCardAlt => Items.FirstOrDefault(x => x.IsDefault) as SetCardAltItemViewModel;
        public AlternateModel BaseCardAlt { get; set; }

        public RelayCommand AddAltCommand { get; private set; }

        public CardModel(IdeCollection<IdeBaseItem> src) : base(src) //for adding new items
        {
            var guid = Guid.NewGuid();  //every card has a new GUID
            _card = new Card(
                guid,                           //card guid
                ((SetModel)Source.Parent).Id,   //set guid
                "Card",                         //card name
                guid.ToString(),                //imageuri
                "",                             //alternate
                ((SizeItemModel)ViewModelLocator.PreviewTabViewModel.CardSizes.DefaultItem)._size,  //size
                new Dictionary<string, CardPropertySet>());     //property sets

            Items = new IdeCollection<IdeBaseItem>(this);
            Items.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            Items.SelectedItemChanged += (a, b) =>
            {
                UpdateSelectedAltTabs(a, b);
            };

            BaseCardAlt = new AlternateModel(Items)
            {
                IsDefault = true,
                Name = ""
            };
            //TODO: Check that the base card initializes in the xml and itemsource properly
            AddAltCommand = new RelayCommand(AddAlt);
        }

        public CardModel(Card c, IdeCollection<IdeBaseItem> src) : base(src) //for loading an existing collection
        {
            _card = c;
            Items = new IdeCollection<IdeBaseItem>(this);
            foreach (var alt in c.PropertySets)
            {
                var AltItem = new AlternateModel(alt.Value, Items);
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
            Items.SelectedItem = BaseCardAlt;
            Items.SelectedItemChanged += (a, b) =>
            {
                UpdateSelectedAltTabs(a, b);
            };

            AddAltCommand = new RelayCommand(AddAlt);
        }

        public CardModel(CardModel c, IdeCollection<IdeBaseItem> src) : base(src) //for copying the item
        {
            var guid = Guid.NewGuid();
            _card = new Card(c._card)
            {
                Id = Guid.NewGuid(),
                ImageUri = guid.ToString()
            };
            Items = new IdeCollection<IdeBaseItem>(this);
            BaseCardAlt = new AlternateModel(c.BaseCardAlt, Items);
            Items.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            foreach (AlternateModel alt in c.Items)
            {
                Items.Add(new AlternateModel(alt, Items));
            }
            Items.SelectedItemChanged += (a, b) =>
            {
                UpdateSelectedAltTabs(a, b);
            };

            AddAltCommand = new RelayCommand(AddAlt);
        }

        public void AddAlt()
        {
            var ret = new AlternateModel(Items);
            Items.Add(ret);
            Items.SelectedItem = ret;
        }

        public void BuildCardDef(NotifyCollectionChangedEventArgs args)
        {
            _card.PropertySets = Items.Select(x => ((AlternateModel)x)._altDef).ToDictionary(x => x.Type, x => x);
            _card.PropertySets.Add(BaseCardAlt.Name, BaseCardAlt._altDef);
        }

        public override object Clone()
        {
            return new CardModel(this, Source);
        }
        public override object Create()
        {
            return new CardModel(Source);
        }

        private void UpdateSelectedAltTabs(object sender, NotifySelectedItemChangedEventArgs args)
        {
            RaisePropertyChanged("ClickBaseCard");
        }
        public bool ClickBaseCard
        {
            get
            {
                return (Items.SelectedItem != null && Items.SelectedItem == BaseCardAlt);
            }
            set
            {
                if (Items.SelectedItem == BaseCardAlt) return;
                Items.SelectedItem = BaseCardAlt;
                RaisePropertyChanged("ClickBaseCard");
            }
        }

        public void UpdateCardName()
        {
            _card.Name = BaseCardAlt.Name;
            RaisePropertyChanged("Name");
        }


        public string Name
        {
            get
            {
                return BaseCardAlt.CardName;
            }
            set
            {
                if (BaseCardAlt.CardName == value) return;
                BaseCardAlt.CardName = value;
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
