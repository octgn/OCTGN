using log4net;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace Octgn.Play.Save
{
    public class ReplayEngine : IDisposable, INotifyPropertyChanged
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public Replay Replay => _reader.Replay;

        private readonly DispatcherTimer _timer;
        private readonly ReplayReader _reader;
        private readonly ReplayClient _client;

        public ReplayEngine(ReplayReader reader, ReplayClient client) {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(5);
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _client = client ?? throw new ArgumentNullException(nameof(client));

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

        public bool IsFastForwarding => _fastforwardTo != null;

        private byte[] _nextMsg = null;
        private TimeSpan _nextTime = TimeSpan.MinValue;

        private DateTime? _lastTickTime = null;

        private TimeSpan? _fastforwardTo;

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
                if (Speed <= 1) {
                    return Speed.ToString(".##%");
                } else if(Speed == 1) {
                    return "1X";
                } else {
                    return Speed.ToString("###.##X");
                }
            }
        }

        private double _speed = 1;

        private async void _timer_Tick(object sender, EventArgs e) {
            try {
                _timer.Stop();

                var timePassedSinceLastTick = _lastTickTime == null
                    ? TimeSpan.Zero
                    : DateTime.Now - _lastTickTime.Value
                ;

                _lastTickTime = DateTime.Now;

                var newTicks = (long)(timePassedSinceLastTick.Ticks * Speed);
                timePassedSinceLastTick = TimeSpan.FromTicks(newTicks);

                CurrentTime += timePassedSinceLastTick;

                if (_fastforwardTo != null) {
                    if (_fastforwardTo.Value <= CurrentTime) {
                        _fastforwardTo = null;
                        Speed = 1;

                        OnPropertyChanged(nameof(IsFastForwarding));
                    }
                }

                if (_nextMsg != null) {
                    if (CurrentTime >= _nextTime) {
                        var next = _nextMsg;

                        _nextMsg = null;

                        _client.AddMessage(next);

                        await Dispatcher.Yield(DispatcherPriority.Loaded);
                    } else {
                        return;
                    }
                }

                if (_nextMsg == null) {
                    try {
                        if (_reader.ReadNextMessage(out var atTime, out var msg)) {
                            _nextMsg = msg;
                            _nextTime = atTime;
                        } else {
                            Pause();
                            return;
                        }
                    } catch (ObjectDisposedException) {
                        Pause();
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

        public void Start() {
            if (_timer.IsEnabled) return;
            _lastTickTime = null;
            _timer.Start();
        }

        public void Pause() {
            if (!_timer.IsEnabled) return;
            _timer.Stop();
        }

        public void FastForwardTo(TimeSpan when) {
            if (_fastforwardTo == when) return;
            if (when == CurrentTime) return;

            if(when < CurrentTime) {
                _reader.StartOver();
                Program.GameEngine.Reset();
                CurrentTime = Replay.GameStartTime;

                _nextMsg = null;
                _nextTime = TimeSpan.MinValue;
                _lastTickTime = null;
            }

            _fastforwardTo = when;
            Speed = 50;

            OnPropertyChanged(nameof(IsFastForwarding));

            Start();
        }

        public void FastForwardToStart() {
            FastForwardTo(_reader.Replay.GameStartTime);
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
                    _reader.Dispose();
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
