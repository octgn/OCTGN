// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Octide.ItemModel
{


    public class ActionItemModel : IBaseAction
    {
        public bool _batch;

        public ActionItemModel(IdeCollection<IdeBaseItem> source) : base(source) //new item
        {
            CanBeDefault = true;
            _action = new GroupAction();
            Name = "New Action";
        }

        public ActionItemModel(GroupAction a, IdeCollection<IdeBaseItem> source) : base(source) //load item
        {
            CanBeDefault = true;
            _action = a;
            if (a.DefaultAction) IsDefault = true;
            if (a.BatchExecute != null) _batch = true;
        }

        public ActionItemModel(ActionItemModel a, IdeCollection<IdeBaseItem> source) : base(source) //copy item
        {
            CanBeDefault = true;
            _action = new GroupAction
            {
                BatchExecute = ((GroupAction)a._action).BatchExecute,
                DefaultAction = ((GroupAction)a._action).DefaultAction,
                Execute = ((GroupAction)a._action).Execute,
                HeaderExecute = a._action.HeaderExecute,
                IsGroup = a.IsGroup,
                Name = a.Name,
                Shortcut = a.Shortcut,
                ShowExecute = a._action.HeaderExecute
            };
            if (a.Batch)
                _batch = true;
        }

        public override object Clone()
        {
            return new ActionItemModel(this, Source);
        }
        public override object Create()
        {
            return new ActionItemModel(Source);
        }


        public string Shortcut
        {
            get
            {
                return ((GroupAction)_action).Shortcut;
            }
            set
            {
                if (value == ((GroupAction)_action).Shortcut) return;
                ((GroupAction)_action).Shortcut = value;
                RaisePropertyChanged("Shortcut");
            }
        }
        public PythonFunctionDefItemModel Execute
        {
            get
            {
                if (Batch == true)
                {
                    return PythonFunctions.FirstOrDefault(x => x.Name == ((GroupAction)_action).BatchExecute);
                }
                else
                {
                    return PythonFunctions.FirstOrDefault(x => x.Name == ((GroupAction)_action).Execute);
                }
            }
            set
            {
                if (Batch == true)
                {
                    if (((GroupAction)_action).BatchExecute == value.Name) return;
                    ((GroupAction)_action).BatchExecute = value.Name;
                }
                else
                {
                    if (((GroupAction)_action).Execute == value.Name) return;
                    ((GroupAction)_action).Execute = value.Name;
                }
                RaisePropertyChanged("Execute");
            }
        }


        public bool Batch
        {
            get
            {
                return _batch;
            }
            set
            {
                if (value == _batch) return;
                _batch = value;
                if (value == true)
                {
                    ((GroupAction)_action).BatchExecute = ((GroupAction)_action).Execute;
                    ((GroupAction)_action).Execute = null;
                }
                else
                {
                    ((GroupAction)_action).Execute = ((GroupAction)_action).Execute;
                    ((GroupAction)_action).BatchExecute = null;
                }
                RaisePropertyChanged("Batch");
                RaisePropertyChanged("Execute");
            }
        }
    }
}
