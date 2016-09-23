using System.Collections.Generic;
using Octgn.Core.DataExtensionMethods;

namespace Octgn.Play
{

    public class CardIdentity
    {
        private static readonly Dictionary<ulong, CardIdentity> All = new Dictionary<ulong, CardIdentity>(100);

        public readonly ulong Id; // id of the card (playerId << 16 | localId)
        public bool InUse; // if true, this cardidentity is currently linked to a card's Type property
        public DataNew.Entities.Card Model; // card type
        public bool Visible; // indicates if a card is face up during a shuffle [transient]

        public CardIdentity(ulong id)
        {
            Id = id;
            lock (All)
            {
                if (All.ContainsKey(id)) All[id] = this;
                else All.Add(id, this);
            }
        }

        public static CardIdentity Find(ulong id)
        {
            CardIdentity res;
            lock (All)
            {
                bool success = All.TryGetValue(id, out res);
                return success ? res : null;
            }
        }

        public static void Delete(ulong id)
        {
            lock(All)
                All.Remove(id);
        }

        public static void Reset()
        {
            lock(All)
                All.Clear();
        }

        public override string ToString()
        {
            return Model == null ? "Card" : Model.PropertyName();
        }
    }
}