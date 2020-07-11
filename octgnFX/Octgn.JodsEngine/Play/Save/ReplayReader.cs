using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Octgn.Play.Save
{
    public class ReplayReader : IDisposable
    {
        private DateTime _startTime;
    
        public Replay Replay { get; }

        private IEnumerable<ReplayEvent> _replayEvents;

        private IEnumerator<ReplayEvent> _replayEventPosition;

        private ReplayReader(IEnumerable<ReplayEvent> events, Replay replay) {
            _replayEvents = events ?? throw new ArgumentNullException(nameof(events));

            _replayEventPosition = _replayEvents.GetEnumerator();

            Replay = replay;

            _startTime = DateTime.Now;
        }

        public void StartOver() {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayWriter));

            _replayEventPosition.Reset();
        }

        public bool ReadNextEvent(out ReplayEvent? replayEvent) {
            if (disposedValue) throw new ObjectDisposedException(nameof(ReplayWriter));

            if (_replayEventPosition.MoveNext()) {
                replayEvent = _replayEventPosition.Current;
                return true;
            } else {
                replayEvent = null;
                return false;
            }
        }

        public IEnumerable<ReplayEvent> ReadAllEvents() {
            return _replayEvents.AsEnumerable();
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

            var replayList = new List<ReplayEvent>();

            long lengthTicks = 0;

            var nextId = 0;
            while (true) {
                try {
                    var eve = ReplayEvent.Read(binaryReader);
                    eve.Id = nextId;
                    nextId++;
                    lengthTicks = eve.Time.Ticks;
                    replayList.Add(eve);
                } catch (EndOfStreamException) {
                    break;
                }
            }

            binaryReader.BaseStream.Position = pos;

            replay.GameLength = TimeSpan.FromTicks(lengthTicks);

            return new ReplayReader(replayList, replay);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    _replayEventPosition?.Dispose();
                }

                _replayEvents = null;

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
