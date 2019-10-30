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
using System.Windows.Data;

namespace Octide.ViewModel
{
    public class DocumentTabViewModel : ViewModelBase
    {

        private DocumentItemViewModel _selectedItem;
        public ObservableCollection<IdeListBoxItemBase> Items { get; private set; }

        public RelayCommand AddCommand { get; private set; }


        public DocumentTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);

            Items = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var document in ViewModelLocator.GameLoader.Game.Documents)
            {
                Items.Add(new DocumentItemViewModel(document) { ItemSource = Items });
            }
            Items.CollectionChanged += (a, b) => {
                ViewModelLocator.GameLoader.Game.Documents = Items.Select(x => (x as DocumentItemViewModel)._document).ToList();
            };
        }
        
        public DocumentItemViewModel SelectedItem
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
            var ret = new DocumentItemViewModel() { ItemSource = Items, Name = "Document" };
            Items.Add(ret);
            SelectedItem = ret;
        }
    }

}