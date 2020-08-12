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
    public class ScriptsTabViewModel : ViewModelBase
    {
        public IdeCollection<IdeBaseItem> GlobalVariables { get; private set; }
        public RelayCommand AddGlobalVariableCommand { get; private set; }
        
        public IdeCollection<IdeBaseItem> PlayerVariables { get; private set; }
        public RelayCommand AddPlayerVariableCommand { get; private set; }

        public IdeCollection<IdeBaseItem> Scripts { get; private set; }
        public RelayCommand AddScriptCommand { get; private set; }

        public IdeCollection<IdeBaseItem> Events { get; private set; }
        public RelayCommand AddEventCommand { get; private set; }

        public ScriptsTabViewModel()
        {
            GlobalVariables = new IdeCollection<IdeBaseItem>();
            foreach (var globalvariable in ViewModelLocator.GameLoader.Game.GlobalVariables)
            {
                GlobalVariables.Add(new GlobalVariableItemModel(globalvariable.Value, GlobalVariables));
            }
            GlobalVariables.CollectionChanged += (sender, args) =>
            {
                ViewModelLocator.GameLoader.Game.GlobalVariables = GlobalVariables.ToDictionary(
                    x => (x as GlobalVariableItemModel).Name,
                    y => (y as GlobalVariableItemModel)._globalVariable
                    );
            };
            AddGlobalVariableCommand = new RelayCommand(AddGlobalVariable);

            PlayerVariables = new IdeCollection<IdeBaseItem>();
            foreach (var globalvariable in ViewModelLocator.GameLoader.Game.Player.GlobalVariables)
            {
                PlayerVariables.Add(new GlobalVariableItemModel(globalvariable.Value, PlayerVariables));
            }
            PlayerVariables.CollectionChanged += (sender, args) =>
            {
                ViewModelLocator.GameLoader.Game.Player.GlobalVariables = PlayerVariables.ToDictionary(
                    x => (x as GlobalVariableItemModel).Name,
                    y => (y as GlobalVariableItemModel)._globalVariable
                    );
            };
            AddPlayerVariableCommand = new RelayCommand(AddPlayerVariable);

            Scripts = new IdeCollection<IdeBaseItem>();
            var game = ViewModelLocator.GameLoader.Game;
            var scriptSerializer = new GameScriptSerializer(game.Id) { Game = game };

            foreach (var scriptPath in game.Scripts)
            {
                var path = Path.Combine(ViewModelLocator.GameLoader.WorkingDirectory.FullName, scriptPath);
                var script = (GameScript)scriptSerializer.Deserialize(path);
                var scriptModel = new ScriptItemModel(script, Scripts);
                scriptModel.Asset = new AssetController(AssetType.PythonScript, path);
                scriptModel.Asset.PropertyChanged += AssetChanged;
                Scripts.Add(scriptModel);
            }

            Scripts.CollectionChanged += (a, b) =>
            {
           //     ViewModelLocator.GameLoader.Scripts = Scripts.Select(x => ((ScriptItemModel)x)._script);
            };
            AddScriptCommand = new RelayCommand(AddScript);

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

        public void AssetChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        public void AddGlobalVariable()
        {
            var ret = new GlobalVariableItemModel(GlobalVariables);
            GlobalVariables.Add(ret);
            GlobalVariables.SelectedItem = ret;
        }
        public void AddPlayerVariable()
        {
            var ret = new GlobalVariableItemModel(PlayerVariables);
            PlayerVariables.Add(ret);
            PlayerVariables.SelectedItem = ret;
        }
        public void AddScript()
        {
            var ret = new ScriptItemModel(Scripts);
            Scripts.Add(ret);
            Scripts.SelectedItem = ret;
        }
        public void AddGameEvent()
        {
            var ret = new GameEventItemModel(Events);
            Events.Add(ret);
            Events.SelectedItem = ret;
        }

        public List<PythonFunctionDefItemModel> PythonFunctions
        {
            get
            {
                var ret = new List<PythonFunctionDefItemModel>();
                foreach (ScriptItemModel script in Scripts)
                {
                    foreach (var function in script.Functions)
                    {
                        ret.Add(function);
                    }
                }
                return ret;
            }
        }
    }

}