using System;

namespace Octgn.Play.Save
{
    public class Replay
    {
        public string Name { get; set; }

        public string User { get; set; }

        public Guid GameId { get; set; }

        public Replay() {
        }
    }
}
