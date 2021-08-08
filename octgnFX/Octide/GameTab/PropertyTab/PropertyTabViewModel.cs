// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.ItemModel;
using Octide.Messages;
using System.Linq;

namespace Octide.ViewModel
{
    public class PropertyTabViewModel : ViewModelBase
    {
        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public static PropertyItemModel NameProperty = new PropertyItemModel(new PropertyDef() { Name = "Name" }, new IdeCollection<IdeBaseItem>());
        public static PropertyItemModel IdProperty = new PropertyItemModel(new PropertyDef() { Name = "Id" }, new IdeCollection<IdeBaseItem>());
        public static PropertyItemModel AlternateProperty = new PropertyItemModel(new PropertyDef() { Name = "Alternate" }, new IdeCollection<IdeBaseItem>());
        public static PropertyItemModel SizeProperty = new PropertyItemModel(new PropertyDef() { Name = "CardSize" }, new IdeCollection<IdeBaseItem>());
        public static PropertyItemModel SetProperty = new PropertyItemModel(new PropertyDef() { Name = "SetName" }, new IdeCollection<IdeBaseItem>());
        //Proxy Properties
        public static PropertyItemModel ProxyNameProperty = new PropertyItemModel(new PropertyDef() { Name = "CardName" }, new IdeCollection<IdeBaseItem>());
        public static PropertyItemModel SizeNameProperty = new PropertyItemModel(new PropertyDef() { Name = "CardSizeName" }, new IdeCollection<IdeBaseItem>());
        public static PropertyItemModel SizeHeightProperty = new PropertyItemModel(new PropertyDef() { Name = "CardSizeHeight" }, new IdeCollection<IdeBaseItem>());
        public static PropertyItemModel SizeWidthProperty = new PropertyItemModel(new PropertyDef() { Name = "CardSizeWidth" }, new IdeCollection<IdeBaseItem>());

        public RelayCommand AddCommand { get; private set; }

        public PropertyTabViewModel()
        {
            Items = new IdeCollection<IdeBaseItem>(this, typeof(PropertyItemModel));
            foreach (var property in ViewModelLocator.GameLoader.Game.CardProperties.Values)
            {
                Items.Add(new PropertyItemModel(property, Items));
            }
            Items.CollectionChanged += (sender, args) =>
            {
                ViewModelLocator.GameLoader.Game.CardProperties = Items.ToDictionary(x => ((PropertyItemModel)x).Name, y => ((PropertyItemModel)y).Property);
                Messenger.Default.Send(new CustomPropertyChangedMessage(args));
            };
            AddCommand = new RelayCommand(AddItem);
        }


        public void AddItem()
        {
            var ret = new PropertyItemModel(Items);
            Items.Add(ret);
        }
    }

}