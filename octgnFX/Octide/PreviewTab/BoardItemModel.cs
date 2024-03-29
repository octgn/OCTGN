﻿// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Octide.ItemModel
{
    public class BoardItemModel : IdeBaseItem
    {
        public GameBoard _board;
        public AssetController Asset { get; set; }

        public BoardItemModel(IdeCollection<IdeBaseItem> source) : base(source) //new item
        {
            CanBeDefault = true;
            _board = new GameBoard
            {
                Height = 50,
                Width = 50
            };
            Asset = new AssetController(AssetType.Image);
            _board.Source = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            Name = "New Board";
            RaisePropertyChanged("Asset");
        }

        public BoardItemModel(GameBoard g, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            CanBeDefault = true;
            _board = g;
            Asset = new AssetController(AssetType.Image);
            Asset.Register(g.Source);
            Asset.PropertyChanged += AssetUpdated;
        }

        public BoardItemModel(BoardItemModel b, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            CanBeDefault = true;
            _board = new GameBoard
            {
                Height = b.Height,
                Width = b.Width,
                XPos = b.XPos,
                YPos = b.YPos
            };
            Asset = new AssetController(AssetType.Image);
            Asset.Register(b._board.Source);
            _board.Source = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            Name = b.Name;
        }
        public override void Cleanup()
        {
            Asset.Cleanup();
            base.Cleanup();
        }

        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _board.Source = Asset.FullPath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Icon");
            }
        }

        public override object Clone()
        {
            return new BoardItemModel(this, Source);
        }

        public override object Create()
        {
            return new BoardItemModel(Source);
        }
        public IEnumerable<string> UniqueNames => Source.Select(x => ((BoardItemModel)x).Name);

        public new string Icon => Asset.SafePath;

        public string Name
        {
            get
            {
                return _board.Name;
            }
            set
            {
                if (_board.Name == value) return;
                _board.Name = Utils.GetUniqueName(value, UniqueNames);
                RaisePropertyChanged("Name");
            }
        }

        public int Height
        {
            get
            {
                return Convert.ToInt32(_board.Height);
            }
            set
            {
                if (value == _board.Height) return;
                if (value < 5) return;
                _board.Height = value;
                RaisePropertyChanged("Height");
            }
        }

        public int Width
        {
            get
            {
                return Convert.ToInt32(_board.Width);
            }
            set
            {
                if (value == _board.Width) return;
                if (value < 5) return;
                _board.Width = value;
                RaisePropertyChanged("Width");
            }
        }

        public int XPos
        {
            get
            {
                return Convert.ToInt32(_board.XPos);
            }
            set
            {
                if (value == _board.XPos) return;
                _board.XPos = value;
                RaisePropertyChanged("XPos");
            }
        }

        public int YPos
        {
            get
            {
                return Convert.ToInt32(_board.YPos);
            }
            set
            {
                if (value == _board.YPos) return;
                _board.YPos = value;
                RaisePropertyChanged("YPos");
            }
        }

        internal void RefreshValues()
        {
            RaisePropertyChanged("");
            RaisePropertyChanged("XPos");
            RaisePropertyChanged("YPos");
        }
    }
}
