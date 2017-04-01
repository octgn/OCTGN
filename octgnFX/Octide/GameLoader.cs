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

    public class GameLoader : ViewModelBase
    {
        private Game game;
        private IEnumerable<Set> sets;
        private ProxyDefinition proxyDef;
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

        private String gamePath;

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
            //A6C8D2E8-7CD8-11DD-8F94-E62B56D89593
            //263b2f19-fb22-4f32-82dd-3d1b28d3da3a
            if (Directory.Exists(Path.Combine(Octgn.Library.Config.Instance.DataDirectory, "GameDatabase", "263b2f19-fb22-4f32-82dd-3d1b28d3da3a")))
                LoadGame(Guid.Parse("263b2f19-fb22-4f32-82dd-3d1b28d3da3a"));
            else 
                New();
        }

        public void New()
        {
            var id = "263b2f19-fb22-4f32-82dd-3d1b28d3da3a";
            var path = Path.Combine(Octgn.Library.Config.Instance.DataDirectory, "GameDatabase", id.ToString());
            var defPath = Path.Combine(path, "definition.xml");
            var setPath = Path.Combine(path, "Sets", "071443fe-b5d1-43b4-bb3a-284c4f6d52ce" );
            var resourcePath = Path.Combine(path, "Resources");

            Directory.CreateDirectory(path);
            Directory.CreateDirectory(resourcePath);
            Directory.CreateDirectory(setPath);
            
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
            definition.LoadXml(Properties.Resources.definition.Replace("{GUID}", id.ToString()).Replace("{OCTVER}", typeof(Octgn.Library.Config).Assembly.GetName().Version.ToString()));
            definition.Save(defPath);

            XmlDocument set = new XmlDocument();
            set.LoadXml(Properties.Resources.set.Replace("{GAMEID}", id.ToString()));
            set.Save(Path.Combine(setPath, "set.xml"));

            XmlDocument proxydef = new XmlDocument();
            definition.LoadXml(Properties.Resources.proxydef);
            definition.Save(Path.Combine(resourcePath, "proxydef.xml"));

            LoadGame(Guid.Parse(id));
            NeedsSave = true;
            DidManualSave = false;
        }
        

        public void LoadGame(string filename)
        {
            //TODO deal with database BS
        }

        public void LoadGame(Guid guid)
        {
            NeedsSave = false;
            DidManualSave = true;


            var g = new Octgn.DataNew.GameSerializer();
            var conf = new FileDbConfiguration();
            conf.DefineCollection<Game>("Game");
            g.Def = new CollectionDefinition<Game>(conf, "GameDatabase");
            try
            {
                Game = DbContext.Get().GameById(guid);
                GamePath = Game.InstallPath;
                Sets = DbContext.Get().SetQuery.By(x => x.GameId, Op.Eq, Game.Id);
                ProxyDef = Game.GetCardProxyDef();
            }
            catch (System.Exception)
            {
                GamePath = "";
            }
        }

/*
            var g = new Octgn.DataNew.GameSerializer();
            g.Def = new CollectionDefinition<Game>(conf, "IdeDevDatabase");
            try
            {
                GamePath = new FileInfo(filename).Directory.FullName;
                Game = (Game)g.Deserialize(filename);
                Sets = Directory.EnumerateFiles(Path.GetDirectoryName(filename), "set.xml", SearchOption.AllDirectories).Select(x =>
                {
                    var s = new Octgn.DataNew.SetSerializer();
                    s.Def = new CollectionDefinition<Set>(conf, "IdeDevDatabase");
                    return (Set)s.Deserialize(x);
                    }
                );
            }
        } */

        public void SaveGame()
        {
         //   if (!NeedsSave)
         //       return;
            var g = new Octgn.DataNew.GameSerializer();
            g.Serialize(Game);
            var s = new Octgn.DataNew.SetSerializer();
            foreach(Set set in this.Sets)
            {
                s.Serialize(set);
            }
            NeedsSave = false;
            DidManualSave = true;
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