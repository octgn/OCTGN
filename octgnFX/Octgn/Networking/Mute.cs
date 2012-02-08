using System;

namespace Octgn.Networking
{
    internal class Mute : IDisposable
    {
        private readonly int oldMuteId;

        public Mute(int muteId)
        {
            oldMuteId = Program.Client.Muted;
            Program.Client.Muted = muteId;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Program.Client.Muted = oldMuteId;
        }

        #endregion
    }
}