// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using Octgn.DataNew.Entities;
using Octide.ViewModel;
using System.Collections.Generic;
using System.Linq;

namespace Octide.ItemModel
{
    public class ScriptItemModel : IdeBaseItem
    {
        public GameScript _script;

        public ScriptItemModel(IdeCollection<IdeBaseItem> source) : base(source) // new item
        {
            _script = new GameScript()
            {
                GameId = ViewModelLocator.GameLoader.Game.Id,
                //  Path = ViewModelLocator.GameLoader.GamePath
                //  Path = AssetManager.Instance.Assets.FirstOrDefault(x => x.Type == AssetType.PythonScript)?.FullPath
                //  TODO: Create a new path for the asset
            };

        }

        public ScriptItemModel(GameScript gameScript, IdeCollection<IdeBaseItem> source) : base(source) // load item
        {
            _script = gameScript;
            ScriptDocument = new TextDocument(gameScript.Script);
        }

        public ScriptItemModel(ScriptItemModel gameScript, IdeCollection<IdeBaseItem> source) : base(source) // copy item
        {
            _script = new GameScript()
            {
                Script = gameScript._script.Script,
                Path = gameScript._script.Path,
                GameId = gameScript._script.GameId
            };
        }
        public override object Clone()
        {
            return new ScriptItemModel(this, Source);
        }
        public override object Create()
        {
            return new ScriptItemModel(Source);
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

        public string Name
        {
            get
            {
                return Asset.FullFileName;
            }
            set
            {
                //TODO: Change Filename
            }
        }



        public List<PythonFunctionDefItemModel> Functions = new List<PythonFunctionDefItemModel>();

        public TextDocument _scriptDocument;
        public TextDocument ScriptDocument
        {
            get
            {
                return _scriptDocument;
            }
            set
            {
                _scriptDocument = value;
                _script.Script = value.Text;
                ParseFunctions();
                RaisePropertyChanged("ScriptDocument");
            }
        }

        public void ParseFunctions()
        {
            var lines = ScriptDocument.Text.Split(new[] { '\r', '\n' });
            foreach (var line in lines)
            {
                if (line.StartsWith("def ") && line.EndsWith(":"))
                {
                    Functions.Add(new PythonFunctionDefItemModel(line));
                }
            }
        }
    }

    public class PythonFunctionDefItemModel
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }

        public PythonFunctionDefItemModel(string line)
        {
            var function = line.Substring(3).Trim();
            Name = function.Substring(0, function.IndexOf("("));
            int paramstart = function.IndexOf("(") + 1;
            int paramend = function.LastIndexOf(")");
            var paramstring = function.Substring(paramstart, paramend - paramstart);
            Parameters = new List<string>();
            foreach (string param in paramstring.Split(','))
            {
                Parameters.Add(param.Trim());
            }
        }

    }

}

