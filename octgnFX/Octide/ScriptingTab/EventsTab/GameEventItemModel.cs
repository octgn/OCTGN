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
    public class GameEventItemModel : IdeBaseItem
    {
        public GameEvent _gameEvent;

        public GameEventItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _gameEvent = new GameEvent()
            {
                Name = "Event",
                PythonFunction = ""
            };
        }

        public GameEventItemModel(GameEvent gameEvent, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _gameEvent = gameEvent;
        }

        public GameEventItemModel(GameEventItemModel gameEvent, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _gameEvent = new GameEvent()
            {
                Name = gameEvent.Name,
                PythonFunction = gameEvent.PythonFunction.Name
            };
        }

        public override object Clone()
        {
            return new GameEventItemModel(this, Source);
        }
        public override object Create()
        {
            return new GameEventItemModel(Source);
        }

        public string Name
        {
            get
            {
                return _gameEvent.Name;
            }
            set
            {
                if (_gameEvent.Name == value) return;
                _gameEvent.Name = value;
                RaisePropertyChanged("Name");
            }
        }
        public PythonFunctionDefItemModel PythonFunction
        {
            get
            {
                return PythonFunctions.FirstOrDefault(x => x.Name == _gameEvent.PythonFunction);
            }
            set
            {
                if (_gameEvent.PythonFunction == value.Name) return;
                _gameEvent.PythonFunction = value.Name;
                RaisePropertyChanged("PythonFunction");
            }
        }

        public List<PythonFunctionDefItemModel> PythonFunctions => ViewModelLocator.PythonTabViewModel.PythonFunctions;

    }
}
