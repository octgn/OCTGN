// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.IO;
using System.Threading.Tasks;

namespace Octide
{
    using System;
    using System.Collections.Generic;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;

    using Octgn.DataNew;
    using Octgn.DataNew.Entities;
    using Octgn.Library;
    using NuGet.Packaging;
    using NuGet.Versioning;
    using System.IO.Compression;
    using Octide.ViewModel;
    using Octide.SetTab.ItemModel;
    using Octide.ItemModel;
    using Microsoft.Win32;
    using System.Linq;
    using System.Windows;
    using System.Diagnostics;

    public class GameLoader : ViewModelBase
    {
        private Game game;
        private IEnumerable<string> events;

        private bool needsSave;

        private bool livesInTempDirectory;

        public AssetController Asset { get; set; }

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

        private DirectoryInfo _workingDirectory;

        public DirectoryInfo WorkingDirectory
        {
            get
            {
                return _workingDirectory;
            }
            set
            {
                if (_workingDirectory != null && _workingDirectory.FullName.Equals(value.FullName)) return;
                _workingDirectory = value;
                game.InstallPath = value.FullName;
                game.Filename = Asset.FullPath;
                ViewModelLocator.AssetsTabViewModel.UpdateWorkingDirectory(value);
            }
        }


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

		public GameLoader()
        {
        }
         
        public void CreateGame()
        {
            _workingDirectory = Directory.CreateDirectory(Path.Combine(Config.Instance.Paths.GraveyardPath, "Game"));

            var resourcePath = Path.Combine(_workingDirectory.FullName, "Assets");

            Directory.CreateDirectory(resourcePath);

            //load in some sample assets

            var back = Path.Combine(resourcePath, "back.png");
            Properties.Resources.back.Save(back);
            ViewModelLocator.AssetsTabViewModel.LoadAsset(new FileInfo(back));

            var front = Path.Combine(resourcePath, "front.png");
            Properties.Resources.front.Save(front);
            ViewModelLocator.AssetsTabViewModel.LoadAsset(new FileInfo(front));

            var hand = Path.Combine(resourcePath, "hand.png");
            Properties.Resources.hand.Save(hand);
            ViewModelLocator.AssetsTabViewModel.LoadAsset(new FileInfo(hand));

            var deck = Path.Combine(resourcePath, "deck.png");
            Properties.Resources.deck.Save(deck);
            ViewModelLocator.AssetsTabViewModel.LoadAsset(new FileInfo(deck));

            var score = Path.Combine(resourcePath, "score.png");
            Properties.Resources.score.Save(score);
            ViewModelLocator.AssetsTabViewModel.LoadAsset(new FileInfo(score));

            var background = Path.Combine(resourcePath, "background.jpg");
            Properties.Resources.background.Save(background);
            ViewModelLocator.AssetsTabViewModel.LoadAsset(new FileInfo(background));
            
            Game = new Game()
            {
                Id = Guid.NewGuid(),
                Name = "My Game",
                Version = new Version(1, 0, 0, 0),
                ScriptVersion = new Version(3, 1, 0, 2),
                OctgnVersion = typeof(Config).Assembly.GetName().Version,
                NoteBackgroundColor = "#FFEBE8C5",
                NoteForegroundColor = "#FF000000",
                Filename = Path.Combine(WorkingDirectory.FullName, "definition.xml"),
                InstallPath = WorkingDirectory.FullName
            };
            Game.CardSizes.Add("", new CardSize
            {
                Name = "Default",
                Height = 88,
                Width = 63,
                Back = back,
                Front = front
            });

            var gameAsset = ViewModelLocator.AssetsTabViewModel.NewAsset(new string[] { }, "definition", ".xml");
            gameAsset.IsReserved = true;
            Asset = new AssetController(gameAsset);

            livesInTempDirectory = true;
        }


		public void ImportGame(FileInfo package)
		{
            //TODO: Extracts into a temp folder, but then it needs to save to a proper location via prompt as well
            livesInTempDirectory = true;
            var ExtractLocation = Config.Instance.Paths.GraveyardPath;
            ZipFile.ExtractToDirectory(package.FullName, ExtractLocation);
            var definition = Path.Combine(ExtractLocation, "definition.xml");
            Asset = new AssetController(AssetType.Xml, definition);

            if (File.Exists(definition))
            {
                LoadGame(new FileInfo(definition));
            }
		}


        public void LoadGame(FileInfo path)
        {
            NeedsSave = false;

            _workingDirectory = path.Directory;

            var files = WorkingDirectory.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var asset = ViewModelLocator.AssetsTabViewModel.LoadAsset(file);
                if (file.FullName == path.FullName)
                {
                    // registers the game definition asset

                    var gameSerializer = new GameSerializer();
                    Game = (Game)gameSerializer.Deserialize(path.FullName);
                    Asset = new AssetController(asset);
                    asset.LockName = true;
                    asset.IsReserved = true;
                }
            }
        }

