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
        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public PropertyItemModel NameProperty;
        public PropertyItemModel IdProperty;
        public PropertyItemModel AlternateProperty;

        public PropertyItemModel SizeProperty;
        public PropertyItemModel SizeNameProperty;
        public PropertyItemModel SizeHeightProperty;
        public PropertyItemModel SizeWidthProperty;
        public PropertyItemModel ProxyNameProperty;

        public RelayCommand AddCommand { get; private set; }

        public PropertyTabViewModel()
        {
            Items = new IdeCollection<IdeBaseItem>(this);
            foreach (var property in ViewModelLocator.GameLoader.Game.CustomProperties)
            {
                Items.Add(new PropertyItemModel(property, Items));
            }
            Items.CollectionChanged += (sender, args) =>
            {
                ViewModelLocator.GameLoader.Game.CustomProperties = Items.Select(x => ((PropertyItemModel)x)._property).ToList();
                Messenger.Default.Send(new CustomPropertyChangedMessage(args)) ;
            };
            AddCommand = new RelayCommand(AddItem);
            
            //TODO: Make sure these property names aren't already set

            NameProperty = new PropertyItemModel(new PropertyDef() { Name = "Name" }, Items);
            IdProperty = new PropertyItemModel(new PropertyDef() { Name = "Id" }, Items);
            AlternateProperty = new PropertyItemModel(new PropertyDef() { Name = "Alternate" }, Items);
            SizeProperty = new PropertyItemModel(new PropertyDef() { Name = "CardSize" }, Items);
            //Proxy Properties
            ProxyNameProperty = new PropertyItemModel(new PropertyDef() { Name = "CardName" }, Items);
            SizeNameProperty = new PropertyItemModel(new PropertyDef() { Name = "CardSizeName" }, Items);
            SizeHeightProperty = new PropertyItemModel(new PropertyDef() { Name = "CardSizeHeight" }, Items);
            SizeWidthProperty = new PropertyItemModel(new PropertyDef() { Name = "CardSizeWidth" }, Items);
        }
        

        public void AddItem()
        {
            var ret = new PropertyItemModel(Items);
            Items.Add(ret);
            Items.SelectedItem = ret;
        }
    }

}