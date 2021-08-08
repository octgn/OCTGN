// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Octide.ItemModel
{
    public class GameEventItemModel : IdeBaseItem
    {
        public GameEvent _gameEvent;

        public GameEventItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _gameEvent = new GameEvent()
            {
                Name = "",
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
                Name = gameEvent.Event.Name,
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


        public GameEventDescriptionItemModel Event
        {
            get
            {
                return RegisteredEvents.FirstOrDefault(x => x.Name == _gameEvent.Name);
            }
            set
            {
                if (_gameEvent.Name == value.Name) return;
                _gameEvent.Name = value.Name;
                RaisePropertyChanged(nameof(Event));
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
                RaisePropertyChanged(nameof(PythonFunction));
            }
        }

        public List<PythonFunctionDefItemModel> PythonFunctions => ViewModelLocator.PythonTabViewModel.PythonFunctions;
        public GameEventDescriptionItemModel[] RegisteredEvents => ViewModelLocator.GameEventTabViewModel.RegisteredEvents;

    }

    public class GameEventDescriptionItemModel : ViewModelBase
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Version ApiVersion { get; set; }
        public List<GameEventArgumentItemModel> Arguments { get; set; }
        public GameEventDescriptionItemModel()
        {
            Arguments = new List<GameEventArgumentItemModel>();
        }
    }

    public class GameEventArgumentItemModel : ViewModelBase
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