        public bool ChooseSaveLocation()
        {
            // this dialog creates a new folder for the game at the specified directory
            var gameLocationDlg = new SaveFileDialog
            {
                FileName = Game.Name,
                Title = "Choose a location to save the game files.",
                Filter = "Folder|*.",
                AddExtension = false
            };
            if (gameLocationDlg.ShowDialog() == false)
            {
                return false;
            }
            if (File.Exists(gameLocationDlg.FileName))
            {
                if (System.Windows.Forms.MessageBox.Show("Warning: Files in this location may be overwritten.  Continue?", "Overwrite Existing Location?")
                     != System.Windows.Forms.DialogResult.Yes)
                {
                    return false;
                }
            }
            else
            {
                System.IO.Directory.CreateDirectory(gameLocationDlg.FileName);
            }
            WorkingDirectory = new DirectoryInfo(gameLocationDlg.FileName);
            livesInTempDirectory = false;
            return true;
        }


        public void SaveGame()
        {
            if (livesInTempDirectory)
            {
                if (ChooseSaveLocation() == false)
                    return;
            }
            //   if (!NeedsSave)
            //       return;

            bool saveAllAssets = true;

            if (ViewModelLocator.AssetsTabViewModel.Assets.Any(x => x.IsLinked == false))
            {
                var result = MessageBox.Show("Unlinked assets detected! Click Yes to save all assets, No to save linked assets only, or Cancel to cancel saving.", "Asset Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.No:
                        saveAllAssets = false;
                        break;
                    case MessageBoxResult.Cancel:
                        return;
                }
            }

            if (!SerializeXmlAssets())
            {
                return;
            }

            ViewModelLocator.AssetsTabViewModel.Watcher.EnableRaisingEvents = false;

            foreach (var asset in ViewModelLocator.AssetsTabViewModel.Assets.Where(x => saveAllAssets || x.IsLinked))
            {
                if (asset.TargetFile != null && asset.TargetFile.FullName != asset.FullPath)
                {
                    asset.TargetFile.Refresh();
                    if (asset.TargetFile.Exists)
                    {
                        asset.TargetFile.Delete();
                    }
                }
                Directory.CreateDirectory(Path.GetDirectoryName(asset.FullPath));
                asset.TargetFile = asset.SafeFile.CopyTo(asset.FullPath, true);
            }


            NeedsSave = false;
            ViewModelLocator.AssetsTabViewModel.Watcher.EnableRaisingEvents = true;
        }

        public bool SerializeXmlAssets()
        {
            try
            {
                var gameTempPath = Path.Combine(ViewModelLocator.AssetsTabViewModel.AssetTempDirectory.FullName, Guid.NewGuid().ToString() + ".xml");

                var gameSerializer = new GameSerializer();
                gameSerializer.OutputPath = gameTempPath;
                gameSerializer.Serialize(Game);
                Asset.SelectedAsset.SafeFile = new FileInfo(gameTempPath);

                var setSerializer = new SetSerializer() { Game = Game };
                foreach (SetModel set in ViewModelLocator.SetTabViewModel.Items)
                {
                    var setTempPath = Path.Combine(ViewModelLocator.AssetsTabViewModel.AssetTempDirectory.FullName, Guid.NewGuid().ToString() + ".xml");
                    setSerializer.OutputPath = setTempPath;
                    setSerializer.Serialize(set._set);
                    set.Asset.SelectedAsset.SafeFile = new FileInfo(setTempPath);
                }
                var scriptSerializer = new GameScriptSerializer(Game.Id) { Game = Game };
                foreach (ScriptItemModel script in ViewModelLocator.ScriptsTabViewModel.Scripts)
                {
                    scriptSerializer.OutputPath = script.Asset.SafePath;
                    scriptSerializer.Serialize(script._script);
                }

                var proxyTempPath = Path.Combine(ViewModelLocator.AssetsTabViewModel.AssetTempDirectory.FullName, Guid.NewGuid().ToString() + ".xml");

                var proxySerializer = new ProxyGeneratorSerializer(Game.Id) { Game = Game};
                proxySerializer.OutputPath = proxyTempPath;
                proxySerializer.Serialize(ViewModelLocator.ProxyTabViewModel._proxydef);
                ViewModelLocator.ProxyTabViewModel.Asset.SelectedAsset.SafeFile = new FileInfo(proxyTempPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ExportGame()
        {

            if (!SerializeXmlAssets())
            {
                return;
            }

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

            var baseRefPath = "\\def";


            foreach (var asset in ViewModelLocator.AssetsTabViewModel.Assets.Where(x => x.IsLinked))
            {
                    var refpath = baseRefPath + "\\" + asset.RelativePath;
                    var pf = new PhysicalPackageFile() { SourcePath = asset.SafeFilePath, TargetPath = refpath };
                    builder.Files.Add(pf);
            }

            var feedPath = Path.Combine(WorkingDirectory.FullName, Game.Name + '-' + Game.Version + ".nupkg");
            var filestream = File.Open(feedPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            builder.Save(filestream);
            filestream.Flush(true);
            filestream.Close();
            Process.Start(WorkingDirectory.FullName);

        }

        public void GameChanged(object sender)
        {
            RaisePropertyChanged("Game");
            Messenger.Default.Send(new PropertyChangedMessage<Game>(sender, null, this.game, "Game"));
            NeedsSave = true;
        }
    }
}