using System.Linq;

using Octgn.Utils;

//using Octgn.Play.GUI;

namespace Octgn.Play
{
    public sealed class Pile : Group
    {
        #region Public interface

        private bool _collapsed;

        internal Pile(Player owner, DataNew.Entities.Group def)
            : base(owner, def)
        {
            _collapsed = def.Collapsed;
        }

        public bool Collapsed
        {
            get { return _collapsed; }
            set
            {
                if (value == _collapsed) return;
                _collapsed = value;
                OnPropertyChanged("Collapsed");
            }
        }

        // Dummy property to allow animations in the player panel
        internal bool AnimateInsertion { get; set; }

        public Card TopCard
        {
            get { return cards.Count > 0 ? null : cards[0]; }
        }

        // C'tor

        // Prepare for a shuffle
        // Returns true if the shuffle is asynchronous and should be waited for completion.
        public bool Shuffle()
        {
            if (Locked) return false;

            // Don't shuffle an empty pile
            if (cards.Count == 0) return false;

            if (Player.Count > 1)
            //if (false)
            {
                WantToShuffle = Locked = true;
                bool ready = true;
                // Unalias only if necessary
                if (cards.Any(c => c.Type.Alias))
                {
                    Program.Client.Rpc.UnaliasGrp(this);
                    ready = false;
                }
                if (ready)
                    DoShuffle();
                return true;
            }
            ShuffleAlone();
            return false;
        }

        // Do the shuffle
        internal void DoShuffle()
        {
            // Set internal fields
            PreparingShuffle = false;
            FilledShuffleSlots = 0;
            HasReceivedFirstShuffledMessage = false;
            // Create aliases
            var cis = new CardIdentity[cards.Count];
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].IsVisibleToAll())
                {
                    cis[i] = cards[i].Type;
                    cis[i].Visible = true;
                }
                else
                {
                    CardIdentity ci = cis[i] = new CardIdentity(Program.GameEngine.GenerateCardId());
                    ci.Alias = ci.MySecret = true;
                    ci.Key = ((ulong) Crypto.PositiveRandom()) << 32 | (uint) cards[i].Type.Id;
                    ci.Visible = false;
                }
            }
            // Shuffle
            var cardIds = new int[cards.Count];
            var cardAliases = new ulong[cards.Count];
            var rnd = new CryptoRandom();
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                int r = rnd.Next(i + 1);
                cardIds[i] = cis[r].Id;
                cardAliases[i] = cis[r].Visible ? ulong.MaxValue : Crypto.ModExp(cis[r].Key);
                cis[r] = cis[i];
            }
            // Send the request
            Program.Client.Rpc.CreateAlias(cardIds, cardAliases);
            Program.Client.Rpc.Shuffle(this, cardIds);
        }

        #endregion

        private void ShuffleAlone()
        {
            var rnd = new CryptoRandom();
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                int r = rnd.Next(i + 1);
                Card temp = cards[r];
                cards[r] = cards[i];
                cards[i] = temp;
            }
            OnShuffled();
        }
    }
}