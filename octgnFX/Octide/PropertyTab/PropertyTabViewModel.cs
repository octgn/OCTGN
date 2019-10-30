// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octide.Messages;
using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GongSolutions.Wpf.DragDrop;
using System.Collections.Specialized;
using Octide.ItemModel;

namespace Octide.ViewModel
{
    public class PropertyTabViewModel : ViewModelBase
    {

        private PropertyItemViewModel _selectedItem;
        public ObservableCollection<IdeListBoxItemBase> Items { get; private set; }

        public PropertyItemViewModel NameProperty;

        public PropertyItemViewModel SizeProperty;

        public RelayCommand AddCommand { get; private set; }

        public PropertyTabViewModel()
        {
            Items = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var property in ViewModelLocator.GameLoader.Game.CustomProperties)
            {
                Items.Add(
                    new PropertyItemViewModel(property)
                    {
                        ItemSource = Items,
                    });
            }
            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.CustomProperties = Items.Select(x => (x as PropertyItemViewModel)._property).ToList();
                Messenger.Default.Send(new CustomPropertyChangedMessage(b)) ;
            };
            AddCommand = new RelayCommand(AddItem);
            NameProperty = new PropertyItemViewModel();
            NameProperty._property.Name = "Name";
            SizeProperty = new PropertyItemViewModel();
            SizeProperty._property.Name = "CardSize";
        }
        
        public PropertyItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem) return;
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        public void AddItem()
        {
            var ret = new PropertyItemViewModel() { ItemSource = Items, Parent = this, Name = "Property" };
            Items.Add(ret);
            SelectedItem = ret;
        }
    }

}