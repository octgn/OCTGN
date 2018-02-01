using System;

namespace Octgn.Server
{
    public class PlayerSettings
    {
        public PlayerSettings() { }

        public PlayerSettings(bool invertedTable, bool isSpectator) : this() {
            InvertedTable = invertedTable;
            IsSpectator = isSpectator;
        }

        /// <summary>
        /// When using a two-sided table, indicates whether this player plays on the opposite side
        /// </summary>
        public bool InvertedTable { get; }

        /// <summary>
        /// Is player a spectator
        /// </summary>
        public bool IsSpectator { get; }
    }
}