using System;
using Octgn.Data;

namespace Octgn.Play.Actions
{
    using Octgn.Core;
    using Octgn.Core.Play;

    public class CreateCard : ActionBase
    {
        private readonly bool _deletesWhenLeavesGroup;
        private readonly bool _faceUp;
        private readonly int _id;
        private readonly ulong _key;
        private readonly DataNew.Entities.Card _model;
        private readonly IPlayPlayer _owner;
        private readonly int _x;
        private readonly int _y;
        internal IPlayCard Card;

        public CreateCard(IPlayPlayer owner, int id, ulong key, bool faceUp, DataNew.Entities.Card model, int x, int y,
                          bool deletesWhenLeavesGroup)
        {
            _owner = owner;
            _id = id;
            _key = key;
            _faceUp = faceUp;
            _deletesWhenLeavesGroup = deletesWhenLeavesGroup;
            _model = model;
            _x = x;
            _y = y;
        }

        internal static event EventHandler Done;

        public override void Do()
        {
            base.Do();
            Card = K.C.Get<IObjectCreator>().CreateCard(_owner, _id, _key, _model, false);
            Card.X = _x;
            Card.Y = _y;
            Card.DeleteWhenLeavesGroup = _deletesWhenLeavesGroup;
            Card.SetFaceUp(_faceUp);
            K.C.Get<IGameEngine>().Table.AddAt(Card, K.C.Get<IGameEngine>().Table.Count);

            if (Done != null) Done(this, EventArgs.Empty);
        }
    }
}