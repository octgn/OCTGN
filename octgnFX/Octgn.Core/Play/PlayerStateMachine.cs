namespace Octgn.Core.Play
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Octgn.Play;

    public class PlayerStateMachine
    {

        // Contains all players in this game (TODO: Rename to All, then cleanup all the dependancies)
        public readonly ObservableCollection<IPlayPlayer> All = new ObservableCollection<IPlayPlayer>();
        public IPlayPlayer LocalPlayer;
        // May be null if there's no global lPlayer in the game definition
        public IPlayPlayer GlobalPlayer;

        // Get all players in the game, except a possible Global lPlayer
        public IEnumerable<IPlayPlayer> AllExceptGlobal
        {
            get { return All.Where(p => p != GlobalPlayer); }
        }

        // Number of players
        internal int Count
        {
            get { return GlobalPlayer == null ? All.Count : All.Count - 1; }
        }

        // Find a lPlayer with his id
        internal IPlayPlayer Find(byte id)
        {
            return All.FirstOrDefault(p => p.Id == id);
        }

        // Resets the lPlayer list
        internal void Reset()
        {
            All.Clear();
            LocalPlayer = GlobalPlayer = null;
        }

        public event Action OnLocalPlayerWelcomed;
        public void FireLocalPlayerWelcomed()
        {
            if (OnLocalPlayerWelcomed != null)
                OnLocalPlayerWelcomed.Invoke();
        }

        // May be null if we're in pure server mode

        internal event EventHandler<PlayerEventArgs> PlayerAdded;
        internal event EventHandler<PlayerEventArgs> PlayerRemoved;

        public void FirePlayerRemoved(PlayerEventArgs args)
        {
            if (PlayerRemoved != null) PlayerRemoved(this, args);
        }
        public void FirePlayerAdded(PlayerEventArgs args)
        {
            if (PlayerAdded != null) PlayerAdded(this, args);
        }
    }

    public class PlayerEventArgs : EventArgs
    {
        public readonly IPlayPlayer Player;

        public PlayerEventArgs(IPlayPlayer p)
        {
            Player = p;
        }
    }
}