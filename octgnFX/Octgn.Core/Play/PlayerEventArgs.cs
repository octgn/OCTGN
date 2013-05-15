namespace Octgn.Core.Play
{
    using System;

    using Octgn.Play;

    public class PlayerEventArgs : EventArgs
    {
        public readonly IPlayPlayer Player;

        public PlayerEventArgs(IPlayPlayer p)
        {
            this.Player = p;
        }
    }
}