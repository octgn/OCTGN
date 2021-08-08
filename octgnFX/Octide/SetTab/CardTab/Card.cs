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
        public Card Card { get; private set; }
        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public RelayCommand AddAltCommand { get; private set; }

        public CardModel(IdeCollection<IdeBaseItem> src) : base(src) //for adding new items
        {
            var guid = Guid.NewGuid();  //every card has a new GUID
            Card = new Card(
                guid,                           //card guid
                ((SetModel)Source.Parent).Id,   //set guid
                "Card",                         //card name
                guid.ToString(),                //imageuri
                "",                             //alternate
                ((SizeItemModel)ViewModelLocator.PreviewTabViewModel.CardSizes.DefaultItem)._size,  //size
                new Dictionary<string, CardPropertySet>());     //property sets

            Items = new IdeCollection<IdeBaseItem>(this, typeof(AlternateModel));
            Items.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            var baseCardAlt = new AlternateModel(Items);
            Items.Add(baseCardAlt);
            Items.DefaultItem = baseCardAlt;
            Items.SelectedItem = baseCardAlt;

            AddAltCommand = new RelayCommand(AddAlt);
        }

        public CardModel(Card c, IdeCollection<IdeBaseItem> src) : base(src) //for loading an existing collection
        {
            Card = c;
            Items = new IdeCollection<IdeBaseItem>(this, typeof(AlternateModel));
            foreach (var alt in c.PropertySets)
            {
                var AltItem = new AlternateModel(alt.Value, Items);
                if (alt.Key == "") // deals with the default card alt
                {
                    Items.DefaultItem = AltItem;
                    Items.SelectedItem = AltItem;
                }
                Items.Add(AltItem);
            }
            Items.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            AddAltCommand = new RelayCommand(AddAlt);
        }

        public CardModel(CardModel c, IdeCollection<IdeBaseItem> src) : base(src) //for copying the item
        {
            var guid = Guid.NewGuid();
            Card = new Card(c.Card)
            {
                Id = Guid.NewGuid()
            };
            Items = new IdeCollection<IdeBaseItem>(this, typeof(AlternateModel));
            Items.CollectionChanged += (a, b) =>
            {
                BuildCardDef(b);
            };
            foreach (AlternateModel alt in c.Items)
            {
                AlternateModel newAlt = new AlternateModel(alt, Items);
                if (alt.IsDefault)
                {
                    Items.DefaultItem = newAlt;
                    Items.SelectedItem = newAlt;
                }
                Items.Add(newAlt);
            }

            AddAltCommand = new RelayCommand(AddAlt);
        }

        public void AddAlt()
        {
            var ret = new AlternateModel(Items);
            Items.Add(ret);
        }

        public void BuildCardDef(NotifyCollectionChangedEventArgs args)
        {
            Card.PropertySets = Items.Select(x => (AlternateModel)x).ToDictionary(x => (x.IsDefault) ? "" : x.Type, x => x._altDef);
        }

        public override object Clone()
        {
            return new CardModel(this, Source);
        }
        public override object Create()
        {
            return new CardModel(Source);
        }

        public void UpdateCardName()
        {
            RaisePropertyChanged("Name");
        }


        public string Name
        {
            get
            {
                return ((AlternateModel)Items.DefaultItem).Name;
            }
            set
            {
                if (((AlternateModel)Items.DefaultItem).Name == value) return;
                ((AlternateModel)Items.DefaultItem).Name = value;
                RaisePropertyChanged("Name");
            }
        }
        public Guid Id
        {
            get
            {
                return Card.Id;
            }
        }
    }
}
