﻿// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Octide.ViewModel
{
    public class GameFontTabViewModel : ViewModelBase
    {
        public GameFontTabViewModel()
        {
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



        }

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
            Keyboard.ClearFocus();
        }

        public List<Control> Menu { get; set; }

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
    }
}