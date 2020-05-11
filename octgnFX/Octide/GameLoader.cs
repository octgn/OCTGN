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

    public class GameLoader : ViewModelBase
    {
        private Game game;
        private IEnumerable<Set> sets;
        private IEnumerable<GameScript> scripts;
        private IEnumerable<string> events;
        private ProxyDefinition proxyDef;
        private string gamePath;

        private string tempPath;

        private bool needsSave;

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

        public IEnumerable<Set> Sets
        {
            get
            {
                return this.sets;
            }
            set
            {
                if (value == this.sets) return;
                this.sets = value;
                this.RaisePropertyChanged("Sets");
                Task.Factory.StartNew(() => Messenger.Default.Send(new PropertyChangedMessage<IEnumerable<Set>>(this, this.sets, value, "Sets")));
            }
        }
        public IEnumerable<GameScript> Scripts
        {
            get
            {
                return this.scripts;
            }
            set
            {
                if (value == this.scripts) return;
                this.scripts = value;
                this.RaisePropertyChanged("Scripts");
                Task.Factory.StartNew(() => Messenger.Default.Send(new PropertyChangedMessage<IEnumerable<GameScript>>(this, this.scripts, value, "Scripts")));
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

        public ProxyDefinition ProxyDef
        {
            get
            {
                return this.proxyDef;
            }
            set
            {
                if (value == this.proxyDef) return;
                this.proxyDef = value;
                this.RaisePropertyChanged("ProxyDef");
                Task.Factory.StartNew(() => Messenger.Default.Send(new PropertyChangedMessage<ProxyDefinition>(this, this.proxyDef, value, "ProxyDef")));
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
        public String GamePath
        {
            get
            {
                return this.gamePath;
            }
            set
            {
                if (value.Equals(this.gamePath)) return;
                this.gamePath = value;
                this.RaisePropertyChanged("GamePath");
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
        }

        public void CreateGame(DirectoryInfo directory)
        {
            var id = Guid.NewGuid();
            var path = directory.FullName;
            var defPath = Path.Combine(path, "definition.xml");
            var resourcePath = Path.Combine(path, "Resources");

            Directory.CreateDirectory(path);
            Directory.CreateDirectory(resourcePath);

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


		public void ImportGame(Game game)
		{

			NeedsSave = false;
			DidManualSave = true;

			Game = game;
			GamePath = Game.InstallPath;
			Sets = Game.Sets().ToList();
			Scripts = Game.GetScripts().ToList();
			ProxyDef = Game.GetCardProxyDef();
		}


        public void LoadGame(FileInfo path)
        {
            NeedsSave = false;
            DidManualSave = true;

            TempPath = Path.Combine(Config.Instance.Paths.GraveyardPath, "IDE-" + Guid.NewGuid());
            GamePath = path.DirectoryName;
            var gameSerializer = new GameSerializer();
            Game = (Game)gameSerializer.Deserialize(path.FullName);

            var setPaths = path.Directory.GetFiles("set.xml", SearchOption.AllDirectories);
            var setSerializer = new SetSerializer() { Game = Game };
            Sets = setPaths.Select(x => (Set)setSerializer.Deserialize(x.FullName));

            var scripts = new List<GameScript>();
            var scriptSerializer = new GameScriptSerializer(Game.Id);
            Scripts = Game.Scripts.Select(x => (GameScript)scriptSerializer.Deserialize(Path.Combine(GamePath, x)));

            var proxySerializer = new ProxyGeneratorSerializer(Game.Id) { Game = Game };
            ProxyDef = (ProxyDefinition)proxySerializer.Deserialize(Path.Combine(GamePath, Game.ProxyGenSource));

        }


        public void SaveGame()
        {
         //   if (!NeedsSave)
         //       return;
            var gameSerializer = new Octgn.DataNew.GameSerializer();
            gameSerializer.Serialize(Game);
            var setSerializer = new Octgn.DataNew.SetSerializer() { Game = Game };
            foreach(Set set in this.Sets)
            {
                setSerializer.Serialize(set);
            }
            var scriptSerializer = new Octgn.DataNew.GameScriptSerializer(Game.Id) { Game = Game };
            foreach(GameScript script in this.Scripts)
            {
                scriptSerializer.Serialize(script);
            }
            var proxySerializer = new Octgn.DataNew.ProxyGeneratorSerializer(Game.Id) { Game = Game };
            proxySerializer.Serialize(ProxyDef);
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


            var feedPath = Path.Combine(GamePath, Game.Name + '-' + Game.Version + ".nupkg");
            var filestream = File.Open(feedPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            builder.Save(filestream);
            filestream.Flush(true);
            filestream.Close();

        }

        public void DeleteGame()
        {
            Directory.Delete(GamePath,true);
        }

        public void GameChanged(object sender)
        {
            RaisePropertyChanged("Game");
            Messenger.Default.Send(new PropertyChangedMessage<Game>(sender, null, this.game, "Game"));
            NeedsSave = true;
        }
    }
}