using System;
using System.Collections.Generic;
using System.IO;

namespace Octgn.Play.Save
{
    public class ReplayWriter : IDisposable
    {
        private DateTime _startTime;

        public Replay Replay { get; private set; }

        public bool IsStarted => Replay != null;

        private Stream _stream;

        //private StreamWriter _streamWriter;
        private BinaryWriter _binaryWriter;

        public ReplayWriter() {
        }

        private Queue<ReplayEvent> _prestartQueue = new Queue<ReplayEvent>();

        public void Start(Replay replay, Stream stream) {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayWriter));
            if (Replay != null) throw new InvalidOperationException("Already started.");

            _startTime = DateTime.Now - _lobbyTime;

            Replay = replay ?? throw new ArgumentNullException(nameof(replay));

            _binaryWriter = new BinaryWriter(_stream = stream ?? throw new ArgumentNullException(nameof(stream)));

            // header count
            _binaryWriter.Write(4);
            _binaryWriter.Write(Replay.Name);
            _binaryWriter.Write(Replay.GameId.ToString());
            _binaryWriter.Write(Replay.User);
            _binaryWriter.Write(_lobbyTime.Ticks); // Game start time, for fastforwarding

            while(_prestartQueue.Count > 0) {
                var item = _prestartQueue.Dequeue();
                ReplayEvent.Write(item, _binaryWriter);
            }

            _binaryWriter.Flush();
        }

        private TimeSpan _lobbyTime = TimeSpan.Zero;

        private bool _wroteStartEvent = false;

        public void WriteEvent(ReplayEvent replayEvent) {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayWriter));

            if(replayEvent.Type == ReplayEventType.Start) {
                if(_wroteStartEvent)
                    throw new InvalidOperationException($"Already wrote Start event");

                _wroteStartEvent = true;
            }

            if (Replay == null) {
                replayEvent.Time = _lobbyTime;
                _lobbyTime += TimeSpan.FromMilliseconds(10);
                _prestartQueue.Enqueue(replayEvent);
            } else {
                replayEvent.Time = DateTime.Now - _startTime;
                ReplayEvent.Write(replayEvent, _binaryWriter);
                _binaryWriter.Flush();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _binaryWriter?.Dispose();
                    _binaryWriter = null;
                    _stream = null;
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
