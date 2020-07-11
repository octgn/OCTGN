using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Octgn.Play.Save
{
    public class ReplayEngine : IDisposable, INotifyPropertyChanged
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Replay Replay { get; }

        private readonly DispatcherTimer _timer;
        private readonly ReplayClient _client;

        private readonly IReadOnlyDictionary<int, ReplayEvent> _events;

        public ReplayEngine(ReplayReader reader, ReplayClient client) {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(5);
            _client = client ?? throw new ArgumentNullException(nameof(client));

            if(reader == null) throw new ArgumentNullException(nameof(reader));

            Replay = reader.Replay;

            _events = reader.ReadAllEvents().ToDictionary(x => x.Id, x => x);

            _timer.Tick += _timer_Tick;
        }

        public TimeSpan CurrentTime {
            get => _currentTime;
            set {
                if (value == _currentTime) return;
                _currentTime = value;
                OnPropertyChanged();
            }
        }
        private TimeSpan _currentTime = TimeSpan.Zero;

        public string PlayButtonText {
            get => IsPlaying
                ? "‖"
                : "▶";
        }

        public bool IsPlaying {
            get => _isPlaying;
            set {
                if (value == _isPlaying) return;
                _isPlaying = value;

                if (value) {
                    _lastTickTime = null;

                    if (!_timer.IsEnabled) {
                        _timer.Start();
                    }
                } else {
                    _fastforwardTo = null;
                    Speed = 1;
                    OnPropertyChanged(nameof(IsFastForwarding));
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(PlayButtonText));
            }
        }
        private bool _isPlaying;

        public bool IsFastForwarding => _fastforwardTo != null;

        private ReplayEvent? _currentEvent;

        private DateTime? _lastTickTime = null;

        private TimeSpan? _fastforwardTo;
        private bool _pauseAfterFastForward;

        public double Speed {
            get => _speed;
            set {
                if (value == _speed) return;
                _speed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SpeedString));
            }
        }

        public string SpeedString {
            get {
                return Speed.ToString("###.##X");
            }
        }

        private double _speed = 1;

        public IEnumerable<ReplayEvent> AllEvents => _events.Select(x => x.Value);

        private async void _timer_Tick(object sender, EventArgs e) {
            try {
                _timer.Stop();

                while (true) {
                    if (!_isPlaying && _fastforwardTo == null) return;

                    var timePassedSinceLastTick = _lastTickTime == null
                        ? TimeSpan.Zero
                        : DateTime.Now - _lastTickTime.Value
                    ;

                    var newTicks = (long)(timePassedSinceLastTick.Ticks * Speed);
                    timePassedSinceLastTick = TimeSpan.FromTicks(newTicks);

                    CurrentTime += timePassedSinceLastTick;

                    var next = default(ReplayEvent);

                    if(_currentEvent == null) {
                        next = _events[0];
                    } else if(_events.Count > _currentEvent.Value.Id + 1) {
                        next = _events[_currentEvent.Value.Id + 1];
                    } else {
                        Pause();
                        return;
                    }

                    if (_fastforwardTo != null) {
                        CurrentTime = next.Time;

                        if (_fastforwardTo.Value <= CurrentTime) {
                            _fastforwardTo = null;
                            Speed = 1;

                            IsPlaying = !_pauseAfterFastForward;

                            OnPropertyChanged(nameof(IsFastForwarding));
                        }
                    }

                    if (CurrentTime >= next.Time) {
                        _currentEvent = next;

                        _client.AddMessage(_currentEvent.Value.Action);

                        await Task.Delay(10);

                        await Dispatcher.Yield(DispatcherPriority.ContextIdle);

                        _lastTickTime = DateTime.Now;
                    } else {
                        _lastTickTime = DateTime.Now;
                        return;
                    }
                }
            } catch(Exception ex) {
                Log.Error("Tick error", ex);
            } finally {
                _timer.Start();
            }
        }

        public void ToggleSpeed() {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayEngine));
            if (_fastforwardTo != null) return;

            switch (Speed) {
                case 1: Speed = 1.5;
                    break;
                case 1.5: Speed = 2;
                    break;
                case 2: Speed = 5;
                    break;
                case 5: Speed = 10;
                    break;
                case 10: Speed = 25;
                    break;

                case 25: Speed = 0.1;
                    break;
                case 0.1: Speed = 0.2;
                    break;
                case 0.2: Speed = 0.5;
                    break;
                case 0.5: Speed = 1;
                    break;
            }
        }

        public void TogglePlay() {
            if (IsPlaying) {
                Pause();
            } else {
                Start();
            }
        }

        public void Start() {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayEngine));
            if (_isPlaying) return;

            IsPlaying = true;
        }

        public void Pause() {
            if (!_isPlaying) return;
            IsPlaying = false;
        }

        public void FastForwardToNextEvent() {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayEngine));
            if (_currentEvent == null) return;

            var current = _currentEvent.Value;

            var nextId = current.Id + 1;

            if(nextId >= _events.Count) {
                nextId = _events.Count - 1;
            }

            if (nextId == current.Id) return;

            var next = _events[nextId];

            FastForwardTo(next.Time);
        }

        public void RewindToPreviousEvent() {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayEngine));
            if (_currentEvent == null) return;

            var current = _currentEvent.Value;

            var nextId = current.Id - 1;

            if (nextId < 0) {
                nextId = 0;
            }

            if (nextId == current.Id) return;

            var next = _events[nextId];

            FastForwardTo(next.Time);
        }

        public void FastForwardToNextTurn() {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayEngine));
            if (_currentEvent == null) return;

            var current = _currentEvent.Value;

            for(var nextId = current.Id + 1;nextId < _events.Count; nextId++) {
                var next = _events[nextId];

                if(next.Type == ReplayEventType.NextTurn || next.Type == ReplayEventType.Reset) {
                    FastForwardTo(next.Time);
                    return;
                }
            }
        }

        public void RewindToPreviousTurn() {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayEngine));
            if (_currentEvent == null) return;

            var current = _currentEvent.Value;

            for (var nextId = current.Id - 1; nextId >= 0; nextId--) {
                var next = _events[nextId];

                if (next.Type == ReplayEventType.NextTurn || next.Type == ReplayEventType.Reset) {
                    FastForwardTo(next.Time);
                    return;
                } else if(nextId == 0) {
                    FastForwardTo(next.Time);
                    return;
                }
            }
        }

        public void FastForwardTo(TimeSpan when) {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayEngine));
            if (_fastforwardTo == when) return;
            if (when == CurrentTime) return;

            if(when < CurrentTime) {
                _currentEvent = null;
                Program.GameEngine.Reset();
                CurrentTime = Replay.GameStartTime;

                _lastTickTime = null;
            }

            _fastforwardTo = when;
            Speed = 50;

            OnPropertyChanged(nameof(IsFastForwarding));

            _pauseAfterFastForward = !IsPlaying;

            Start();
        }

        public void FastForwardToStart() {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayEngine));
            FastForwardTo(Replay.GameStartTime);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string property = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _timer.Stop();
                    _timer.Tick -= _timer_Tick;
                    (_events as Dictionary<int, ReplayEvent>)?.Clear();
                }

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose() {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
