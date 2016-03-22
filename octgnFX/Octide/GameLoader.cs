using System.IO;
using System.Threading.Tasks;

namespace Octide
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Messaging;

    using Octgn.DataNew.Entities;
    using Octgn.DataNew.FileDB;

    public class GameLoader : ViewModelBase
    {
        private Game game;
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
            New();
        }

        public void New()
        {
            var id = Guid.NewGuid();
            var path = Path.Combine(Octgn.Library.Config.Instance.DataDirectory, "IdeDevDatabase", id.ToString());
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
            definition.LoadXml(Properties.Resources.definition.Replace("{GUID}", id.ToString()).Replace("{OCTVER}", typeof(Octgn.Library.Config).Assembly.GetName().Version.ToString()));
            definition.Save(defPath);

            XmlDocument proxydef = new XmlDocument();
            definition.LoadXml(Properties.Resources.proxydef);
            definition.Save(Path.Combine(resourcePath, "proxydef.xml"));

            LoadGame(defPath);
            NeedsSave = true;
            DidManualSave = false;
        }

        public void LoadGame(string filename)
        {
            NeedsSave = false;
            DidManualSave = true;
            var s = new Octgn.DataNew.GameSerializer();
            var conf = new FileDbConfiguration();
            conf.DefineCollection<Game>("Game");
            s.Def = new CollectionDefinition<Game>(conf, "IdeDevDatabase");
            try
            {
                GamePath = new FileInfo(filename).Directory.FullName;
                Game = (Game)s.Deserialize(filename);
            }
            catch (System.Exception)
            {
                GamePath = "";
            }
        }

        public void SaveGame()
        {
         //   if (!NeedsSave)
         //       return;
            var s = new Octgn.DataNew.GameSerializer();
            s.Serialize(Game);
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