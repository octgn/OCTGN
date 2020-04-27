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
 
    public partial class ScriptAssetItemModel : IdeBaseItem
    {
        private GameScript _script;

        public ScriptAssetItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _script = new GameScript()
            {
                GameId = ViewModelLocator.GameLoader.Game.Id,
             //   Path = ViewModelLocator.GameLoader.GamePath
                Path = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.PythonScript)?.FullPath
            };

        }

        public ScriptAssetItemModel(GameScript s, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _script = s;
        }

        public ScriptAssetItemModel(ScriptAssetItemModel s, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _script = new GameScript()
            {
                Script = s._script.Script,
                Path = s._script.Path,
                GameId = s._script.GameId
            };
        }

        public Asset Asset
        {
            get
            {
                return Asset.Load(_script.Path);
            }
            set
            {
                _script.Path = value?.FullPath;
                RaisePropertyChanged("Asset");
            }
        }
    }
}
