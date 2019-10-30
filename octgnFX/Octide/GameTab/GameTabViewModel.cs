// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Octgn.DataNew.Entities;
using Octide.ItemModel;

namespace Octide.ViewModel
{
    public class GameTabViewModel : ViewModelBase
    {
        public DeckSectionDropHandler DeckSectionDropHandler { get; set; }
        public GameTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);
            AddGlobalCommand = new RelayCommand(AddGlobalItem);
            DeckSectionDropHandler = new DeckSectionDropHandler();

            Items = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var deckSection in ViewModelLocator.GameLoader.Game.DeckSections.Values)
            {
                Items.Add(new DeckSectionItemViewModel(deckSection) { ItemSource = Items });
            }
            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.DeckSections = Items.Cast<DeckSectionItemViewModel>().ToDictionary(x => x.Name, y => y._deckSection);
            };
            GlobalItems = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var globalDeckSection in ViewModelLocator.GameLoader.Game.SharedDeckSections.Values)
            {
                GlobalItems.Add(new DeckSectionItemViewModel(globalDeckSection) { ItemSource = Items });
            }
            GlobalItems.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.SharedDeckSections = GlobalItems.Cast<DeckSectionItemViewModel>().ToDictionary(x => x.Name, y => y._deckSection);
            };

            DeckEditorFont = new GameFontItemModel(ViewModelLocator.GameLoader.Game.DeckEditorFont);
            ContextFont = new GameFontItemModel(ViewModelLocator.GameLoader.Game.ContextFont);
            NotesFont = new GameFontItemModel(ViewModelLocator.GameLoader.Game.NoteFont);
            ChatFont = new GameFontItemModel(ViewModelLocator.GameLoader.Game.ChatFont);

            Messenger.Default.Register<PropertyChangedMessage<Game>>(this,
                x =>
                {
                    this.RaisePropertyChanged(string.Empty);
                });


        }
        public string Name
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.Name;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.Name) return;
                ViewModelLocator.GameLoader.Game.Name = value;
				RaisePropertyChanged("Name");
				ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string Description
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.Description;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.Description) return;
                ViewModelLocator.GameLoader.Game.Description = value;
                RaisePropertyChanged("Description");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string GameUrl
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.GameUrl;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.GameUrl) return;
                ViewModelLocator.GameLoader.Game.GameUrl= value;
                RaisePropertyChanged("GameUrl");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string IconUrl
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.IconUrl;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.IconUrl) return;
                ViewModelLocator.GameLoader.Game.IconUrl = value;
                RaisePropertyChanged("IconUrl");
				ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public int MarkerSize
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.MarkerSize;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.MarkerSize) return;
                ViewModelLocator.GameLoader.Game.MarkerSize = value;
                RaisePropertyChanged("MarkerSize");
				ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string Version
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.Version.ToString();
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.Version.ToString()) return;
                if (System.Version.TryParse(value, out Version v))
                {
                    ViewModelLocator.GameLoader.Game.Version = v;
                }
                RaisePropertyChanged("Version");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string Authors
        {
            get
            {
                return String.Join(", ",ViewModelLocator.GameLoader.Game.Authors);
            }
            set
            {
                var list = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x=>!String.IsNullOrWhiteSpace(x)).ToList();
                ViewModelLocator.GameLoader.Game.Authors = list;
                RaisePropertyChanged("Authors");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public string Tags
        {
            get
            {
                return String.Join(", ", ViewModelLocator.GameLoader.Game.Tags);
            }
            set
            {
                var list = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
                ViewModelLocator.GameLoader.Game.Tags = list;
                RaisePropertyChanged("Tags");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public bool UseTwoSidedTable
        {
            get
            {
                return ViewModelLocator.GameLoader.Game.UseTwoSidedTable;
            }
            set
            {
                if (value == ViewModelLocator.GameLoader.Game.UseTwoSidedTable) return;
                ViewModelLocator.GameLoader.Game.UseTwoSidedTable = value;
                RaisePropertyChanged("UseTwoSidedTable");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public Color NoteBackgroundColor
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString(ViewModelLocator.GameLoader.Game.NoteBackgroundColor);
            }
            set
            {
                ViewModelLocator.GameLoader.Game.NoteBackgroundColor = new ColorConverter().ConvertToString(value);
                RaisePropertyChanged("NoteBackgroundColor");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }

        public Color NoteForegroundColor
        {
            get
            {
                return (Color)ColorConverter.ConvertFromString(ViewModelLocator.GameLoader.Game.NoteForegroundColor);
            }
            set
            {
                ViewModelLocator.GameLoader.Game.NoteForegroundColor = new ColorConverter().ConvertToString(value);
                RaisePropertyChanged("NoteForegroundColor");
                ViewModelLocator.GameLoader.GameChanged(this);
            }
        }


        public GameFontItemModel DeckEditorFont { get; set; }
        public GameFontItemModel ContextFont { get; set; }
        public GameFontItemModel ChatFont { get; set; }
        public GameFontItemModel NotesFont { get; set; }


        public void UpdateFonts()
        {
            ViewModelLocator.GameLoader.Game.DeckEditorFont = DeckEditorFont._font;
            ViewModelLocator.GameLoader.Game.ContextFont = ContextFont._font;
            ViewModelLocator.GameLoader.Game.NoteFont = NotesFont._font;
            ViewModelLocator.GameLoader.Game.ChatFont = ChatFont._font;
        }


        public ObservableCollection<IdeListBoxItemBase> Items { get; private set; }
        public ObservableCollection<IdeListBoxItemBase> GlobalItems { get; private set; }

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand AddGlobalCommand { get; private set; }
        public void AddItem()
        {
            var ret = new DeckSectionItemViewModel() { ItemSource = Items, Name = "Section", Global = false };
            Items.Add(ret);
        }
        public void AddGlobalItem()
        {
            var ret = new DeckSectionItemViewModel() {ItemSource = GlobalItems, Name = "Section", Global = true };
            GlobalItems.Add(ret);
        }

    }
    public class DeckSectionDropHandler : IDropTarget
    {
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