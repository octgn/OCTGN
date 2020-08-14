﻿// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Linq;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using System.Collections.ObjectModel;
using Octide.ItemModel;
using System.Windows.Media;

namespace Octide.ViewModel
{
    public class SoundTabViewModel : ViewModelBase
    {
        public IdeCollection<IdeBaseItem> Items { get; private set; }

        public RelayCommand AddCommand { get; private set; }

        private MediaPlayer _mediaPlayer;

        public SoundTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);
            _mediaPlayer = new MediaPlayer();

            Items = new IdeCollection<IdeBaseItem>(this);
            foreach (var sound in ViewModelLocator.GameLoader.Game.Sounds)
            {
                Items.Add(new SoundItemModel(sound.Value, Items));
            }
            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.Sounds = Items.ToDictionary(x => (x as SoundItemModel).Name, y => (y as SoundItemModel)._sound);
            };
        }
        
        public void AddItem()
        {
            var ret = new SoundItemModel(Items);
            Items.Add(ret);
            Items.SelectedItem = ret;
        }
        
        public void PlaySound(Asset sound)
        {
            _mediaPlayer.Open(new Uri(sound.SafeFilePath));

            _mediaPlayer.Play();
        }
        
        public void StopSound()
        {

            _mediaPlayer.Stop();
        }

    }

}