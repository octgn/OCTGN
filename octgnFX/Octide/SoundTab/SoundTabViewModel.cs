// /* This Source Code Form is subject to the terms of the Mozilla Public
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
        private SoundItemViewModel _selectedItem;
        public ObservableCollection<IdeListBoxItemBase> Items { get; private set; }

        public RelayCommand AddCommand { get; private set; }

        private MediaPlayer _mediaPlayer;

        public SoundTabViewModel()
        {
            AddCommand = new RelayCommand(AddItem);
            _mediaPlayer = new MediaPlayer();

            Items = new ObservableCollection<IdeListBoxItemBase>();
            foreach (var sound in ViewModelLocator.GameLoader.Game.Sounds)
            {
                Items.Add(new SoundItemViewModel(sound.Value) { ItemSource = Items });
            }
            Items.CollectionChanged += (a, b) =>
            {
                ViewModelLocator.GameLoader.Game.Sounds = Items.ToDictionary(x => (x as SoundItemViewModel).Name, y => (y as SoundItemViewModel)._sound);
            };
        }
        
        public SoundItemViewModel SelectedItem
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
            var ret = new SoundItemViewModel() { ItemSource = Items, Name = "Sound" };
            Items.Add(ret);
            SelectedItem = ret;
        }
        
        public void PlaySound(Asset sound)
        {
            _mediaPlayer.Open(new Uri(sound.FullPath));

            _mediaPlayer.Play();
        }
        
        public void StopSound()
        {

            _mediaPlayer.Stop();
        }

    }

}