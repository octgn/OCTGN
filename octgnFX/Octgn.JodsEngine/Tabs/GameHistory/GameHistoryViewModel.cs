using Octgn.Core.Play.Save;
using Octgn.Play.Save;
using Octgn.Play.State;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Media;

namespace Octgn.Tabs.GameHistory
{
    public class GameHistoryViewModel : INotifyPropertyChanged
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

        public DateTime StartTime {
            get => _startTime;
            private set {
                if (value.Equals(_startTime)) {
                    return;
                }
                _startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }
        private DateTime _startTime;

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

        public GameHistoryViewModel(History history, string gameName, string path) {
            Id = history.Id;
            GameId = history.GameId;
            Name = history.Name;
            StartTime = history.DateStarted.LocalDateTime;
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
            StartTime = history.StartTime;
            RunTime = history.RunTime;
            GameName = history.GameName;
            _players = new ObservableCollection<GameHistoryPlayerViewModel>(history.Players);
        }

        public void Update(GameHistoryViewModel history) {
            Id = history.Id;
            GameId = history.GameId;
            Name = history.Name;
            StartTime = history.StartTime;
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
    }
}