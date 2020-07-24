using Newtonsoft.Json;
using Octgn.Core.Play.Save;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Media;

namespace Octgn.Tabs.GameHistory
{
    public class GameHistoryViewModel : INotifyPropertyChanged, IHistory
    {
        private static log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Guid Id {
            get => _id;
            set {
                if (value == Id) return;
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        private Guid _id;

        public Guid GameId {
            get => _gameId;
            private set {
                if (value != _gameId) {
                    _gameId = value;
                    OnPropertyChanged(nameof(GameId));
                }
            }
        }
        private Guid _gameId;

        public string GameName {
            get => _gameName;
            private set {
                if (value != _gameName) {
                    _gameName = value;
                    OnPropertyChanged(nameof(GameName));
                }
            }
        }
        private string _gameName;

        public string Name {
            get => _name;
            private set {
                if (value == _name) {
                    return;
                }
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private string _name;

        public DateTimeOffset DateStarted {
            get => _dateStarted;
            private set {
                if (value.Equals(_dateStarted)) {
                    return;
                }
                _dateStarted = value;
                OnPropertyChanged(nameof(DateStarted));
            }
        }
        private DateTimeOffset _dateStarted;

        public DateTimeOffset DateSaved {
            get => _dateSaved;
            private set {
                if (value.Equals(_dateSaved)) {
                    return;
                }
                _dateSaved = value;
                OnPropertyChanged(nameof(DateSaved));
            }
        }
        private DateTimeOffset _dateSaved;

        public string RunTime {
            get => _runTime;
            set {
                if (value.Equals(_runTime)) {
                    return;
                }
                _runTime = value;
                OnPropertyChanged(nameof(RunTime));
            }
        }
        private string _runTime;

        public string ReplayFile {
            get => _replayFile;
            set {
                if (value.Equals(_replayFile)) {
                    return;
                }
                _replayFile = value;
                OnPropertyChanged(nameof(ReplayFile));
                OnPropertyChanged(nameof(HasReplay));
            }
        }
        private string _replayFile;

        public bool HasReplay =>
            !string.IsNullOrWhiteSpace(ReplayFile)
            && File.Exists(ReplayFile);

        public string LogFile {
            get => _logFile;
            set {
                if (value.Equals(_logFile)) {
                    return;
                }
                _logFile = value;
                OnPropertyChanged(nameof(LogFile));
            }
        }
        private string _logFile;

        public string Path {
            get => _path;
            set {
                if (value.Equals(_path)) {
                    return;
                }
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }
        private string _path;

        public IGameSaveState State  {
            get => _state;
            set {
                if (value.Equals(_state)) {
                    return;
                }
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }
        private IGameSaveState _state;

        public ObservableCollection<GameHistoryPlayerViewModel> Players {
            get => _players;
            set {
                if (value.Equals(_players)) {
                    return;
                }
                _players = value;
                OnPropertyChanged(nameof(Players));
            }
        }
        private ObservableCollection<GameHistoryPlayerViewModel> _players;

        public GameHistoryViewModel() {
        }

        public GameHistoryViewModel(IHistory history, string gameName, string path) {
            Id = history.Id;
            GameId = history.GameId;
            Name = history.Name;
            DateStarted = history.DateStarted.LocalDateTime;
            var runTime = (history.DateSaved.LocalDateTime - history.DateStarted.LocalDateTime);
            RunTime = string.Format("{0}h {1}m", Math.Floor(runTime.TotalHours), runTime.Minutes);
            GameName = gameName;
            Path = path;

            _players = new ObservableCollection<GameHistoryPlayerViewModel>();

            foreach(var player in history.State.Players) {
                var pvm = new GameHistoryPlayerViewModel(player);
                _players.Add(pvm);
            }
        }

        public GameHistoryViewModel(GameHistoryViewModel history) {
            Id = history.Id;
            GameId = history.GameId;
            Name = history.Name;
            DateStarted = history.DateStarted;
            RunTime = history.RunTime;
            GameName = history.GameName;
            _players = new ObservableCollection<GameHistoryPlayerViewModel>(history.Players);
        }

        public void Update(GameHistoryViewModel history) {
            Id = history.Id;
            GameId = history.GameId;
            Name = history.Name;
            DateStarted = history.DateStarted;
            RunTime = history.RunTime;
            GameName = history.GameName;
            _players = new ObservableCollection<GameHistoryPlayerViewModel>(history.Players);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class GameHistoryPlayerViewModel : INotifyPropertyChanged
    {
        public string Name {
            get => _name;
            private set {
                if (value == _name) {
                    return;
                }
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private string _name;

        public SolidColorBrush Color {
            get => _color;
            private set {
                if (value == _color) {
                    return;
                }
                _color = value;
                OnPropertyChanged(nameof(Color));
            }
        }
        private SolidColorBrush _color;

        public GameHistoryPlayerViewModel() { }

        public GameHistoryPlayerViewModel(IPlayerSaveState player) {
            Name = player.Nickname;
            Color = new SolidColorBrush(player.Color);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static byte[] Serialize(GameHistoryPlayerViewModel history) {
            var str = JsonConvert.SerializeObject(history, Formatting.Indented);

            var bytes = Encoding.UTF8.GetBytes(str);

            return bytes;
        }

        public static GameHistoryPlayerViewModel Deserialize(byte[] data) {
            var str = Encoding.UTF8.GetString(data);

            var history = JsonConvert.DeserializeObject<GameHistoryPlayerViewModel>(str);

            return history;
        }
    }
}