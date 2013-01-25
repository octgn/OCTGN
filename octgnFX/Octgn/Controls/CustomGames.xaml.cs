using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Octgn.Utils;
using Skylabs.Lobby;

namespace Octgn.Controls
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    using Microsoft.Scripting.Utils;

    /// <summary>
    /// Interaction logic for CustomGames.xaml
    /// </summary>
    public partial class CustomGames : UserControl
    {
        //    public static DependencyProperty HostedGameListProperty = DependencyProperty.Register(
        //"HostedGameList", typeof(List<HostedGameViewModel>), typeof(CustomGames));

        //    public List<HostedGameViewModel> HostedGameList
        //    {
        //        get { return GetValue(HostedGameListProperty) as List<HostedGameViewModel>; }
        //        set{SetValue(HostedGameListProperty, value);}
        //    }

        public ObservableCollection<HostedGameViewModel> HostedGameList { get; set; }

        private Timer timer;
        private bool isConnected;
        private bool waitingForGames;

        public CustomGames()
        {
            InitializeComponent();
            HostedGameList = new ObservableCollection<HostedGameViewModel>();
            Program.LobbyClient.OnLoginComplete += LobbyClient_OnLoginComplete;
            Program.LobbyClient.OnDisconnect += LobbyClient_OnDisconnect;
            Program.LobbyClient.OnDataReceived += LobbyClient_OnDataReceived;
            timer = new Timer(10000);
            timer.Start();
            timer.Elapsed += timer_Elapsed;
        }

        void LobbyClient_OnDisconnect(object sender, EventArgs e)
        {
            isConnected = false;
        }

        void LobbyClient_OnLoginComplete(object sender, LoginResults results)
        {
            isConnected = true;
        }

        void LobbyClient_OnDataReceived(object sender, DataRecType type, object data)
        {
            if (type == DataRecType.GameList || type == DataRecType.GamesNeedRefresh)
            {
                Trace.WriteLine("Games Received");
                RefreshGameList();
                waitingForGames = false;
            }
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Trace.WriteLine("Timer ticks");
            if (!isConnected || waitingForGames) return;
            Trace.WriteLine("Begin refresh games.");
            waitingForGames = true;
            Program.LobbyClient.BeginGetGameList();
        }

        void RefreshGameList()
        {
            Trace.WriteLine("Refreshing list...");
            var list = Program.LobbyClient.GetHostedGames().Select(x => new HostedGameViewModel(x)).ToList();
            Dispatcher.Invoke(new Action(() =>
            {
                var removeList = HostedGameList.Where(i => !list.Any(x => x.Port == i.Port)).ToList();
                removeList.ForEach(x => HostedGameList.Remove(x));
                var addList = list.Where(i => !HostedGameList.Any(x => x.Port == i.Port)).ToList();
                HostedGameList.AddRange(addList);
                foreach(var g in HostedGameList)
                    g.Update();
            }));
        }
    }
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
            GameId = data.GameGuid;
            GameVersion = data.GameVersion;
            Name = data.Name;
            User = data.UserHosting;
            Port = data.Port;
            Status = data.GameStatus;
            StartTime = data.TimeStarted;
            GameName = "{Unknown Game}";
            if (game == null) return;
            CanPlay = true;
            GameName = game.Name;
        }
        public void Update()
        {
            var game = Program.GamesRepository.Games.FirstOrDefault(x => x.Id == GameId);
            if (game == null)
            {
                GameName = "{Unknown Game}";
                CanPlay = false;
                return;
            }
            CanPlay = true;
            GameName = game.Name;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
