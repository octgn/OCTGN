// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.IO;
using System.Threading.Tasks;

namespace Octide
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;
    using Microsoft.Win32;
    using NuGet.Packaging;
    using NuGet.Versioning;
    using Octgn.DataNew;
    using Octgn.DataNew.Entities;
    using Octgn.Library;
    using Octide.ItemModel;
    using Octide.SetTab.ItemModel;
    using Octide.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO.Compression;
    using System.Linq;
    using System.Windows;

    public class GameLoader : ViewModelBase
    {
        private Game game;
        private IEnumerable<string> events;

        private bool needsSave;

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
            var _tempDirectory = Directory.CreateDirectory(Path.Combine(Config.Instance.Paths.GraveyardPath, "Game"));

            var resourcePath = Path.Combine(_tempDirectory.FullName, "Assets");

            Directory.CreateDirectory(resourcePath);

            Game = new Game()
            {
                Id = Guid.NewGuid(),
                Name = "My Game",
                Version = new Version(1, 0, 0, 0),
                ScriptVersion = new Version(3, 1, 0, 2),
                OctgnVersion = typeof(Config).Assembly.GetName().Version,
                Authors = new List<string>() { "OCTIDE" },
                Description = "A game created using OCTGN Game Development Studio",
                NoteBackgroundColor = "#FFEBE8C5",
                NoteForegroundColor = "#FF000000"
            };
            Game.Table = new Group()
            {
             //   Background = background,
                Name = "Table",
                Width = 640,
                Height = 480
            };

            var gameAsset = ViewModelLocator.AssetsTabViewModel.LoadExternalAsset(new FileInfo("dummy/definition.xml"), new string[] { });
            gameAsset.LockName = true;
            gameAsset.IsReserved = true;
            Asset = new AssetController(AssetType.Xml);
            Asset.SelectedAsset = gameAsset;
            Asset.PropertyChanged += AssetUpdated;
        }


        public void ImportGame(FileInfo package)
        {
            //TODO: Extracts into a temp folder, but then it needs to save to a proper location via prompt as well
            var ExtractLocation = Config.Instance.Paths.GraveyardPath;

            ZipFile.ExtractToDirectory(package.FullName, ExtractLocation);
            var definition = Path.Combine(ExtractLocation, "def", "definition.xml");

            if (File.Exists(definition))
            {
                LoadGame(new FileInfo(definition));
            }
            else
            {
                // throw an invalid game exception if it can't find the game xml
            }
        }


        public void LoadGame(FileInfo path)
        {
            NeedsSave = false;
            ViewModelLocator.AssetsTabViewModel.WorkingDirectory = path.Directory;

            var files = path.Directory.GetFiles("*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var asset = ViewModelLocator.AssetsTabViewModel.LoadInternalAsset(file);
                if (file.FullName == path.FullName)
                {
                    // registers the game definition asset

                    var gameSerializer = new GameSerializer();
                    Game = (Game)gameSerializer.Deserialize(path.FullName);
                    Asset = new AssetController(AssetType.Xml);
                    asset.LockName = true;
                    asset.IsReserved = true;
                    Asset.SelectedAsset = asset;
                    Asset.PropertyChanged += AssetUpdated;
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
                Directory.CreateDirectory(gameLocationDlg.FileName);
            }
            ViewModelLocator.AssetsTabViewModel.WorkingDirectory = new DirectoryInfo(gameLocationDlg.FileName);
            return true;
        }


        public void SaveGame()
        {
            if (ViewModelLocator.AssetsTabViewModel.WorkingDirectory == null)
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
                if (asset.FileLocationPath != null && asset.FileLocationPath != asset.FullPath)
                {
                    var file = new FileInfo(asset.FileLocationPath);
                    if (file.Exists)
                        file.Delete();
                }
                Directory.CreateDirectory(Path.GetDirectoryName(asset.FullPath));
                asset.FileLocationPath = asset.SafeFile.CopyTo(asset.FullPath, true).FullName;
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
                foreach (PythonItemModel script in ViewModelLocator.PythonTabViewModel.Scripts)
                {
                    scriptSerializer.OutputPath = script.Asset.SafePath;
                    scriptSerializer.Serialize(script._script);
                }

                var proxyTempPath = Path.Combine(ViewModelLocator.AssetsTabViewModel.AssetTempDirectory.FullName, Guid.NewGuid().ToString() + ".xml");

                var proxySerializer = new ProxyGeneratorSerializer(Game.Id) { Game = Game };
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
                Description = Game.Description ?? "A placeholder description.  Don't forget to include one!",
                ProjectUrl = new Uri(Game.GameUrl ?? "https://www.octgn.net"),
                IconUrl = new Uri(Game.IconUrl ?? "https://github.com/octgn/OCTGN/raw/b69fe2cca3b337acc138491ee3f647a2a69fbda5/octgnFX/Octide/Resources/icon.jpg"),
                Version = new NuGetVersion(Game.Version),
                Title = Game.Name
            };
            foreach (var a in Game.Authors) builder.Authors.Add(a);
            if (builder.Authors.Count == 0) builder.Authors.Add("OCTIDE");
            foreach (var t in Game.Tags) builder.Authors.Add(t);

            var baseRefPath = "\\def";


            foreach (var asset in ViewModelLocator.AssetsTabViewModel.Assets.Where(x => x.IsLinked))
            {
                var refpath = baseRefPath + "\\" + asset.RelativePath;
                var pf = new PhysicalPackageFile() { SourcePath = asset.SafeFilePath, TargetPath = refpath };
                builder.Files.Add(pf);
            }

            var feedPath = Path.Combine(ViewModelLocator.AssetsTabViewModel.WorkingDirectory.FullName, Game.Name + '-' + Game.Version + ".nupkg");
            var filestream = File.Open(feedPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            builder.Save(filestream);
            filestream.Flush(true);
            filestream.Close();
            Process.Start(ViewModelLocator.AssetsTabViewModel.WorkingDirectory.FullName);

        }

        private void AssetUpdated(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Path")
            {
                Game.Filename = Asset.FullPath;
                RaisePropertyChanged("Asset");
            }
        }

        public void GameChanged(object sender)
        {
            RaisePropertyChanged("Game");
            Messenger.Default.Send(new PropertyChangedMessage<Game>(sender, null, this.game, "Game"));
            NeedsSave = true;
        }
    }
}