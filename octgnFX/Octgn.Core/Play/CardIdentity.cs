using System;
using System.Collections.Generic;
using Octgn.Data;

namespace Octgn.Play
{
    using Octgn.Core.DataExtensionMethods;

    public class CardIdentity
    {
        private static readonly Dictionary<int, CardIdentity> All = new Dictionary<int, CardIdentity>(100);

        public readonly int Id; // id of the card (playerId << 16 | localId)
        public bool Alias; // if alias is true, the type in key is not a card type but another CardIdentity to substitue
        public bool InUse; // if true, this cardidentity is currently linked to a card's Type property
        public ulong Key; // (nonce << 32 | type), either encrypted, or not
        public DataNew.Entities.Card Model; // card type
        public bool MySecret; // if mySecret is true, key is not encrypted, and has not been made publicly available yet
        public bool Revealing; // true if the card is being - or has been - revealed
        public bool Visible; // indicates if a card is face up during a shuffle [transient]

        public CardIdentity(int id)
        {
            Id = id;
            All.Add(id, this);
        }

        public event EventHandler<RevealEventArgs> Revealed;

        public static CardIdentity Find(int id)
        {
            CardIdentity res;
            bool success = All.TryGetValue(id, out res);
            return success ? res : null;
        }

        public static void Delete(int id)
        {
            All.Remove(id);
        }

        public static void Reset()
        {
            All.Clear();
        }

        public override string ToString()
        {
            return Model == null ? "Card" : Model.PropertyName();
        }

        public void OnRevealed(CardIdentity newId)
        {
            if (Revealed != null)
                Revealed(this, new RevealEventArgs {NewIdentity = newId});
        }
    }

    public class RevealEventArgs : EventArgs
    {
        public CardIdentity NewIdentity { get; set; }
    }
}