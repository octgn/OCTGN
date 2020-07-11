using System;

namespace Octgn.Play.Save
{
    public class Replay
    {
        public string Name { get; set; }

        public string User { get; set; }

        public Guid GameId { get; set; }

        public TimeSpan GameStartTime { get; set; }

        public TimeSpan GameLength { get; set; }

        public Replay() {
        }
    }
}
