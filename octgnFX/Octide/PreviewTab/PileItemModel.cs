// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using MahApps.Metro.Controls;
using Octgn.DataNew.Entities;
using System.ComponentModel;

namespace Octide.ItemModel
{
    public class PileItemModel : BaseGroupItemModel
    {
        public AssetController Asset { get; set; }

        public PileItemModel(IdeCollection<IdeBaseItem> source) : base(source) //new item
        {
            Asset = new AssetController(AssetType.Image);
            _group.Icon = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
            RaisePropertyChanged("Asset");
            Name = "New Group";
        }

        public PileItemModel(Group g, IdeCollection<IdeBaseItem> source) : base(g, source) //load
        {
            Asset = new AssetController(AssetType.Image);
            Asset.Register(g.Icon);
            Asset.PropertyChanged += AssetUpdated;
        }

        public PileItemModel(PileItemModel g, IdeCollection<IdeBaseItem> source) : base(g, source) // copy item
        {

            _group.Shortcut = g.Shortcut?.ToString();
            _group.Ordered = g.Ordered;
            _group.MoveTo = g.MoveTo;
            _group.ViewState = g.ViewState;


            Asset = new AssetController(AssetType.Image);
            Asset.Register(g._group.Icon);
            _group.Icon = Asset.FullPath;
            Asset.PropertyChanged += AssetUpdated;
        }

        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                _group.Icon = Asset.FullPath;
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
            return new PileItemModel(this, Source);
        }
        public override object Create()
        {
            return new PileItemModel(Source);
        }

        public new string Icon => Asset.SafePath;

        public GroupViewState ViewState
        {
            get
            {
                return _group.ViewState;
            }
            set
            {
                if (_group.ViewState == value) return;
                _group.ViewState = value;
                RaisePropertyChanged("ViewState");
            }
        }

        public bool MoveTo
        {

            get
            {
                return _group.MoveTo;
            }
            set
            {
                if (value == _group.MoveTo) return;
                _group.MoveTo = value;
                RaisePropertyChanged("MoveTo");
            }
        }

        public bool Ordered
        {

            get
            {
                return _group.Ordered;
            }
            set
            {
                if (value == _group.Ordered) return;
                _group.Ordered = value;
                RaisePropertyChanged("Ordered");
            }
        }

        public HotKey Shortcut
        {
            get
            {
                return Utils.GetHotKey(_group.Shortcut);
            }
            set
            {
                var ret = value.ToString();
                if (ret == _group.Shortcut) return;
                _group.Shortcut = ret;
                RaisePropertyChanged("Shortcut");
            }
        }

        public bool Shuffle
        {
            get
            {
                return ShuffleShortcut != null;
            }
            set
            {
                if (value == Shuffle) return;
                ShuffleShortcut = value == true ? "" : null;
                RaisePropertyChanged("Shuffle");
            }
        }

        public string ShuffleShortcut
        {
            get
            {
                return _group.ShuffleShortcut;
            }
            set
            {
                if (value == _group.ShuffleShortcut) return;
                _group.ShuffleShortcut = value;
                RaisePropertyChanged("ShuffleShortcut");
            }
        }

    }

}

