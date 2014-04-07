using System.IO;
using System.Threading.Tasks;

namespace Octide
{
    using System;
    using System.Collections.Generic;

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
            var path = Path.Combine(Octgn.Library.Config.Instance.DataDirectory, "GameDatabase", id.ToString());
            var defPath = Path.Combine(path, "definition.xml");

            Directory.CreateDirectory(path);
            var g = new Game();
            g.Id = id;
            g.InstallPath = path;
            g.Name = id.ToString();
            g.Version = new Version(1, 0, 0, 0);
            g.Authors = new List<string>();
            g.Tags = new List<string>();
            g.NoteBackgroundColor = "#FFEBE8C5";
            g.NoteForegroundColor = "#FF000000";
            g.OctgnVersion = typeof(Octgn.Library.Config).Assembly.GetName().Version;
            g.Filename = defPath;
            var s = new Octgn.DataNew.GameSerializer();
            s.Serialize(g);
            LoadGame(g.Filename);
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
            s.Def = new CollectionDefinition<Game>(conf, "GameDatabase");
            try
            {
                GamePath = new FileInfo(filename).Directory.FullName;
                Game = (Game)s.Deserialize(filename);
            }
            catch (System.Exception e)
            {
                GamePath = "";
            }
        }

        public void SaveGame()
        {
            if (!NeedsSave)
                return;
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