// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Linq;
using System.ComponentModel;
using System.Windows;
using System.IO;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

using Octide.Messages;
using Octgn.DataNew.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GongSolutions.Wpf.DragDrop;
using System.Collections.Specialized;
using Octide.ItemModel;
using Octgn.DataNew;

namespace Octide.ViewModel
{
    public class GameEventTabViewModel : ViewModelBase
    {
        public IdeCollection<IdeBaseItem> Events { get; private set; }
        public RelayCommand AddEventCommand { get; private set; }

        public GameEventTabViewModel()
        {
            Events = new IdeCollection<IdeBaseItem>();
            foreach (var gameEventType in ViewModelLocator.GameLoader.Game.Events)
            {
                foreach (var gameEvent in gameEventType.Value)
                {
                    Events.Add(new GameEventItemModel(gameEvent, Events));
                }
            }

            Events.CollectionChanged += (a, b) =>
            {
                var items = 
                ViewModelLocator.GameLoader.Game.Events = Events.GroupBy(x => ((GameEventItemModel)x).Name)
                                                                .ToDictionary(
                                                                    x => x.Key,
                                                                    y => y.Select(z => ((GameEventItemModel)z)._gameEvent).ToArray());
            };
            AddEventCommand = new RelayCommand(AddGameEvent);

        }

        public void AddGameEvent()
        {
            var ret = new GameEventItemModel(Events);
            Events.Add(ret);
            Events.SelectedItem = ret;
        }
    }

}