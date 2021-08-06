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
    public class PythonTabViewModel : ViewModelBase
    {
        public IdeCollection<IdeBaseItem> Scripts { get; private set; }
        public RelayCommand AddScriptCommand { get; private set; }

        public PythonTabViewModel()
        {
            Scripts = new IdeCollection<IdeBaseItem>();
            var game = ViewModelLocator.GameLoader.Game;
            var scriptSerializer = new GameScriptSerializer(game.Id) { Game = game };

            foreach (var scriptPath in game.Scripts)
            {
                var path = Path.Combine(ViewModelLocator.GameLoader.WorkingDirectory.FullName, scriptPath);
                var script = (GameScript)scriptSerializer.Deserialize(path);
                var scriptModel = new PythonItemModel(script, Scripts);
                Scripts.Add(scriptModel);
            }

            Scripts.CollectionChanged += (a, b) =>
            {
                //     ViewModelLocator.GameLoader.Scripts = Scripts.Select(x => ((ScriptItemModel)x)._script);
            };
            AddScriptCommand = new RelayCommand(AddScript);
        }

        public void AddScript()
        {
            var ret = new PythonItemModel(Scripts);
            Scripts.Add(ret);
            Scripts.SelectedItem = ret;
        }
        public List<PythonFunctionDefItemModel> PythonFunctions
        {
            get
            {
                var ret = new List<PythonFunctionDefItemModel>();
                foreach (PythonItemModel script in Scripts)
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