// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.IO;
using System.Threading.Tasks;

namespace Octide
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using System.Linq;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;

    using Octgn.DataNew;
    using Octgn.DataNew.Entities;
    using Octgn.DataNew.FileDB;
    using Octgn.Core.DataExtensionMethods;
    using Octgn.Library;
    using Octgn.ProxyGenerator;
    using System.Windows;
    using System.Collections.ObjectModel;
    using NuGet.Packaging;
    using NuGet.Versioning;
    using System.Xml.Linq;
    using System.IO.Compression;
    using Octide.ViewModel;
    using System.ComponentModel;
    using Octide.SetTab.ItemModel;
    using Octide.ItemModel;

    public class GameLoader : ViewModelBase
    {
        private Game game;
        private IEnumerable<string> events;
        private string directory;

        private string tempPath;

        private bool needsSave;

        public string Directory
        {
            get
            {
                return this.directory;
            }
            set
            {
                if (value.Equals(this.directory)) return;
                this.directory = value;
                this.RaisePropertyChanged("Directory");
            }
        }

        public Game Game
        {
            get
            {
                return this.game;
            }
            set
            {
                if (value == this.game) return;
                this.game = value;
                this.RaisePropertyChanged("Game");
                this.RaisePropertyChanged("ValidGame");
                Task.Factory.StartNew(() => Messenger.Default.Send(new PropertyChangedMessage<Game>(this, this.game, value,
                    "Game")));
            }
        }

        public AssetController Asset { get; private set; }


        public IEnumerable<string> Events
        {
            get
            {
                return this.Events;
            }
            set
            {
                if (value == this.Events) return;
                this.Events = value;
                this.RaisePropertyChanged("Events");
                Task.Factory.StartNew(() => Messenger.Default.Send(new PropertyChangedMessage<IEnumerable<string>>(this, this.Events, value, "Events")));

            }
        }

        public String TempPath
        {
            get
            {
                return this.tempPath;
            }
            set
            {
                if (value.Equals(this.tempPath)) return;
                this.tempPath = value;
                this.RaisePropertyChanged("TempPath");
            }
        }

        public bool ValidGame
        {
            get
            {
                return Game != null && Game.Table != null;
            }
        }

        public bool NeedsSave
        {
            get
            {
                return needsSave;
            }
            set
            {
                if (value == needsSave) return;
                needsSave = value;
                this.RaisePropertyChanged("Game");
                this.RaisePropertyChanged("NeedsSave");
                Messenger.Default.Send(new PropertyChangedMessage<bool>(this, value, value, "NeedsSave"));
            }
        }

        public bool DidManualSave { get; set; }

		public GameLoader()
        {
            TempPath = Path.Combine(Config.Instance.Paths.GraveyardPath, "IDE-" + Guid.NewGuid());
        }

        public void CreateGame(DirectoryInfo directory)
        {
            var id = Guid.NewGuid();
            var path = directory.FullName;
            var defPath = Path.Combine(path, "definition.xml");
            var resourcePath = Path.Combine(path, "Resources");

            System.IO.Directory.CreateDirectory(path);
            System.IO.Directory.CreateDirectory(resourcePath);

            var bg = Properties.Resources.background;
            bg.Save(Path.Combine(resourcePath, "background.jpg"));
            bg.Dispose();
            var cback = Properties.Resources.back;
            cback.Save(Path.Combine(resourcePath, "back.png"));
            cback.Dispose();
            var cfront = Properties.Resources.front;
            cfront.Save(Path.Combine(resourcePath, "front.png"));
            cfront.Dispose();
            var pblank = Properties.Resources.blank;
            pblank.Save(Path.Combine(resourcePath, "blank.png"));
            pblank.Dispose();
            var hand = Properties.Resources.hand;
            hand.Save(Path.Combine(resourcePath, "hand.png"));
            hand.Dispose();
            var deck = Properties.Resources.deck;
            deck.Save(Path.Combine(resourcePath, "deck.png"));
            deck.Dispose();
            var score = Properties.Resources.score;
            score.Save(Path.Combine(resourcePath, "score.png"));
            score.Dispose();

            XmlDocument definition = new XmlDocument();
            definition.LoadXml(Properties.Resources.definition
                .Replace("{GUID}", id.ToString())
                .Replace("{OCTVER}", typeof(Config).Assembly.GetName().Version.ToString()));
            definition.Save(defPath);
            

            definition.LoadXml(Properties.Resources.proxydef);
            definition.Save(Path.Combine(resourcePath, "proxydef.xml"));
            LoadGame(new FileInfo(defPath));
        }


		public void ImportGame(FileInfo package)
		{
            //TODO: Extracts into a temp folder, but then it needs to save to a proper location via prompt as well
            ZipFile.ExtractToDirectory(package.FullName, TempPath);
            TempPath = Path.Combine(TempPath, "def");
            var definition = Path.Combine(TempPath, "definition.xml");
            if (File.Exists(definition))
            {
                LoadGame(new FileInfo(definition));
            }
		}


        public void LoadGame(FileInfo path)
        {
            NeedsSave = false;
            DidManualSave = true;

            Directory = path.DirectoryName;
            ViewModelLocator.AssetsTabViewModel.CollectAssets();

            var gameSerializer = new GameSerializer();
            Game = (Game)gameSerializer.Deserialize(path.FullName);
            Asset = new AssetController(AssetType.Xml, path.FullName);
            Asset.PropertyChanged += AssetChanged;
        }

        public void AssetChanged(object sender, PropertyChangedEventArgs e)
        {
            //TODO: Watch for changes to the game XML
        }

        public void SaveGame()
        {
         //   if (!NeedsSave)
         //       return;
            var gameSerializer = new Octgn.DataNew.GameSerializer();
            gameSerializer.Serialize(Game);
            var setSerializer = new Octgn.DataNew.SetSerializer() { Game = Game };
            foreach(SetModel set in ViewModelLocator.SetTabViewModel.Items)
            {
                setSerializer.Serialize(set._set);
            }
            var scriptSerializer = new Octgn.DataNew.GameScriptSerializer(Game.Id) { Game = Game };
            foreach(ScriptItemModel script in ViewModelLocator.ScriptsTabViewModel.Scripts)
            {
                scriptSerializer.Serialize(script._script);
            }
            var proxySerializer = new Octgn.DataNew.ProxyGeneratorSerializer(Game.Id) { Game = Game };
            proxySerializer.Serialize(ViewModelLocator.ProxyTabViewModel._proxydef);
            NeedsSave = false;
            DidManualSave = true;
        }

        public void ExportGame()
        {
            var builder = new PackageBuilder()
            {
                Id = Game.Id.ToString(),
                Description = Game.Description,
                ProjectUrl = new Uri(Game.GameUrl),
                Version = new NuGetVersion(Game.Version),
                Title = Game.Name,
                IconUrl = new Uri(Game.IconUrl),

            };
            foreach (var a in Game.Authors) builder.Authors.Add(a);
            foreach (var t in Game.Tags) builder.Authors.Add(t);

            var g = new GameSerializer();
            g.Serialize(Game);

            var baseRefPath = "\\def";

            foreach (var asset in ViewModelLocator.AssetsTabViewModel.Assets)
            {
                if (asset.IsLinked)
                {
                    var refpath = baseRefPath + "\\" + asset.RelativePath;
                    var pf = new PhysicalPackageFile() { SourcePath = asset.FullPath, TargetPath = refpath };
                    builder.Files.Add(pf);
                }
            }


            var feedPath = Path.Combine(Directory, Game.Name + '-' + Game.Version + ".nupkg");
            var filestream = File.Open(feedPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            builder.Save(filestream);
            filestream.Flush(true);
            filestream.Close();

        }

        public void DeleteGame()
        {
            System.IO.Directory.Delete(Directory, true);
        }

        public void GameChanged(object sender)
        {
            RaisePropertyChanged("Game");
            Messenger.Default.Send(new PropertyChangedMessage<Game>(sender, null, this.game, "Game"));
            NeedsSave = true;
        }
    }
}