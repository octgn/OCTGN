// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Linq;

namespace Octide.ItemModel
{
    public class GlobalVariableItemModel : IdeBaseItem
    {
        public GlobalVariable _globalVariable;

        public GlobalVariableItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _globalVariable = new GlobalVariable()
            {
                Value = ""
            };
            Name = "newVar";
        }

        public GlobalVariableItemModel(GlobalVariable globalvariable, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _globalVariable = globalvariable;
        }

        public GlobalVariableItemModel(GlobalVariableItemModel globalvariable, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _globalVariable = new GlobalVariable();
            Name = globalvariable.Name;
            DefaultValue = globalvariable.DefaultValue;
        }

        public override object Clone()
        {
            return new GlobalVariableItemModel(this, Source);
        }
        public override object Create()
        {
            return new GlobalVariableItemModel(Source);
        }

        public IEnumerable<string> UniqueNames => Source.Select(x => ((GlobalVariableItemModel)x).Name);

        public string Name
        {
            get
            {
                return _globalVariable.Name;
            }
            set
            {
                if (_globalVariable.Name == value) return;
                _globalVariable.Name = Utils.GetUniqueName(value, UniqueNames);
                RaisePropertyChanged("Name");
            }
        }
        public string DefaultValue
        {
            get
            {
                return _globalVariable.Value;
            }
            set
            {
                if (_globalVariable.Value == value) return;
                _globalVariable.Value = value;
                RaisePropertyChanged("DefaultValue");
            }
        }

    }
}
