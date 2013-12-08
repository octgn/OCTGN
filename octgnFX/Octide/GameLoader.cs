using System.IO;

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
                Messenger.Default.Send(new PropertyChangedMessage<Game>(this, this.game, value, "Game"));
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

        public GameLoader()
        {
            game = new Game();
            game.Version = new Version("0.0.0.0");
            game.Authors = new List<string>();
            game.Tags = new List<string>();
            game.NoteBackgroundColor = "#FFEBE8C5";
            game.NoteForegroundColor = "#FF000000";
        }

        public void LoadGame(string filename)
        {
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
            
        }

        public void GameChanged(object sender)
        {
            RaisePropertyChanged("Game");
            Messenger.Default.Send(new PropertyChangedMessage<Game>(sender, null, this.game, "Game"));
        }
    }
}