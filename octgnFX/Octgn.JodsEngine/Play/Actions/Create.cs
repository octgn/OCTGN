using System;
using Octgn.Data;

namespace Octgn.Play.Actions
{
    public class CreateCard : ActionBase
    {
        private readonly bool _deletesWhenLeavesGroup;
        private readonly bool _faceUp;
        private readonly int _id;
        private readonly DataNew.Entities.Card _model;
        private readonly Player _owner;
        private readonly int _x;
        private readonly int _y;
        internal Card Card;

        public CreateCard(Player owner, int id,bool faceUp, DataNew.Entities.Card model, int x, int y,
                          bool deletesWhenLeavesGroup)
        {
            _owner = owner;
            _id = id;
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
                new Card(_owner, _id, _model, false,_model.Size.Name)                    {X = _x, Y = _y, DeleteWhenLeavesGroup = _deletesWhenLeavesGroup};
            Card.SetFaceUp(_faceUp);
            Program.GameEngine.Table.AddAt(Card, Program.GameEngine.Table.Count);

            if (Done != null) Done(this, EventArgs.Empty);
        }
    }
}