// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
using Octide.ItemModel;
using Octide.Messages;
using System.Linq;
using System.Windows;

namespace Octide.ViewModel
{
    public class DeckSectionTabViewModel : ViewModelBase
    {

        public IdeCollection<IdeBaseItem> Items { get; private set; }
        public IdeCollection<IdeBaseItem> GlobalItems { get; private set; }

        public bool HasPiles => ViewModelLocator.PreviewTabViewModel.Piles.Count > 0;
        public bool HasGlobalPiles => ViewModelLocator.PreviewTabViewModel.GlobalPiles.Count > 0;

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand AddGlobalCommand { get; private set; }
        public DeckSectionDropHandler DeckSectionDropHandler { get; set; }
        public DeckSectionTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);
            AddGlobalCommand = new RelayCommand(AddGlobalItem);
            DeckSectionDropHandler = new DeckSectionDropHandler();

            Items = new IdeCollection<IdeBaseItem>(this, typeof(DeckSectionItemModel));
            foreach (var deckSection in ViewModelLocator.GameLoader.Game.DeckSections.Values)
            {
                Items.Add(new DeckSectionItemModel(deckSection, Items));
            }
            Items.CollectionChanged += (sender, args) =>
            {
                ViewModelLocator.GameLoader.Game.DeckSections = Items.ToDictionary(x => ((DeckSectionItemModel)x).Name, y => ((DeckSectionItemModel)y)._deckSection);
            };
            GlobalItems = new IdeCollection<IdeBaseItem>(this, typeof(DeckSectionItemModel));
            foreach (var globalDeckSection in ViewModelLocator.GameLoader.Game.SharedDeckSections.Values)
            {
                GlobalItems.Add(new DeckSectionItemModel(globalDeckSection, GlobalItems));
            }
            GlobalItems.CollectionChanged += (sender, args) =>
            {
                ViewModelLocator.GameLoader.Game.SharedDeckSections = GlobalItems.ToDictionary(x => ((DeckSectionItemModel)x).Name, y => ((DeckSectionItemModel)y)._deckSection);
            };


            Messenger.Default.Register<GroupChangedMessage>(this, UpdateGroups);

        }

        private void UpdateGroups(GroupChangedMessage m)
        {
            RaisePropertyChanged("HasPiles");
            RaisePropertyChanged("HasGlobalPiles");
        }

        public void AddItem()
        {
            var ret = new DeckSectionItemModel(Items);
            Items.Add(ret);
        }
        public void AddGlobalItem()
        {
            var ret = new DeckSectionItemModel(GlobalItems);
            GlobalItems.Add(ret);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Messenger.Default.Unregister<GroupChangedMessage>(this, UpdateGroups);
        }
    }

    public class DeckSectionDropHandler : IDropTarget
    {
        public void DragEnter(IDropInfo dropInfo) {
            
        }

        public void DragLeave(IDropInfo dropInfo) {
            
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.VisualSource != dropInfo.VisualTarget)
            {
                dropInfo.Effects = DragDropEffects.None;
            }
            else
            {
                GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.DragOver(dropInfo);
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            GongSolutions.Wpf.DragDrop.DragDrop.DefaultDropHandler.Drop(dropInfo);
        }
    }
}