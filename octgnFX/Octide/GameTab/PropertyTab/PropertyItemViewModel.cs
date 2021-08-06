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
        public PropertyDef Property { get; set; }

        public PropertyItemModel(IdeCollection<IdeBaseItem> source) : base(source)
        {
            Property = new PropertyDef();
            Name = "New Property";
        }

        public PropertyItemModel(PropertyDef p, IdeCollection<IdeBaseItem> source) : base(source)
        {
            Property = p;
        }

        public PropertyItemModel(PropertyItemModel p, IdeCollection<IdeBaseItem> source) : base(source)
        {
            Property = p.Property.Clone() as PropertyDef;
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
                return Property.Name;
            }
            set
            {
                if (value == Property.Name) return;
                Property.Name = Utils.GetUniqueName(value, UniqueNames);
                RaisePropertyChanged("Name");
                Messenger.Default.Send(new CustomPropertyChangedMessage() { Prop = this, Action = PropertyChangedMessageAction.Modify });
            }
        }

        public PropertyType Type
        {
            get
            {
                return Property.Type;
            }
            set
            {
                if (value == Property.Type) return;
                Property.Type = value;
                RaisePropertyChanged("Type");
            }
        }

        public PropertyTextKind TextKind
        {
            get
            {
                return Property.TextKind;
            }
            set
            {
                if (value == Property.TextKind) return;
                Property.TextKind = value;
                RaisePropertyChanged("TextKind");
            }
        }

        public bool Hidden
        {

            get
            {
                return Property.Hidden;
            }
            set
            {
                if (value == Property.Hidden) return;
                Property.Hidden = value;
                RaisePropertyChanged("Hidden");
            }
        }
        public bool IgnoreText
        {

            get
            {
                return Property.IgnoreText;
            }
            set
            {
                if (value == Property.IgnoreText) return;
                Property.IgnoreText = value;
                RaisePropertyChanged("IgnoreText");
            }
        }
    }
}
