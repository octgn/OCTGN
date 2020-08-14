// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using Octgn.DataNew.Entities;
using Octide.Messages;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Octide.ItemModel
{
    public class GameModeItemModel : IdeBaseItem
    {
        public GameMode _gameMode;
        public AssetController Asset { get; set; }

        public GameModeItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _gameMode = new GameMode();
            Name = "New Game Mode";
            Asset = new AssetController(AssetType.Image);
            Asset.PropertyChanged += AssetUpdated;
            _gameMode.Image = Asset.FullPath;
        }

        public GameModeItemModel(GameMode g, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _gameMode = g;
            Asset = new AssetController(AssetType.Image);
            Asset.Register(g.Image);
            Asset.PropertyChanged += AssetUpdated;
        }

        public GameModeItemModel(GameModeItemModel g, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _gameMode = new GameMode();
            Asset = new AssetController(AssetType.Image);
            Asset.Register(g._gameMode.Image);
            Asset.PropertyChanged += AssetUpdated;
            _gameMode.Image = Asset.FullPath;
            Name = g.Name;
            UseTwoSidedTable = g.UseTwoSidedTable;
            ShortDescription = g.ShortDescription;
            PlayerCount = g.PlayerCount;
        }

        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _gameMode.Image = Asset.FullPath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Icon");
            }
        }
        public override void Cleanup()
        {
            Asset.SelectedAsset = null;
            base.Cleanup();
        }

        public override object Clone()
        {
            return new GameModeItemModel(this, Source);
        }
        public override object Create()
        {
            return new GameModeItemModel(Source);
        }

        public string Name
        {
            get
            {
                return _gameMode.Name;
            }
            set
            {
                if (_gameMode.Name == value) return;
                _gameMode.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public new string Icon => Asset.SafePath;


        public int PlayerCount
        {
            get
            {
                return _gameMode.PlayerCount;
            }
            set
            {
                if (_gameMode.PlayerCount == value) return;
                _gameMode.PlayerCount = value;
                RaisePropertyChanged("PlayerCount");
            }
        }
        public bool UseTwoSidedTable
        {
            get
            {
                return _gameMode.UseTwoSidedTable;
            }
            set
            {
                if (_gameMode.UseTwoSidedTable == value) return;
                _gameMode.UseTwoSidedTable = value;
                RaisePropertyChanged("UseTwoSidedTable");
            }
        }
        public string ShortDescription
        {
            get
            {
                return _gameMode.ShortDescription;
            }
            set
            {
                if (_gameMode.ShortDescription == value) return;
                _gameMode.ShortDescription = value;
                RaisePropertyChanged("ShortDescription");
            }
        }
    }
}
