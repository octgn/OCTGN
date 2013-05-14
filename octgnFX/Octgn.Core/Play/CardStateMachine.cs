namespace Octgn.Core.Play
{
    using System.Collections.Generic;

    using Octgn.Play;

    public class CardStateMachine
    {
        public readonly Dictionary<int, IPlayCard> All = new Dictionary<int, IPlayCard>();

        public string DefaultFront
        {
            get { return K.C.Get<IGameEngine>().Definition.CardFront; }
        }

        public string DefaultBack
        {
            get { return K.C.Get<IGameEngine>().Definition.CardBack; }
        }

        internal IPlayCard Find(int id)
        {
            IPlayCard res;
            bool success = All.TryGetValue(id, out res);
            return success ? res : null;
        }

        internal void Reset()
        {
            All.Clear();
        }

    }
}