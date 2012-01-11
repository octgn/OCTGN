using System;

namespace Octgn.Networking
{
  class Mute : IDisposable
  {
    private int oldMuteId;

    public Mute(int muteId)
    {
      oldMuteId = Program.Client.Muted;
      Program.Client.Muted = muteId;
    }

    public void Dispose()
    {
      Program.Client.Muted = oldMuteId;
    }
  }
}
