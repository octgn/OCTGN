using System;

namespace Octgn.Networking
{
    using Octgn.Core;

    internal class Mute : IDisposable
    {
        private readonly int _oldMuteId;

        public Mute(int muteId)
        {
            _oldMuteId = K.C.Get<Client>().Muted;
            K.C.Get<Client>().Muted = muteId;
        }

        #region IDisposable Members

        public void Dispose()
        {
            K.C.Get<Client>().Muted = _oldMuteId;
        }

        #endregion
    }
}