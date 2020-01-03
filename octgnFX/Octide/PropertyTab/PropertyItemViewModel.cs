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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Octide.ItemModel
{
    public class PropertyItemViewModel : IdeListBoxItemBase
    {
        public PropertyDef _property { get; set; }

        public new ObservableCollection<PropertyItemViewModel> ItemSource { get; set; }
        public PropertyItemViewModel()
        {
            _property = new PropertyDef();
        }

        public PropertyItemViewModel(PropertyDef p)
        {
            _property = p;
        }

        public PropertyItemViewModel(PropertyItemViewModel p)
        {
            ItemSource = p.ItemSource;
            Parent = p.Parent;
            _property = p._property.Clone() as PropertyDef;
            _property.Name = Utils.GetUniqueName(p.Name, ItemSource.Select(x => x.Name));
        }

        public override object Clone()
        {
            return new PropertyItemViewModel(this);
        }
        
        public override void Copy()
        {
            if (CanCopy == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, Clone() as PropertyItemViewModel);
        }

        public override void Insert()
        {
            if (CanInsert == false) return;
            var index = ItemSource.IndexOf(this);
            ItemSource.Insert(index, new PropertyItemViewModel() { Parent = Parent, ItemSource = ItemSource, Name = "Property" });
        }
       
        public string Name
        {
            get
            {
                return _property.Name;
            }
            set
            {
                if (value == _property.Name) return;
                if (string.IsNullOrEmpty(value)) return;
                _property.Name = Utils.GetUniqueName(value, ItemSource.Select(x => x.Name));
                RaisePropertyChanged("Name");
                Messenger.Default.Send(new CustomPropertyChangedMessage() { Prop = this, Action = PropertyChangedMessageAction.Modify});
            }
        }

        public PropertyType Type
        {
            get
            {
                return _property.Type;
            }
            set
            {
                if (value == _property.Type) return;
                _property.Type = value;
                RaisePropertyChanged("Type");
            }
        }

        public PropertyTextKind TextKind
        {
            get
            {
                return _property.TextKind;
            }
            set
            {
                if (value == _property.TextKind) return;
                _property.TextKind = value;
                RaisePropertyChanged("TextKind");
            }
        }

        public bool Hidden
        {

            get
            {
                return _property.Hidden;
            }
            set
            {
                if (value == _property.Hidden) return;
                _property.Hidden = value;
                RaisePropertyChanged("Hidden");
            }
        }
        public bool IgnoreText
        {

            get
            {
                return _property.IgnoreText;
            }
            set
            {
                if (value == _property.IgnoreText) return;
                _property.IgnoreText = value;
                RaisePropertyChanged("IgnoreText");
            }
        }
    }
}
