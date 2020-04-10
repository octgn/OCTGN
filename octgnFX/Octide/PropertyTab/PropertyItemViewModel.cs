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
    public class PropertyItemModel : IdeBaseItem
    {
        public PropertyDef _property { get; set; }

        public PropertyItemModel(IdeCollection<IdeBaseItem> source) : base(source)
        {
            _property = new PropertyDef();
            Name = "New Property";
        }

        public PropertyItemModel(PropertyDef p, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _property = p;
        }

        public PropertyItemModel(PropertyItemModel p, IdeCollection<IdeBaseItem> source) : base(source)
        {
            _property = p._property.Clone() as PropertyDef;
            Name = p.Name;
        }

        public override object Clone()
        {
            return new PropertyItemModel(this, Source);
        }
        public override object Create()
        {
            return new PropertyItemModel(Source);
        }

        public IEnumerable<string> UniqueNames => Source.Select(x => ((PropertyItemModel)x).Name);

        public string Name
        {
            get
            {
                return _property.Name;
            }
            set
            {
                if (value == _property.Name) return;
                _property.Name = Utils.GetUniqueName(value, UniqueNames);
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
