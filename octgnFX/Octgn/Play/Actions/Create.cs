using System;
using Octgn.Data;

namespace Octgn.Play.Actions
{
    public class CreateCard : ActionBase
    {
        private readonly bool _deletesWhenLeavesGroup;
        private readonly bool _faceUp;
        private readonly int _id;
        private readonly ulong _key;
        private readonly CardModel _model;
        private readonly Player _owner;
        private readonly int _x;
        private readonly int _y;
        internal Card Card;

        public CreateCard(Player owner, int id, ulong key, bool faceUp, CardModel model, int x, int y,
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

            Card =
                new Card(_owner, _id, _key, Program.Game.Definition.CardDefinition, _faceUp ? _model : null, false)
                    {X = _x, Y = _y, DeleteWhenLeavesGroup = _deletesWhenLeavesGroup};
            Card.SetFaceUp(_faceUp);
            Program.Game.Table.AddAt(Card, Program.Game.Table.Count);

            if (Done != null) Done(this, EventArgs.Empty);
        }
    }
    public class CreateCardInGroup : ActionBase
    {
        private readonly int _id;
        private readonly ulong _key;
        private readonly CardModel _model;
        private readonly Player _owner;
        private readonly Group _group;
        internal Card Card;


        public CreateCardInGroup(Player owner, int id, ulong key, CardModel model, Group group)
        {
            _owner = owner;
            _id = id;
            _key = key;
            _model = model;
        }

        internal static event EventHandler Done;

        public override void Do()
        {
            base.Do();

            Card = new Card(_owner, _id, _key, Program.Game.Definition.CardDefinition, _model, false);
            Card.SetFaceUp(true);
            _group.AddAt(Card,_group.Count);

            if (Done != null) Done(this, EventArgs.Empty);
        }
    }
}