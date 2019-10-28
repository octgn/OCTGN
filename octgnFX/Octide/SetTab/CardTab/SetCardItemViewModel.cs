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
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Octide.ItemModel
{
    public class SetCardItemViewModel : IdeListBoxItemBase
    {
        public Card _card { get; private set; }
        public ObservableCollection<IdeListBoxItemBase> Items { get; private set; }

     //   public SetCardAltItemViewModel BaseCardAlt => Items.FirstOrDefault(x => x.IsDefault) as SetCardAltItemViewModel;
        public SetCardAltItemViewModel BaseCardAlt { get; private set; }

        private SetCardAltItemViewModel _selectedItem;

        public RelayCommand AddAltCommand { get; private set; }

        public SetCardItemViewModel() //for adding new items
        {
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, action => CardSizeChanged(action));
            AddAltCommand = new RelayCommand(AddAlt);
            var guid = Guid.NewGuid();  //every card has a new GUID
            _card = new Card(
                guid,
                ViewModelLocator.SetTabViewModel.SelectedItem.Id,
                "Card",
                guid.ToString(),
                "",
                ViewModelLocator.PreviewTabViewModel.DefaultSize._size,
                new Dictionary<string, CardPropertySet>());

            Items = new ObservableCollection<IdeListBoxItemBase>();
            Items.CollectionChanged += (a, b) =>
            {
                UpdateCardAlts();
            };
            BaseCardAlt = new SetCardAltItemViewModel()
            {
                IsDefault = true,
                Parent = this
            };
            BaseCardAlt.Name = "";
        }

        public SetCardItemViewModel(Card c) //for loading an existing collection
        {
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, action => CardSizeChanged(action));
            AddAltCommand = new RelayCommand(AddAlt);
            _card = c;
            Items = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var alt in c.PropertySets)
            {
                var AltItem = new SetCardAltItemViewModel(alt.Value)
                {
                    ItemSource = Items,
                    Parent = this
                };
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
                UpdateCardAlts();
            };
            SelectedItem = BaseCardAlt;
        }

        public SetCardItemViewModel(SetCardItemViewModel c) //for copying the item
        {
            Messenger.Default.Register<CustomPropertyChangedMessage>(this, action => CustomPropertyChanged(action));
            Messenger.Default.Register<CardSizeChangedMesssage>(this, action => CardSizeChanged(action));
            AddAltCommand = new RelayCommand(AddAlt);
            Parent = c.Parent;
            ItemSource = c.ItemSource;
            var guid = Guid.NewGuid();
            _card = new Card(c._card)
            {
                Id = Guid.NewGuid(),
                ImageUri = guid.ToString()
            };
            Items = new ObservableCollection<IdeListBoxItemBase>();
            foreach (SetCardAltItemViewModel alt in c.Items)
            {
                Items.Add(new SetCardAltItemViewModel(alt) { ItemSource = c.ItemSource, Parent = this });
            }
            BaseCardAlt = new SetCardAltItemViewModel(c.BaseCardAlt);
            Items.CollectionChanged += (a, b) =>
            {
                UpdateCardAlts();
            };
        }

        public void AddAlt()
        {
            var ret = new SetCardAltItemViewModel()
            {
                Parent = this,
                ItemSource = Items,
                Name = "Alt"
            };
            Items.Add(ret);
            SelectedItem = ret;
            RaisePropertyChanged("SelectedAlt");
        }

        public void CustomPropertyChanged(CustomPropertyChangedMessage m)
        {
            foreach (SetCardAltItemViewModel alt in Items)
            {
              //  alt.UpdateAltPropertySet();
            }
        }

        public void CardSizeChanged(CardSizeChangedMesssage m)
        {
            foreach (SetCardAltItemViewModel alt in Items)
            {
               alt.UpdateAltCardSize();
            }
        }

        public void UpdateCardName()
        {
            RaisePropertyChanged("Name");
        }

        public void UpdateCardAlts()
        {
            _card.PropertySets = Items.Select(x => (x as SetCardAltItemViewModel)._altDef).ToDictionary(x => x.Type, x => x);
            _card.PropertySets.Add(BaseCardAlt.Name, BaseCardAlt._altDef);
        }

        public override object Clone()
        {
            return new SetCardItemViewModel(this);
        }
        
        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = (Parent as SetItemViewModel).CardItems.IndexOf(this);
            (Parent as SetItemViewModel).CardItems.Insert(index, Clone() as SetCardItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = (Parent as SetItemViewModel).CardItems.IndexOf(this);
            (Parent as SetItemViewModel).CardItems.Insert(index, new SetCardItemViewModel() { Parent = Parent, ItemSource = ItemSource });
        }

        public SetCardAltItemViewModel SelectedItem
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
                return BaseCardAlt.NameProperty.Value;
            }
            set
            {
                if (BaseCardAlt.NameProperty.Value == value) return;
                BaseCardAlt.NameProperty.Value = value;
                RaisePropertyChanged("Name");
            }
        }
    }
}
