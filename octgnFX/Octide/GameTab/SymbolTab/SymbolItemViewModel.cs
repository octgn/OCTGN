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
    public class SymbolItemModel : IdeBaseItem
    {
        public Symbol _symbol;

        public SymbolItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _symbol = new Symbol();
            Name = "New Symbol";
            Id = "symbol";
            Asset = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image);
        }

        public SymbolItemModel(Symbol s, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _symbol = s;
        }

        public SymbolItemModel(SymbolItemModel s, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _symbol = new Symbol()
            {
                Source = s.Asset.FullPath
            };
            Name = s.Name;
            Id = s.Id;
        }

        public override object Clone()
        {
            return new SymbolItemModel(this, Source);
        }
        public override object Create()
        {
            return new SymbolItemModel(Source);
        }

        public string Name
        {
            get
            {
                return _symbol.Name;
            }
            set
            {
                if (_symbol.Name == value) return;
                _symbol.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public IEnumerable<string> UniqueNames => Source.Select(x => ((SymbolItemModel)x).Id);

        public string Id
        {
            get
            {
                return _symbol.Id;
            }
            set
            {
                if (_symbol.Id == value) return;
                _symbol.Id = Utils.GetUniqueName(value, UniqueNames);
                RaisePropertyChanged("Id");
            }
        }

        public new string Icon => Asset?.FullPath;

        public Asset Asset
        {
            get
            {
                return Asset.Load(_symbol.Source);
            }
            set
            {
                _symbol.Source = value?.FullPath;
                RaisePropertyChanged("Asset");
                RaisePropertyChanged("Icon");
            }
        }

    }
}
