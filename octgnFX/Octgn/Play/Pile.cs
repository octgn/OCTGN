using Octgn.Utils;

//using Octgn.Play.GUI;

namespace Octgn.Play
{
    using System.Collections.Generic;

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
            get { return cards.Count > 0 ? cards[0] : null; }
        }

        // C'tor

        // Prepare for a shuffle
        // Returns true if the shuffle is asynchronous and should be waited for completion.
        public bool Shuffle()
        {
            if (Locked) return false;

            // Don't shuffle an empty pile
            if (cards.Count == 0) return false;

            //if (Player.Count > 1)
            ////if (false)
            //{
                //WantToShuffle = Locked = true;
                //bool ready = true;
                // Unalias only if necessary
                //if (cards.Any(c => c.Type.Alias))
                //{
                //    Program.Client.Rpc.UnaliasGrp(this);
                //    ready = false;
                //}
                //if (ready)
                DoShuffle();
                return true;
            //}
            //ShuffleAlone();
            //return false;
        }

        // Do the shuffle
        internal void DoShuffle()
        {
            // Set internal fields
            //PreparingShuffle = false;
            //FilledShuffleSlots = 0;
            //HasReceivedFirstShuffledMessage = false;
            // Create aliases
            //var cis = new CardIdentity[cards.Count];
            //for (int i = 0; i < cards.Count; i++)
            //{
            //    cis[i] = cards[i].CreateIdentity();
            //}
            // Shuffle
            var cardIds = new int[cards.Count];
            //var cardAliases = new ulong[cards.Count];
            var rnd = new CryptoRandom();
            var posit = new short[cards.Count];
            var availPosList = new List<short>(cards.Count);
            for (var i = 0; i < cards.Count; i++)
            {
                //availPosList[i] = (short)i;
                availPosList.Add((short)i);
            }
            for (int i = cards.Count - 1; i >= 0; i--)
            {
                var availPos = rnd.Next(availPosList.Count);
                var pos = availPosList[availPos];
                availPosList.RemoveAt(availPos);
                cardIds[i] = cards[pos].Id;
                //cardAliases[i] = cis[r].Visible ? ulong.MaxValue : Crypto.ModExp(cis[r].Key);
                //cis[pos] = cis[i];
                posit[i] = pos;
            }
            // Send the request
            //Program.Client.Rpc.CreateAlias(cardIds, cardAliases);
            //Program.Client.Rpc.Shuffle(this, cardIds);
            Program.Client.Rpc.Shuffled(this, cardIds, posit);
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