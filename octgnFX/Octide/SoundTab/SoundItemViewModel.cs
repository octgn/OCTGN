// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System.Linq;

namespace Octide.ItemModel
{
 
    public class SoundItemViewModel : IdeListBoxItemBase
    {
        public GameSound _sound { get; private set; }
        public RelayCommand PlayCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }

        public SoundItemViewModel() // new item
        {
            PlayCommand = new RelayCommand(PlaySound);
            StopCommand = new RelayCommand(StopSound);

            _sound = new GameSound();

            Asset = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Sound);

        }

        public SoundItemViewModel(GameSound s) // load item
        {
            PlayCommand = new RelayCommand(PlaySound);
            StopCommand = new RelayCommand(StopSound);

            _sound = s;
        }

        public SoundItemViewModel(SoundItemViewModel s) // copy item
        {
            PlayCommand = new RelayCommand(PlaySound);
            StopCommand = new RelayCommand(StopSound);

            _sound = new GameSound()
            {
                Name = Utils.GetUniqueName(s.Name, s.ItemSource.Select(x => (x as SoundItemViewModel).Name)),
                Gameid = s._sound.Gameid,
                Src = s.Asset.FullPath
            };
            ItemSource = s.ItemSource;
            Parent = s.Parent;
        }

        public string Name
        {
            get
            {
                return _sound.Name;
            }
            set
            {
                if (_sound.Name == value) return;
                if (string.IsNullOrEmpty(value)) return;
                _sound.Name = Utils.GetUniqueName(value, ItemSource.Select(x => (x as SoundItemViewModel).Name));
                RaisePropertyChanged("Name");
            }
        }

        public Asset Asset
        {
            get
            {
                return Asset.Load(_sound.Src);
            }
            set
            {
                _sound.Src = value?.FullPath;
                RaisePropertyChanged("Asset");
            }
        }

        public override object Clone()
        {
            return new SoundItemViewModel(this);
        }
        
        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as SoundItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new SoundItemViewModel() { Parent = Parent, ItemSource = ItemSource, Name = "Property" });
        }

        public void PlaySound()
        {
            ViewModelLocator.SoundTabViewModel.PlaySound(Asset);
        }
        public void StopSound()
        {
            ViewModelLocator.SoundTabViewModel.StopSound();
        }
    }
}
