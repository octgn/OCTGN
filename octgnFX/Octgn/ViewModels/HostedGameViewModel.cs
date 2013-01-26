namespace Octgn.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Linq;

    using Skylabs.Lobby;

    public class HostedGameViewModel : INotifyPropertyChanged
    {
        private Guid gameId;

        private string gameName;

        private Version gameVersion;

        private string name;

        private NewUser user;

        private int port;

        private EHostedGame status;

        private DateTime startTime;

        private bool canPlay;

        public Guid GameId
        {
            get
            {
                return this.gameId;
            }
            private set
            {
                this.gameId = value;
                this.OnPropertyChanged("GameId");
            }
        }

        public string GameName
        {
            get
            {
                return this.gameName;
            }
            private set
            {
                if (value == this.gameName)
                {
                    return;
                }
                this.gameName = value;
                this.OnPropertyChanged("GameName");
            }
        }

        public Version GameVersion
        {
            get
            {
                return this.gameVersion;
            }
            private set
            {
                if (Equals(value, this.gameVersion))
                {
                    return;
                }
                this.gameVersion = value;
                this.OnPropertyChanged("GameVersion");
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            private set
            {
                if (value == this.name)
                {
                    return;
                }
                this.name = value;
                this.OnPropertyChanged("Name");
            }
        }

        public NewUser User
        {
            get
            {
                return this.user;
            }
            private set
            {
                if (Equals(value, this.user))
                {
                    return;
                }
                this.user = value;
                this.OnPropertyChanged("User");
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
            private set
            {
                if (value == this.port)
                {
                    return;
                }
                this.port = value;
                this.OnPropertyChanged("Port");
            }
        }

        public Skylabs.Lobby.EHostedGame Status
        {
            get
            {
                return this.status;
            }
            private set
            {
                if (value == this.status)
                {
                    return;
                }
                this.status = value;
                this.OnPropertyChanged("Status");
            }
        }

        public DateTime StartTime
        {
            get
            {
                return this.startTime;
            }
            private set
            {
                if (value.Equals(this.startTime))
                {
                    return;
                }
                this.startTime = value;
                this.OnPropertyChanged("StartTime");
            }
        }

        public bool CanPlay
        {
            get
            {
                return this.canPlay;
            }
            private set
            {
                if (value.Equals(this.canPlay))
                {
                    return;
                }
                this.canPlay = value;
                this.OnPropertyChanged("CanPlay");
            }
        }

        public HostedGameViewModel(HostedGameData data)
        {
            var game = Program.GamesRepository.Games.FirstOrDefault(x => x.Id == data.GameGuid);
            this.GameId = data.GameGuid;
            this.GameVersion = data.GameVersion;
            this.Name = data.Name;
            this.User = data.UserHosting;
            this.Port = data.Port;
            this.Status = data.GameStatus;
            this.StartTime = data.TimeStarted;
            this.GameName = "{Unknown Game}";
            if (game == null) return;
            this.CanPlay = true;
            this.GameName = game.Name;
        }
        public void Update()
        {
            var game = Program.GamesRepository.Games.FirstOrDefault(x => x.Id == this.GameId);
            if (game == null)
            {
                this.GameName = "{Unknown Game}";
                this.CanPlay = false;
                return;
            }
            this.CanPlay = true;
            this.GameName = game.Name;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}