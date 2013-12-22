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
            get
            {
                lock (cards)
                {
                    return cards.Count > 0 ? cards[0] : null;
                }
            }
        }

        #region Overrides of Group

        public override void OnCardsChanged()
        {
            base.OnCardsChanged();
            OnPropertyChanged("TopCard");
        }

        #endregion

        // Prepare for a shuffle
        public void Shuffle()
        {
            if (Locked || Count == 0) return;

            lock (cards)
            {
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
                // move own cards to new positions
                DoShuffle(cardIds, posit);
                // Inform other players
                Program.Client.Rpc.Shuffled(Player.LocalPlayer, this, cardIds, posit);
            }
        }

        // Do the shuffle
        internal void DoShuffle(int[] card, short[] pos)
        {
            // Check the args
            if (card.Length != pos.Length)
            {
                Program.TraceWarning("[Shuffled] Cards and positions lengths don't match.");
                return;
            }
            //Build the Dict. of new locations
            var shuffled = new Dictionary<int, Card>();
            for (int i = 0; i < card.Length; i++)
            {
                shuffled.Add(pos[i], this[i]);
                // Get the card
                CardIdentity ci = CardIdentity.Find(card[i]);
                if (ci == null)
                {
                    Program.TraceWarning("[Shuffled] Card not found.");
                    continue;
                }
                this[i].SetVisibility(ci.Visible ? DataNew.Entities.GroupVisibility.Everybody : DataNew.Entities.GroupVisibility.Nobody, null);
            }
            //Move cards to their new indexes
            for (int i = 0; i < card.Length; i++)
            {
                Remove(shuffled[i]);
                AddAt(shuffled[i], i);
            }

            OnShuffled();
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