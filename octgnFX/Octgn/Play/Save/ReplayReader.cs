using System;
using System.Collections.Generic;
using System.IO;

namespace Octgn.Play.Save
{
    public class ReplayReader : IDisposable
    {
        private DateTime _startTime;

        public Replay Replay { get; private set; }

        public bool IsStarted => Replay != null;

        private Stream _stream;

        //private StreamWriter _streamWriter;
        private BinaryReader _binaryReader;

        public ReplayReader() {
        }

        public void Start(Stream stream) {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayWriter));
            if (Replay != null) throw new InvalidOperationException("Already started.");

            _binaryReader = new BinaryReader(_stream = stream ?? throw new ArgumentNullException(nameof(stream)));

            _startTime = DateTime.Now;

            var headerCount = _binaryReader.ReadInt32();
            var name = _binaryReader.ReadString();
            var gameIdString = _binaryReader.ReadString();

            var gameId = Guid.Parse(gameIdString);

            Replay = new Replay {
                Name = name,
                GameId = gameId
            };
        }

        public byte[] ReadNextMessage() {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayWriter));

            long timeTicks = 0;
            try {
                timeTicks = _binaryReader.ReadInt64();
            } catch (EndOfStreamException) {
                return null;
            }
            var time = TimeSpan.FromTicks(timeTicks);

            var dataLength = _binaryReader.ReadInt32();

            var data = _binaryReader.ReadBytes(dataLength);

            var stream = new MemoryStream(data);
            var reader = new BinaryReader(stream);

            var muted = reader.ReadInt32();

            byte method = reader.ReadByte();

            if (method == 5 || method == 7) return null;

            return data;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _binaryReader?.Dispose();
                    _binaryReader = null;
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
