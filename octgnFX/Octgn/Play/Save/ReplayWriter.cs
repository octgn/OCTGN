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

        private Queue<(TimeSpan time, byte[] message)> _prestartQueue = new Queue<(TimeSpan, byte[])>();

        public void Start(Replay replay, Stream stream) {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayWriter));
            if (Replay != null) throw new InvalidOperationException("Already started.");

            Replay = replay ?? throw new ArgumentNullException(nameof(replay));

            _binaryWriter = new BinaryWriter(_stream = stream ?? throw new ArgumentNullException(nameof(stream)));

            _startTime = DateTime.Now;

            // header count
            _binaryWriter.Write(2);
            _binaryWriter.Write(Replay.Name);
            _binaryWriter.Write(Replay.GameId.ToString());
            _binaryWriter.Write(Replay.User);

            while(_prestartQueue.Count > 0) {
                var item = _prestartQueue.Dequeue();
                _binaryWriter.Write((DateTime.Now - item.time).Ticks);
                _binaryWriter.Write(item.message.Length);
                _binaryWriter.Write(item.message);
            }

            _binaryWriter.Flush();
        }

        public void WriteMessage(byte[] message) {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayWriter));

            if (Replay == null) {
                _prestartQueue.Enqueue((DateTime.Now - _startTime, message));
            } else {
                _binaryWriter.Write((DateTime.Now - _startTime).Ticks);
                _binaryWriter.Write(message.Length);
                _binaryWriter.Write(message);
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
