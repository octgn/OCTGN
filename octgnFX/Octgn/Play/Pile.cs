using System;
using Octgn.Definitions;
//using Octgn.Play.GUI;

namespace Octgn.Play
{
    public sealed class Pile : Group
    {
        #region Public interface

        private bool _collapsed;

        internal Pile(Player owner, GroupDef def)
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
            {
                WantToShuffle = Locked = true;
                bool ready = true;
                // Unalias only if necessary
                foreach (Card c in cards)
                    if (c.Type.alias)
                    {
                        Program.Client.Rpc.UnaliasGrp(this);
                        ready = false;
                        break;
                    }
                if (ready)
                    DoShuffle();
                return true;
            }
            else
            {
                ShuffleAlone();
                return false;
            }
        }

        // Do the shuffle
        internal void DoShuffle()
        {
            // Set internal fields
            PreparingShuffle = false;
            filledShuffleSlots = 0;
            hasReceivedFirstShuffledMessage = false;
            // Create aliases
            var cis = new CardIdentity[cards.Count];
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].IsVisibleToAll())
                {
                    cis[i] = cards[i].Type;
                    cis[i].visible = true;
                }
                else
                {
                    CardIdentity ci = cis[i] = new CardIdentity(Program.Game.GenerateCardId());
                    ci.alias = ci.mySecret = true;
                    ci.key = ((ulong) Crypto.PositiveRandom()) << 32 | (uint) cards[i].Type.id;
                    ci.visible = false;
                }
            }
            // Shuffle
            var cardIds = new int[cards.Count];
            var cardAliases = new ulong[cards.Count];
            var rnd = new Random();
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                int r = rnd.Next(i);
                cardIds[i] = cis[r].id;
                cardAliases[i] = cis[r].visible ? ulong.MaxValue : Crypto.ModExp(cis[r].key);
                cis[r] = cis[i];
            }
            // Send the request
            Program.Client.Rpc.CreateAlias(cardIds, cardAliases);
            Program.Client.Rpc.Shuffle(this, cardIds);
        }

        #endregion

        private void ShuffleAlone()
        {
            var rnd = new Random();
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                int r = rnd.Next(i);
                Card temp = cards[r];
                cards[r] = cards[i];
                cards[i] = temp;
            }
            OnShuffled();
        }
    }
}