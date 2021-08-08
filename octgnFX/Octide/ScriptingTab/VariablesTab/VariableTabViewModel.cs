// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Octide.ItemModel;
using System.Linq;

namespace Octide.ViewModel
{
    public class VariableTabViewModel : ViewModelBase
    {
        public IdeCollection<IdeBaseItem> GlobalVariables { get; private set; }
        public RelayCommand AddGlobalVariableCommand { get; private set; }

        public IdeCollection<IdeBaseItem> PlayerVariables { get; private set; }
        public RelayCommand AddPlayerVariableCommand { get; private set; }

        public VariableTabViewModel()
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
        }

        public void AddGlobalVariable()
        {
            var ret = new GlobalVariableItemModel(GlobalVariables);
            GlobalVariables.Add(ret);
        }
        public void AddPlayerVariable()
        {
            var ret = new GlobalVariableItemModel(PlayerVariables);
            PlayerVariables.Add(ret);
        }
    }
}