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

    public class GameLoader : ViewModelBase
    {
        private Game game;
        private IEnumerable<Set> sets;
        private ProxyDefinition proxyDef;
        private String gamePath;
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

		public ObservableCollection<Game> IdeDevDatabaseGames
		{
			get
			{
				return new ObservableCollection<Game>(DbContext.Get().Games);
			}
		}
		public GameLoader()
		{
			SetFileDb();
		}

		public void SetFileDb()
		{
			var config = new FileDbConfiguration()
				.SetDirectory(Path.Combine(Config.Instance.Paths.DataDirectory, "IdeDevDatabase"))
				.SetExternalDb()
				.DefineCollection<Game>("Game")
				.OverrideRoot(x => x.Directory(""))
				.SetPart(x => x.Property(y => y.Id))
				.SetPart(x => x.File("definition.xml"))
				.SetSerializer<GameSerializer>()
				.Conf()
				.DefineCollection<Set>("Sets")
				.OverrideRoot(x => x.Directory(""))
				.SetPart(x => x.Property(y => y.GameId))
				.SetPart(x => x.Directory("Sets"))
				.SetPart(x => x.Property(y => y.Id))
				.SetPart(x => x.File("set.xml"))
				.SetSerializer<SetSerializer>()
				.Conf()
				.DefineCollection<GameScript>("Scripts")
				.OverrideRoot(x => x.Directory(""))
				.SetSteril()
				.Conf()
				.DefineCollection<ProxyDefinition>("Proxies")
				.OverrideRoot(x => x.Directory(""))
				.SetSteril()
				.Conf()
				.SetCacheProvider<FullCacheProvider>();

			DbContext.SetContext(config);
		}

        public void New()
        {
            var id = Guid.NewGuid();
            var path = Path.Combine(Config.Instance.DataDirectory, "IdeDevDatabase", id.ToString());
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


            XmlDocument proxydef = new XmlDocument();
            definition.LoadXml(Properties.Resources.proxydef);
            definition.Save(Path.Combine(resourcePath, "proxydef.xml"));
			RaisePropertyChanged("IdeDevDatabaseGames");
        }
        
		public void ImportGame(string directory)
		{
			NeedsSave = false;
			DidManualSave = true;
		}

		public void LoadGame(Game game)
		{

			NeedsSave = false;
			DidManualSave = true;

			Game = game;
			GamePath = Game.InstallPath;
			Sets = Game.Sets().ToList();
			ProxyDef = Game.GetCardProxyDef();
		}
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