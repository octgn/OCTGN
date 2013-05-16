using System;

namespace Octgn.Networking
{
    internal class Mute : IDisposable
    {
        private readonly int _oldMuteId;

        public Mute(int muteId)
        {
            _oldMuteId = Program.Client.Muted;
            Program.Client.Muted = muteId;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Program.Client.Muted = _oldMuteId;
        }

        #endregion
    }
}