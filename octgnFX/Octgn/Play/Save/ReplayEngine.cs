using System;
using System.Windows.Threading;

namespace Octgn.Play.Save
{
    public class ReplayEngine : IDisposable
    {
        private readonly DispatcherTimer _timer;
        private readonly ReplayReader _reader;
        private readonly ReplayClient _client;

        public ReplayEngine(ReplayReader reader, ReplayClient client) {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _client = client ?? throw new ArgumentNullException(nameof(client));

            _timer.Tick += _timer_Tick;
        }

        private byte[] _nextMsg = null;
        private TimeSpan _nextTime = TimeSpan.MinValue;

        private TimeSpan _currentTime = TimeSpan.Zero;

        private DateTime? _lastTickTime = null;

        private void _timer_Tick(object sender, EventArgs e) {
            while (true) {
                var timePassedSinceLastTick = _lastTickTime == null
                    ? TimeSpan.Zero
                    : DateTime.Now - _lastTickTime.Value
                ;

                _currentTime += timePassedSinceLastTick;

                _lastTickTime = DateTime.Now;

                if (_nextMsg != null) {
                    if (_currentTime >= _nextTime) {
                        var next = _nextMsg;

                        _nextMsg = null;

                        _client.AddMessage(next);
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
            }
        }

        public void Start() {
            _lastTickTime = null;
            _timer.Start();
        }

        public void Pause() {
            _timer.Stop();
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
