// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GongSolutions.Wpf.DragDrop;
using GongSolutions.Wpf.DragDrop.Utilities;
using Octgn.DataNew.Entities;
using Octide.ItemModel;
using Octide.Messages;

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

            Items = new IdeCollection<IdeBaseItem>(this);
            foreach (var deckSection in ViewModelLocator.GameLoader.Game.DeckSections.Values)
            {
                Items.Add(new DeckSectionItemModel(deckSection, Items));
            }
            Items.CollectionChanged += (sender, args) =>
            {
                ViewModelLocator.GameLoader.Game.DeckSections = Items.ToDictionary(x => ((DeckSectionItemModel)x).Name, y => ((DeckSectionItemModel)y)._deckSection);
            };
            GlobalItems = new IdeCollection<IdeBaseItem>(this);
            foreach (var globalDeckSection in ViewModelLocator.GameLoader.Game.SharedDeckSections.Values)
            {
                GlobalItems.Add(new DeckSectionItemModel(globalDeckSection, GlobalItems));
            }
            GlobalItems.CollectionChanged += (sender, args) =>
            {
                ViewModelLocator.GameLoader.Game.SharedDeckSections = GlobalItems.ToDictionary(x => ((DeckSectionItemModel)x).Name, y => ((DeckSectionItemModel)y)._deckSection);
            };

            DeckEditorFont = new GameFontItemModel(ViewModelLocator.GameLoader.Game.DeckEditorFont);
            ContextFont = new GameFontItemModel(ViewModelLocator.GameLoader.Game.ContextFont);
            NotesFont = new GameFontItemModel(ViewModelLocator.GameLoader.Game.NoteFont);
            ChatFont = new GameFontItemModel(ViewModelLocator.GameLoader.Game.ChatFont);

            EnterChatCommand = new RelayCommand(EnterChat);
            EscChatCommand = new RelayCommand(EscapeChat);

            Menu = new List<Control>();
            var header = new MenuItem();
            header.Header = "Sample Menu";
            header.SetResourceReference(UserControl.StyleProperty, "MenuHeader");
            Menu.Add(header);
            Menu.Add(new Separator());
            Menu.Add(new MenuItem { Header = "bb" });

            Messenger.Default.Register<PropertyChangedMessage<Game>>(this,
                x =>
                {
                    RaisePropertyChanged(string.Empty);
                });
            Messenger.Default.Register<GroupChangedMessage>(this, x =>
                {
                    RaisePropertyChanged("HasPiles");
                    RaisePropertyChanged("HasGlobalPiles");
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

        #region game fonts

        public RelayCommand EnterChatCommand { get; set; }
        public RelayCommand EscChatCommand { get; set; }
        public GameFontItemModel DeckEditorFont { get; set; }
        public GameFontItemModel ContextFont { get; set; }
        public GameFontItemModel ChatFont { get; set; }
        public GameFontItemModel NotesFont { get; set; }

        public string Chat { get; set; }

        public string _chatInputText;
        public string ChatInputText
        {
            get
            {
                return _chatInputText;
            }
            set
            {
                if (_chatInputText == value) return;
                _chatInputText = value;
                RaisePropertyChanged("ChatInputText");
            }
        }

        public void EnterChat()
        {
            if (!string.IsNullOrWhiteSpace(ChatInputText))
            {
                if (Chat == null) Chat = "";
                Chat = Chat + "<Player> " + ChatInputText + "\n";
                RaisePropertyChanged("Chat");
            }
            ChatInputText = null;
        }

        public void EscapeChat()
        {
            ChatInputText = null;
        }

        public void UpdateFonts()
        {
            ViewModelLocator.GameLoader.Game.DeckEditorFont = DeckEditorFont._font;
            ViewModelLocator.GameLoader.Game.ContextFont = ContextFont._font;
            ViewModelLocator.GameLoader.Game.NoteFont = NotesFont._font;
            ViewModelLocator.GameLoader.Game.ChatFont = ChatFont._font;
        }

        #endregion

        public List<Control> Menu { get; set; }

        #region deck sections
        public IdeCollection<IdeBaseItem> Items { get; private set; }
        public IdeCollection<IdeBaseItem> GlobalItems { get; private set; }

        public bool HasPiles => (ViewModelLocator.PreviewTabViewModel.Piles.Count > 0);
        public bool HasGlobalPiles => (ViewModelLocator.PreviewTabViewModel.GlobalPiles.Count > 0);

        public RelayCommand AddCommand { get; private set; }
        public RelayCommand AddGlobalCommand { get; private set; }
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
        #endregion
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