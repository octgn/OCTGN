using System;
using System.IO;

namespace Octgn.Play.Save
{
    public class ReplayReader : IDisposable
    {
        private DateTime _startTime;
    
        public Replay Replay { get; }

        private BinaryReader _binaryReader;

        private readonly long _streamGameStartPosition;

        private ReplayReader(BinaryReader reader, Replay replay, long streamGameStartPosition) {
            _binaryReader = reader ?? throw new ArgumentNullException(nameof(reader));
            _streamGameStartPosition = streamGameStartPosition;

            Replay = replay;

            _startTime = DateTime.Now;
        }

        public void StartOver() {
            _binaryReader.BaseStream.Position = _streamGameStartPosition;
        }

        public bool ReadNextMessage(out TimeSpan atTime, out byte[] msg) {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayWriter));

            long timeTicks = 0;
            try {
                timeTicks = _binaryReader.ReadInt64();
            } catch (EndOfStreamException) {
                atTime = default(TimeSpan);
                msg = null;

                return false;
            }

            atTime = TimeSpan.FromTicks(timeTicks);

            var dataLength = _binaryReader.ReadInt32();

            msg = _binaryReader.ReadBytes(dataLength);

            return true;
        }

        public static ReplayReader FromStream(Stream stream) {
            var binaryReader = new BinaryReader(stream ?? throw new ArgumentNullException(nameof(stream)));

            var headerCount = binaryReader.ReadInt32();
            var name = binaryReader.ReadString();
            var gameIdString = binaryReader.ReadString();
            var username = binaryReader.ReadString();
            var gameStartTimeTicks = binaryReader.ReadInt64();

            var gameStartTime = TimeSpan.FromTicks(gameStartTimeTicks);

            var gameId = Guid.Parse(gameIdString);

            var replay = new Replay {
                Name = name,
                GameId = gameId,
                User = username,
                GameStartTime = gameStartTime
            };

            var pos = binaryReader.BaseStream.Position;

            long lengthTicks = 0;

            while (true) {
                try {
                    lengthTicks = binaryReader.ReadInt64();
                    var length = binaryReader.ReadInt32();
                    binaryReader.ReadBytes(length);
                } catch (EndOfStreamException) {
                    break;
                }
            }

            binaryReader.BaseStream.Position = pos;

            replay.GameLength = TimeSpan.FromTicks(lengthTicks);

            return new ReplayReader(binaryReader, replay, pos);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _binaryReader?.Dispose();
                    _binaryReader = null;
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
