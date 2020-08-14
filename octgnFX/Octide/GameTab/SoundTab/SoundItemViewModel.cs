// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Octide.ItemModel
{
 
    public class SoundItemModel : IdeBaseItem
    {
        public GameSound _sound { get; private set; }
        public RelayCommand PlayCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }
        public AssetController Asset { get; set; }

        public SoundItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            PlayCommand = new RelayCommand(PlaySound);
            StopCommand = new RelayCommand(StopSound);

            _sound = new GameSound();
            Name = "New Sound";

            Asset = new AssetController(AssetType.Sound);
            _sound.Src = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;

        }

        public SoundItemModel(GameSound s, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            PlayCommand = new RelayCommand(PlaySound);
            StopCommand = new RelayCommand(StopSound);

            _sound = s;
            Asset = new AssetController(AssetType.Sound);
            Asset.Register(s.Src);
            Asset.PropertyChanged += AssetUpdated;
        }

        public SoundItemModel(SoundItemModel s, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            PlayCommand = new RelayCommand(PlaySound);
            StopCommand = new RelayCommand(StopSound);

            _sound = new GameSound()
            {
                Gameid = s._sound.Gameid,
            };
            Asset = new AssetController(AssetType.Sound);
            Asset.Register(s._sound.Src);
            _sound.Src = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            Name = s.Name;
        }

        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _sound.Src = Asset.FullPath;
                RaisePropertyChanged("Asset");
            }
        }
        public override void Cleanup()
        {
            Asset.SelectedAsset = null;
            base.Cleanup();
        }

        public override object Clone()
        {
            return new SoundItemModel(this, Source);
        }
        public override object Create()
        {
            return new SoundItemModel(Source);
        }

        public IEnumerable<string> UniqueNames => Source.Select(x => ((SoundItemModel)x).Name);

        public string Name
        {
            get
            {
                return _sound.Name;
            }
            set
            {
                if (_sound.Name == value) return;
                _sound.Name = Utils.GetUniqueName(value, UniqueNames);
                RaisePropertyChanged("Name");
            }
        }


        public void PlaySound()
        {
            if (Asset.SelectedAsset != null)
               ViewModelLocator.SoundTabViewModel.PlaySound(Asset.SelectedAsset);
        }
        public void StopSound()
        {
            ViewModelLocator.SoundTabViewModel.StopSound();
        }
    }
}
