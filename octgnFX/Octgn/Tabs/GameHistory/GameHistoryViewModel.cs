using Octgn.Play.Save;
using System;
using System.ComponentModel;
using System.IO;

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

        public string StartTime {
            get => _startTime;
            private set {
                if (value.Equals(_startTime)) {
                    return;
                }
                _startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }
        private string _startTime;

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

        public GameHistoryViewModel() {
        }

        public GameHistoryViewModel(History history, string gameName) {
            Id = history.Id;
            GameId = history.GameId;
            Name = history.Name;
            StartTime = history.DateStarted.LocalDateTime.ToString("f");
            var runTime = (history.DateSaved.LocalDateTime - history.DateStarted.LocalDateTime);
            RunTime = string.Format("{0}h {1}m", Math.Floor(runTime.TotalHours), runTime.Minutes);
            GameName = gameName;
        }

        public GameHistoryViewModel(GameHistoryViewModel history) {
            Id = history.Id;
            GameId = history.GameId;
            Name = history.Name;
            StartTime = history.StartTime;
            RunTime = history.RunTime;
            GameName = history.GameName;
        }

        public void Update(GameHistoryViewModel history) {
            Id = history.Id;
            GameId = history.GameId;
            Name = history.Name;
            StartTime = history.StartTime;
            RunTime = history.RunTime;
            GameName = history.GameName;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}