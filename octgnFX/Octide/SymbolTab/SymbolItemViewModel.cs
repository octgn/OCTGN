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
    public class SymbolItemViewModel : IdeListBoxItemBase
    {
        public Symbol _symbol;

        public SymbolItemViewModel() // new item
        {
            _symbol = new Symbol
            {
                Id = Utils.GetUniqueName("Symbol", ItemSource.Select(x => (x as SymbolItemViewModel).Id)),
                Name = "Symbol",
                //Source = ViewModelLocator.GameLoader.GamePath
                Source = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.Image)?.FullPath,
            };
        }

        public SymbolItemViewModel(Symbol s) // load item
        {
            _symbol = s;
        }

        public SymbolItemViewModel(SymbolItemViewModel s) // copy item
        {
            _symbol = new Symbol()
            {
                Id = Utils.GetUniqueName(s.Id, ItemSource.Select(x => (x as SymbolItemViewModel).Name)),
                Name = s.Name,
                Source = s.Asset.FullPath
            };
            Parent = s.Parent;
            ItemSource = s.ItemSource;
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

        public string Id
        {
            get
            {
                return _symbol.Id;
            }
            set
            {
                if (_symbol.Id == value) return;
                if (string.IsNullOrEmpty(value)) return;
                _symbol.Id = Utils.GetUniqueName(value, ItemSource.Select(x => (x as SymbolItemViewModel).Id));
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

        public override object Clone()
        {
            return new SymbolItemViewModel(this);
        }

        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as SymbolItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new SymbolItemViewModel() { Parent = Parent, ItemSource = ItemSource });
        }
    }
}
