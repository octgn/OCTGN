using System;
using System.Linq;
using System.Security.Cryptography;
using Octgn.Definitions;
using Octgn.Utils;

//using Octgn.Play.GUI;

namespace Octgn.Play
{
    public sealed class Pile : Group
    {
        #region Public interface

        private bool _collapsed;
        private static RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider();

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
            var cardIds = new int[cards.Count];
            var cardAliases = new ulong[cards.Count];
            var rndbytes = new Byte[4];
            var cardRnds = new uint[cards.Count];
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].IsVisibleToAll())
                {
                    cis[i] = cards[i].Type;
                    cis[i].Visible = true;
                }
                else
                {
                    CardIdentity ci = cis[i] = new CardIdentity(Program.Game.GenerateCardId());
                    ci.Alias = ci.MySecret = true;
                    ci.Key = ((ulong) Crypto.PositiveRandom()) << 32 | (uint) cards[i].Type.Id;
                    ci.Visible = false;
                }
            }
            // Shuffle
            do
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    rnd.GetBytes(rndbytes);
                    cardRnds[i] = BitConverter.ToUInt32(rndbytes, 0);
                }
            } while (!SortShuffle(cardRnds, 0, cards.Count - 1, cis, false))

            // Shuffle complete, build arrays
            for (int i = 0; i < cards.Count; i++)
            {
                cardIds[i] = cis[i].Id;
                cardAliases[i] = cis[i].Visible ? ulong.MaxValue : Crypto.ModExp(cis[i].Key);
            }

            // Send the request
            Program.Client.Rpc.CreateAlias(cardIds, cardAliases);
            Program.Client.Rpc.Shuffle(this, cardIds);
        }

        #endregion

        private void ShuffleAlone()
        {
            var rndbytes = new Byte[4];
            var cardRnds = new uint[cards.Count];
            do
            {
                for (int i = 0; i < cards.Count; i++)
                {
                    rnd.GetBytes(rndbytes);
                    cardRnds[i] = BitConverter.ToUInt32(rndbytes, 0);
                }
            } while (!SortShuffle(cardRnds, 0, cards.Count - 1, null, true))
            OnShuffled();
        }

        // Quick sort pile to shuffle. Return false if 2 cards were assigned equal values.
        internal bool SortShuffle(uint[] a, int left, int right, CardIdentity[] cis, bool local)
        {
            int i = left;
            int j = right;
            uint x = a[(left + right) / 2];
            uint w = 0;
            while (i <= j)
            {
                while (a[i] < x)
                {
                    i++;
                }
                while (x < a[j])
                {
                    j--;
                }
                if (i <= j)
                {
                    if (a[i] == w || a[j] == w)
                        return false;
                    if (cis != null)
                    {
                        CardIdentity ci = cis[i];
                        cis[i] = cis[j];
                        cis[j] = ci;
                    }
                    if (local)
                    {
                        Card temp = cards[i];
                        cards[i] = cards[j];
                        cards[j] = temp;
                    }

                    w = a[i];
                    a[i++] = a[j];
                    a[j--] = w;
                }
            }
            if (left < j)
            {
                if (!SortShuffle(a, left, j, cis, local))
                    return false;
            }
            if (i < right)
            {
                if (!SortShuffle(a, i, right, cis, local))
                    return false;
            }
            return true;
        }
    }
}
