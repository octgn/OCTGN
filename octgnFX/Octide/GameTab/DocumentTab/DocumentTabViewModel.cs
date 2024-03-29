﻿// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octide.ItemModel;
using System.Linq;

namespace Octide.ViewModel
{
    public class DocumentTabViewModel : ViewModelBase
    {

        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public RelayCommand AddCommand { get; private set; }


        public DocumentTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);

            Items = new IdeCollection<IdeBaseItem>(this, typeof(DocumentItemModel));
            foreach (var document in ViewModelLocator.GameLoader.Game.Documents)
            {
                Items.Add(new DocumentItemModel(document, Items));
            }
            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.Documents = Items.Select(x => (x as DocumentItemModel)._document).ToList();
            };
        }

        public void AddItem()
        {
            var ret = new DocumentItemModel(Items);
            Items.Add(ret);
        }
    }

}