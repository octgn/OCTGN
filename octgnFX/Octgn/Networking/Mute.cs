using System;

namespace Octgn.Networking
{
    public class Mute : IDisposable
    {
        private readonly Guid? _oldMuteId;

        public Mute(Guid? muteId)
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