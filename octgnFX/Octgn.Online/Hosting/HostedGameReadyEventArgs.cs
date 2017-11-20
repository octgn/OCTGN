using Octgn.Communication;
using System;

namespace Octgn.Online.Hosting
{
    public class HostedGameReadyEventArgs : EventArgs
    {
        public Client Client { get; set; }

        public HostedGame Game { get; set; }
    }
}
