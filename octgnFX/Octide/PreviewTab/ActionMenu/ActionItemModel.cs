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
        }

        public ActionItemModel(ActionItemModel a, IdeCollection<IdeBaseItem> source) : base(source) //copy item
        {
            CanBeDefault = true;
            _action = new GroupAction
            {
                DefaultAction = ((GroupAction)a._action).DefaultAction,
                Execute = ((GroupAction)a._action).Execute,
                IsBatchExecutable = a.Batch,
                IsGroup = a.IsGroup,
                Name = a.Name,
                Shortcut = a.Shortcut,
                HeaderExecute = a._action.HeaderExecute,
                ShowExecute = a._action.ShowExecute
            };
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
                return PythonFunctions.FirstOrDefault(x => x.Name == ((GroupAction)_action).Execute);
            }
            set
            {
                if (((GroupAction)_action).Execute == value.Name) return;
                ((GroupAction)_action).Execute = value.Name;
                RaisePropertyChanged("Execute");
            }
        }


        public bool Batch
        {
            get
            {
                return ((GroupAction)_action).IsBatchExecutable;
            }
            set
            {
                if (((GroupAction)_action).IsBatchExecutable == value) return;
                ((GroupAction)_action).IsBatchExecutable = value;
                RaisePropertyChanged("Batch");
            }
        }
    }
}
