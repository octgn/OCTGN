using System.Collections.Generic;
using System;

namespace Octgn.Play
{
    public class CardIdentity
    {
        private static Dictionary<int, CardIdentity> All = new Dictionary<int, CardIdentity>(100);

        public readonly int id;                 // id of the card (playerId << 16 | localId)
        public Data.CardModel model;            // card type
        public ulong key;                       // (nonce << 32 | type), either encrypted, or not
        public bool alias;                      // if alias is true, the type in key is not a card type but another CardIdentity to substitue
        public bool mySecret;                   // if mySecret is true, key is not encrypted, and has not been made publicly available yet
        public bool revealing;                  // true if the card is being - or has been - revealed
        public bool inUse;                      // if true, this cardidentity is currently linked to a card's Type property
        public bool visible;                    // indicates if a card is face up during a shuffle [transient]

        public event EventHandler<RevealEventArgs> Revealed;

        public CardIdentity(int id)
        {
            this.id = id;
            All.Add(id, this);
        }

        public static CardIdentity Find(int id)
        {
            CardIdentity res;
            bool success = All.TryGetValue(id, out res);
            return success ? res : null;
        }

        public static void Delete(int id)
        { All.Remove(id); }

        public static void Reset()
        { All.Clear(); }

        public override string ToString()
        {
            return model == null ? "Card" : model.Name;
        }

        public void OnRevealed(CardIdentity newId)
        {
            if (Revealed != null)
                Revealed(this, new RevealEventArgs() { NewIdentity = newId });
        }
    }

    public class RevealEventArgs : EventArgs
    {
        public CardIdentity NewIdentity { get; set; }
    }

    internal class CardIdentityNamer
    {
        public Octgn.Play.Gui.CardRun Target { get; set; }

        public void Rename(object sender, RevealEventArgs e)
        {
            var id = (CardIdentity)sender;
            id.Revealed -= Rename;
            var newId = e.NewIdentity;
            if (newId.model != null)
                Target.SetCardModel(newId.model);
            else
                newId.Revealed += Rename;
        }
    }
}